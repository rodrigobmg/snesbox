#if !FAST_PPU
using System;
using Nall;

namespace Snes
{
    partial class PPU
    {
        private partial class Screen
        {
            public PPU self;
            public ArraySegment<ushort> output;

            public Regs regs = new Regs();

            public void scanline()
            {
                output = new ArraySegment<ushort>(self.output.Array, self.output.Offset + self.PPUCounter.vcounter() * 1024, self.output.Count - self.PPUCounter.vcounter() * 1024);
                if (self.display.interlace && self.PPUCounter.field())
                {
                    output = new ArraySegment<ushort>(output.Array, output.Offset + 512, output.Count - 512);
                }
            }

            public void run()
            {
                ushort color;
                int outputOffset = 0;
                if (self.regs.pseudo_hires == false && self.regs.bgmode != 5 && self.regs.bgmode != 6)
                {
                    color = get_pixel(false);
                    output.Array[output.Offset + outputOffset++] = color;
                    output.Array[output.Offset + outputOffset++] = color;
                }
                else
                {
                    color = get_pixel(true);
                    output.Array[output.Offset + outputOffset++] = color;
                    color = get_pixel(false);
                    output.Array[output.Offset + outputOffset++] = color;
                }

                output = new ArraySegment<ushort>(output.Array, output.Offset + outputOffset, output.Count - outputOffset);
            }

            public void reset()
            {
                regs.addsub_mode = Convert.ToBoolean(0);
                regs.direct_color = Convert.ToBoolean(0);
                regs.color_mode = Convert.ToBoolean(0);
                regs.color_halve = Convert.ToBoolean(0);
                regs.bg1_color_enable = Convert.ToBoolean(0);
                regs.bg2_color_enable = Convert.ToBoolean(0);
                regs.bg3_color_enable = Convert.ToBoolean(0);
                regs.bg4_color_enable = Convert.ToBoolean(0);
                regs.oam_color_enable = Convert.ToBoolean(0);
                regs.back_color_enable = Convert.ToBoolean(0);
                regs.color_r = 0;
                regs.color_g = 0;
                regs.color_b = 0;
            }

            public void serialize(Serializer s)
            {
                s.integer(regs.addsub_mode, "regs.addsub_mode");
                s.integer(regs.direct_color, "regs.direct_color");

                s.integer(regs.color_mode, "regs.color_mode");
                s.integer(regs.color_halve, "regs.color_halve");
                s.integer(regs.bg1_color_enable, "regs.bg1_color_enable");
                s.integer(regs.bg2_color_enable, "regs.bg2_color_enable");
                s.integer(regs.bg3_color_enable, "regs.bg3_color_enable");
                s.integer(regs.bg4_color_enable, "regs.bg4_color_enable");
                s.integer(regs.oam_color_enable, "regs.oam_color_enable");
                s.integer(regs.back_color_enable, "regs.back_color_enable");

                s.integer(regs.color_b, "regs.color_b");
                s.integer(regs.color_g, "regs.color_g");
                s.integer(regs.color_r, "regs.color_r");
            }

            public Screen(PPU self_)
            {
                self = self_;
                for (uint l = 0; l < 16; l++)
                {
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
                                light_table[l, (r << 10) + (g << 5) + b] = (ushort)((ab << 10) + (ag << 5) + ar);
                            }
                        }
                    }
                }
            }

            private ushort[,] light_table = new ushort[16, 32768];

            enum Source { BG1, BG2, BG3, BG4, OAM, BACK }

            private ushort get_pixel(bool swap)
            {
                bool[] color_enable = { regs.bg1_color_enable, regs.bg2_color_enable, regs.bg3_color_enable, regs.bg4_color_enable, regs.oam_color_enable, regs.back_color_enable };

                //===========
                //main screen
                //===========

                uint priority_main = 0;
                uint color_main = 0;
                uint source_main = 0;

                if (Convert.ToBoolean(self.bg1.output.main.priority))
                {
                    priority_main = self.bg1.output.main.priority;
                    if (regs.direct_color && (self.regs.bgmode == 3 || self.regs.bgmode == 4 || self.regs.bgmode == 7))
                    {
                        color_main = get_direct_color(self.bg1.output.main.palette, self.bg1.output.main.tile);
                    }
                    else
                    {
                        color_main = get_color(self.bg1.output.main.palette);
                    }
                    source_main = (uint)Source.BG1;
                }
                if (self.bg2.output.main.priority > priority_main)
                {
                    priority_main = self.bg2.output.main.priority;
                    color_main = get_color(self.bg2.output.main.palette);
                    source_main = (uint)Source.BG2;
                }
                if (self.bg3.output.main.priority > priority_main)
                {
                    priority_main = self.bg3.output.main.priority;
                    color_main = get_color(self.bg3.output.main.palette);
                    source_main = (uint)Source.BG3;
                }
                if (self.bg4.output.main.priority > priority_main)
                {
                    priority_main = self.bg4.output.main.priority;
                    color_main = get_color(self.bg4.output.main.palette);
                    source_main = (uint)Source.BG4;
                }
                if (self.oam.output.main.priority > priority_main)
                {
                    priority_main = self.oam.output.main.priority;
                    color_main = get_color(self.oam.output.main.palette);
                    source_main = (uint)Source.OAM;
                }
                if (priority_main == 0)
                {
                    color_main = get_color(0);
                    source_main = (uint)Source.BACK;
                }

                //==========
                //sub screen
                //==========

                uint priority_sub = 0;
                uint color_sub = 0;
                uint source_sub = 0;

                if (Convert.ToBoolean(self.bg1.output.sub.priority))
                {
                    priority_sub = self.bg1.output.sub.priority;
                    if (regs.direct_color && (self.regs.bgmode == 3 || self.regs.bgmode == 4 || self.regs.bgmode == 7))
                    {
                        color_sub = get_direct_color(self.bg1.output.sub.palette, self.bg1.output.sub.tile);
                    }
                    else
                    {
                        color_sub = get_color(self.bg1.output.sub.palette);
                    }
                    source_sub = (uint)Source.BG1;
                }
                if (self.bg2.output.sub.priority > priority_sub)
                {
                    priority_sub = self.bg2.output.sub.priority;
                    color_sub = get_color(self.bg2.output.sub.palette);
                    source_sub = (uint)Source.BG2;
                }
                if (self.bg3.output.sub.priority > priority_sub)
                {
                    priority_sub = self.bg3.output.sub.priority;
                    color_sub = get_color(self.bg3.output.sub.palette);
                    source_sub = (uint)Source.BG3;
                }
                if (self.bg4.output.sub.priority > priority_sub)
                {
                    priority_sub = self.bg4.output.sub.priority;
                    color_sub = get_color(self.bg4.output.sub.palette);
                    source_sub = (uint)Source.BG4;
                }
                if (self.oam.output.sub.priority > priority_sub)
                {
                    priority_sub = self.oam.output.sub.priority;
                    color_sub = get_color(self.oam.output.sub.palette);
                    source_sub = (uint)Source.OAM;
                }
                if (priority_sub == 0)
                {
                    if (self.regs.pseudo_hires == true || self.regs.bgmode == 5 || self.regs.bgmode == 6)
                    {
                        color_sub = get_color(0);
                    }
                    else
                    {
                        color_sub = (uint)((regs.color_b << 10) + (regs.color_g << 5) + (regs.color_r << 0));
                    }
                    source_sub = (uint)Source.BACK;
                }

                if (swap == true)
                {
                    Utility.Swap(ref priority_main, ref priority_sub);
                    Utility.Swap(ref color_main, ref color_sub);
                    Utility.Swap(ref source_main, ref source_sub);
                }

                ushort output;
                if (!regs.addsub_mode)
                {
                    source_sub = (uint)Source.BACK;
                    color_sub = (uint)((regs.color_b << 10) + (regs.color_g << 5) + (regs.color_r << 0));
                }

                if (self.window.output.main.color_enable == false)
                {
                    if (self.window.output.sub.color_enable == false)
                    {
                        return 0x0000;
                    }
                    color_main = 0x0000;
                }

                bool color_exempt = (source_main == (uint)Source.OAM && self.oam.output.main.palette < 192);
                if (!color_exempt && color_enable[source_main] && self.window.output.sub.color_enable)
                {
                    bool halve = false;
                    if (regs.color_halve && self.window.output.main.color_enable)
                    {
                        if (!regs.addsub_mode || source_sub != (uint)Source.BACK)
                        {
                            halve = true;
                        }
                    }
                    output = addsub(color_main, color_sub, halve);
                }
                else
                {
                    output = (ushort)color_main;
                }

                //========
                //lighting
                //========

                output = light_table[self.regs.display_brightness, output];
                if (self.regs.display_disabled)
                {
                    output = 0x0000;
                }
                return output;
            }

            private ushort addsub(uint x, uint y, bool halve)
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

            private ushort get_color(uint palette)
            {
                palette <<= 1;
                return (ushort)(StaticRAM.cgram[palette + 0] + (StaticRAM.cgram[palette + 1] << 8));
            }

            private ushort get_direct_color(uint palette, uint tile)
            { 	//palette = -------- BBGGGRRR
                //tile    = ---bgr-- --------
                //output  = 0BBb00GG Gg0RRRr0
                return (ushort)(((palette << 7) & 0x6000) + ((tile >> 0) & 0x1000)
                    + ((palette << 4) & 0x0380) + ((tile >> 5) & 0x0040)
                    + ((palette << 2) & 0x001c) + ((tile >> 9) & 0x0002));
            }
        }
    }
}
#endif