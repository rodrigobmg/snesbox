#if PERFORMANCE
using System;
using System.Linq;
using Nall;

namespace Snes
{
    partial class PPU
    {
        private partial class Screen
        {
            public Regs regs = new Regs();
            public Output output = new Output();

            public ColorWindow window = new ColorWindow();
            public ushort[][] light_table;

            public uint get_palette(uint color)
            {
                return BitConverter.ToUInt16(StaticRAM.cgram.data(), (int)(color) * 2);
            }

            public uint get_direct_color(uint palette, uint tile)
            {
                return ((tile & 7) << 2) | ((palette & 1) << 1) |
                    (((tile >> 3) & 7) << 7) | (((palette >> 1) & 1) << 6) |
                    ((tile >> 6) << 13) | ((palette >> 2) << 12);
            }

            public ushort addsub(uint x, uint y, bool halve)
            {
                if (!regs.color_mode)
                {
                    if (!halve)
                    {
                        uint sum = x + y;
                        uint carry = (sum - ((x ^ y) & 0x0421)) & 0x8420;
                        return (ushort)((sum - carry) | (carry - (carry >> 5)));
                    }
                    else
                    {
                        return (ushort)((x + y - ((x ^ y) & 0x0421)) >> 1);
                    }
                }
                else
                {
                    uint diff = x - y + 0x8420;
                    uint borrow = (diff - ((x ^ y) & 0x8420)) & 0x8420;
                    if (!halve)
                    {
                        return (ushort)((diff - borrow) & (borrow - (borrow >> 5)));
                    }
                    else
                    {
                        return (ushort)((((diff - borrow) & (borrow - (borrow >> 5))) & 0x7bde) >> 1);
                    }
                }
            }

            public void scanline()
            {
                uint main_color = get_palette(0);
                uint sub_color = (self.regs.pseudo_hires == false && self.regs.bgmode != 5 && self.regs.bgmode != 6)
                                   ? regs.color : main_color;

                for (uint x = 0; x < 256; x++)
                {
                    output.main[x].color = main_color;
                    output.main[x].priority = 0;
                    output.main[x].source = 6;

                    output.sub[x].color = sub_color;
                    output.sub[x].priority = 0;
                    output.sub[x].source = 6;
                }

                window.render(Convert.ToBoolean(0));
                window.render(Convert.ToBoolean(1));
            }

            public void render_black()
            {
                ArraySegment<ushort> data = new ArraySegment<ushort>(self.output.Array, self.output.Offset + (self.PPUCounter.vcounter() * 1024), self.output.Count - (self.PPUCounter.vcounter() * 1024));
                if (self.interlace() && self.PPUCounter.field())
                {
                    data = new ArraySegment<ushort>(data.Array, data.Offset + 512, data.Count - 512);
                }
                Array.Copy(Enumerable.Repeat((ushort)0, (int)(self.display.width << 1)).ToArray(), 0, data.Array, data.Offset, (int)(self.display.width << 1));
            }

            public ushort get_pixel_main(uint x)
            {
                var main = output.main[x];
                var sub = output.sub[x];

                if (!regs.addsub_mode)
                {
                    sub.source = 6;
                    sub.color = regs.color;
                }

                if (!Convert.ToBoolean(window.main[x]))
                {
                    if (!Convert.ToBoolean(window.sub[x]))
                    {
                        return 0x0000;
                    }
                    main.color = 0x0000;
                }

                if (main.source != 5 && regs.color_enable[main.source] && Convert.ToBoolean(window.sub[x]))
                {
                    bool halve = false;
                    if (regs.color_halve && Convert.ToBoolean(window.main[x]))
                    {
                        if (!regs.addsub_mode || sub.source != 6)
                        {
                            halve = true;
                        }
                    }
                    return addsub(main.color, sub.color, halve);
                }

                return (ushort)main.color;
            }

            public ushort get_pixel_sub(uint x)
            {
                var main = output.sub[x];
                var sub = output.main[x];

                if (!regs.addsub_mode)
                {
                    sub.source = 6;
                    sub.color = regs.color;
                }

                if (!Convert.ToBoolean(window.main[x]))
                {
                    if (!Convert.ToBoolean(window.sub[x]))
                    {
                        return 0x0000;
                    }
                    main.color = 0x0000;
                }

                if (main.source != 5 && regs.color_enable[main.source] && Convert.ToBoolean(window.sub[x]))
                {
                    bool halve = false;
                    if (regs.color_halve && Convert.ToBoolean(window.main[x]))
                    {
                        if (!regs.addsub_mode || sub.source != 6)
                        {
                            halve = true;
                        }
                    }
                    return addsub(main.color, sub.color, halve);
                }

                return (ushort)main.color;
            }

            public void render()
            {
                ArraySegment<ushort> data = new ArraySegment<ushort>(self.output.Array, self.output.Offset + (self.PPUCounter.vcounter() * 1024), self.output.Count - (self.PPUCounter.vcounter() * 1024));
                if (self.interlace() && self.PPUCounter.field())
                {
                    data = new ArraySegment<ushort>(data.Array, data.Offset + 512, data.Count - 512);
                }
                ushort[] light = light_table[self.regs.display_brightness];

                if (!self.regs.pseudo_hires && self.regs.bgmode != 5 && self.regs.bgmode != 6)
                {
                    for (uint i = 0; i < 256; i++)
                    {
                        data.Array[data.Offset + i] = light[get_pixel_main(i)];
                    }
                }
                else
                {
                    int arrayIndex = 0;
                    for (uint i = 0; i < 256; i++)
                    {
                        data.Array[data.Offset + arrayIndex++] = light[get_pixel_sub(i)];
                        data.Array[data.Offset + arrayIndex++] = light[get_pixel_main(i)];
                    }
                }
            }

            public void serialize(Serializer s)
            {
                s.integer(regs.addsub_mode, "regs.addsub_mode");
                s.integer(regs.direct_color, "regs.direct_color");

                s.integer(regs.color_mode, "regs.color_mode");
                s.integer(regs.color_halve, "regs.color_halve");
                s.array(regs.color_enable, "regs.color_enable");

                s.integer(regs.color_b, "regs.color_b");
                s.integer(regs.color_g, "regs.color_g");
                s.integer(regs.color_r, "regs.color_r");
                s.integer(regs.color, "regs.color");

                for (uint i = 0; i < 256; i++)
                {
                    s.integer(output.main[i].color, "output.main[i].color");
                    s.integer(output.main[i].priority, "output.main[i].priority");
                    s.integer(output.main[i].source, "output.main[i].source");

                    s.integer(output.sub[i].color, "output.sub[i].color");
                    s.integer(output.sub[i].priority, "output.sub[i].priority");
                    s.integer(output.sub[i].source, "output.sub[i].source");
                }

                window.serialize(s);
            }

            public Screen(PPU self)
            {
                this.self = self;

                light_table = new ushort[16][];
                for (uint l = 0; l < 16; l++)
                {
                    light_table[l] = new ushort[32768];
                    for (uint r = 0; r < 32; r++)
                    {
                        for (uint g = 0; g < 32; g++)
                        {
                            for (uint b = 0; b < 32; b++)
                            {
                                double luma = (double)l / 15.0;
                                uint ar = (uint)(luma * r + 0.5);
                                uint ag = (uint)(luma * g + 0.5);
                                uint ab = (uint)(luma * b + 0.5);
                                light_table[l][(r << 10) + (g << 5) + (b << 0)] = (ushort)((ab << 10) + (ag << 5) + (ar << 0));
                            }
                        }
                    }
                }
            }

            public PPU self;
        }
    }
}
#endif