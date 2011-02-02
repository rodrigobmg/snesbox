#if !FAST_PPU
using System;
using Nall;

namespace Snes
{
    partial class PPU
    {
        private partial class Background
        {
            public PPU self;
            public enum ID { BG1, BG2, BG3, BG4 }
            public uint id;

            public enum Mode { BPP2, BPP4, BPP8, Mode7, Inactive }
            public enum ScreenSize { Size32x32, Size32x64, Size64x32, Size64x64 }
            public enum TileSize { Size8x8, Size16x16 }

            public T t = new T();
            public Regs regs = new Regs();
            public Output output = new Output();

            public void scanline()
            {
                if (self.PPUCounter.vcounter() == 1)
                {
                    t.mosaic_y = 1;
                    t.mosaic_countdown = 0;
                }
                else
                {
                    if (!Convert.ToBoolean(regs.mosaic) || !Convert.ToBoolean(t.mosaic_countdown))
                    {
                        t.mosaic_y = self.PPUCounter.vcounter();
                    }
                    if (!Convert.ToBoolean(t.mosaic_countdown))
                    {
                        t.mosaic_countdown = regs.mosaic + 1;
                    }
                    t.mosaic_countdown--;
                }

                t.x = 0;
            }

            public void run()
            {
                bool hires = (self.regs.bgmode == 5 || self.regs.bgmode == 6);

                if ((self.PPUCounter.hcounter() & 2) == 0)
                {
                    output.main.priority = 0;
                    output.sub.priority = 0;
                }
                else if (hires == false)
                {
                    return;
                }

                if (regs.mode == (int)Mode.Inactive)
                {
                    return;
                }
                if (regs.main_enabled == false && regs.sub_enabled == false)
                {
                    return;
                }

                uint x = t.x++;
                uint y = t.mosaic_y;
                if (regs.mode == (int)Mode.Mode7)
                {
                    run_mode7(x, y);
                    return;
                }

                uint color_depth = (regs.mode == (int)Mode.BPP2 ? 0U : regs.mode == (int)Mode.BPP4 ? 1U : 2U);
                uint palette_offset = (self.regs.bgmode == 0 ? (id << 5) : 0);
                uint palette_size = (2U << (int)color_depth);
                uint tile_mask = (0x0fffU >> (int)color_depth);
                uint tiledata_index = regs.tiledata_addr >> (int)(4 + color_depth);

                uint tile_height = (Convert.ToInt32(regs.tile_size) == (int)TileSize.Size8x8 ? 3U : 4U);
                uint tile_width = (!hires ? tile_height : 4);

                uint width = (!hires ? 256U : 512U);
                uint mask_x = (tile_height == 3 ? width : (width << 1));
                uint mask_y = mask_x;
                if (Convert.ToBoolean(regs.screen_size & 1))
                {
                    mask_x <<= 1;
                }
                if (Convert.ToBoolean(regs.screen_size & 2))
                {
                    mask_y <<= 1;
                }
                mask_x--;
                mask_y--;

                uint hscroll = regs.hoffset;
                uint vscroll = regs.voffset;
                if (hires)
                {
                    hscroll <<= 1;
                    if (self.regs.interlace)
                    {
                        y = (uint)((y << 1) + Convert.ToInt32(self.PPUCounter.field()));
                    }
                }

                uint hoffset = hscroll + mosaic_table[regs.mosaic, x];
                uint voffset = vscroll + y;

                if (self.regs.bgmode == 2 || self.regs.bgmode == 4 || self.regs.bgmode == 6)
                {
                    ushort opt_x = (ushort)(x + (hscroll & 7));

                    if (opt_x >= 8)
                    {
                        uint hval = self.bg3.get_tile((uint)((opt_x - 8) + (self.bg3.regs.hoffset & ~7)), self.bg3.regs.voffset + 0);
                        uint vval = self.bg3.get_tile((uint)((opt_x - 8) + (self.bg3.regs.hoffset & ~7)), self.bg3.regs.voffset + 8U);
                        uint opt_valid_bit = (id == (int)ID.BG1 ? (uint)0x2000 : (uint)0x4000);

                        if (self.regs.bgmode == 4)
                        {
                            if (Convert.ToBoolean(hval & opt_valid_bit))
                            {
                                if (!Convert.ToBoolean(hval & 0x8000))
                                {
                                    hoffset = (uint)(opt_x + (hval & ~7));
                                }
                                else
                                {
                                    voffset = y + hval;
                                }
                            }
                        }
                        else
                        {
                            if (Convert.ToBoolean(hval & opt_valid_bit))
                            {
                                hoffset = (uint)(opt_x + (hval & ~7));
                            }
                            if (Convert.ToBoolean(vval & opt_valid_bit))
                            {
                                voffset = y + vval;
                            }
                        }
                    }
                }

                hoffset &= mask_x;
                voffset &= mask_y;

                uint tile_number = get_tile(hoffset, voffset);
                bool mirror_y = Convert.ToBoolean(tile_number & 0x8000);
                bool mirror_x = Convert.ToBoolean(tile_number & 0x4000);
                uint priority = (Convert.ToBoolean(tile_number & 0x2000) ? regs.priority1 : regs.priority0);
                uint palette_number = (tile_number >> 10) & 7;
                uint palette_index = palette_offset + (palette_number << (int)palette_size);

                if (tile_width == 4 && Convert.ToBoolean(hoffset & 8) != mirror_x)
                {
                    tile_number += 1;
                }
                if (tile_height == 4 && Convert.ToBoolean(voffset & 8) != mirror_y)
                {
                    tile_number += 16;
                }
                tile_number &= 0x03ff;
                tile_number += tiledata_index;
                tile_number &= tile_mask;

                if (mirror_x)
                {
                    hoffset ^= 7;
                }
                if (mirror_y)
                {
                    voffset ^= 7;
                }

                byte color = (byte)get_color(hoffset, voffset, (ushort)tile_number);
                if (color == 0)
                {
                    return;
                }

                color += (byte)palette_index;

                if (hires == false)
                {
                    if (regs.main_enabled)
                    {
                        output.main.priority = priority;
                        output.main.palette = color;
                        output.main.tile = tile_number;
                    }

                    if (regs.sub_enabled)
                    {
                        output.sub.priority = priority;
                        output.sub.palette = color;
                        output.sub.tile = tile_number;
                    }
                }
                else
                {
                    if (Convert.ToBoolean(x & 1))
                    {
                        if (regs.main_enabled)
                        {
                            output.main.priority = priority;
                            output.main.palette = color;
                            output.main.tile = tile_number;
                        }
                    }
                    else
                    {
                        if (regs.sub_enabled)
                        {
                            output.sub.priority = priority;
                            output.sub.palette = color;
                            output.sub.tile = tile_number;
                        }
                    }
                }
            }

            public uint get_tile(uint x, uint y)
            {
                bool hires = (self.regs.bgmode == 5 || self.regs.bgmode == 6);
                uint tile_height = (Convert.ToInt32(regs.tile_size) == (int)TileSize.Size8x8 ? 3U : 4U);
                uint tile_width = (!hires ? tile_height : 4);
                uint width = (!hires ? 256U : 512U);
                uint mask_x = (tile_height == 3 ? width : (width << 1));
                uint mask_y = mask_x;
                if (Convert.ToBoolean(regs.screen_size & 1))
                {
                    mask_x <<= 1;
                }
                if (Convert.ToBoolean(regs.screen_size & 2))
                {
                    mask_y <<= 1;
                }
                mask_x--;
                mask_y--;

                uint screen_x = (Convert.ToBoolean(regs.screen_size & 1) ? (32U << 5) : 0U);
                uint screen_y = (Convert.ToBoolean(regs.screen_size & 2) ? (32U << 5) : 0U);
                if (regs.screen_size == 3)
                {
                    screen_y <<= 1;
                }

                x = (x & mask_x) >> (int)tile_width;
                y = (y & mask_y) >> (int)tile_height;

                ushort pos = (ushort)(((y & 0x1f) << 5) + (x & 0x1f));
                if (Convert.ToBoolean(x & 0x20))
                {
                    pos += (ushort)screen_x;
                }
                if (Convert.ToBoolean(y & 0x20))
                {
                    pos += (ushort)screen_y;
                }

                ushort addr = (ushort)(regs.screen_addr + (pos << 1));
                return (uint)(StaticRAM.vram[addr + 0U] + (StaticRAM.vram[addr + 1U] << 8));
            }

            public uint get_color(uint x, uint y, ushort offset)
            {
                uint mask = (uint)(0x80 >> (int)(x & 7));

                switch ((Background.Mode)regs.mode)
                {
                    case Background.Mode.BPP2:
                        {
                            offset = (ushort)((offset * 16) + ((y & 7) * 2));

                            uint d0 = StaticRAM.vram[offset + 0U];
                            uint d1 = StaticRAM.vram[offset + 1U];

                            return ((Bit.ToBit(d0 & mask)) << 0)
                                + ((Bit.ToBit(d1 & mask)) << 1);
                        }
                    case Background.Mode.BPP4:
                        {
                            offset = (ushort)((offset * 32) + ((y & 7) * 2));

                            uint d0 = StaticRAM.vram[offset + 0U];
                            uint d1 = StaticRAM.vram[offset + 1U];
                            uint d2 = StaticRAM.vram[offset + 16U];
                            uint d3 = StaticRAM.vram[offset + 17U];

                            return ((Bit.ToBit(d0 & mask)) << 0)
                                + ((Bit.ToBit(d1 & mask)) << 1)
                                + ((Bit.ToBit(d2 & mask)) << 2)
                                + ((Bit.ToBit(d3 & mask)) << 3);
                        }
                    case Background.Mode.BPP8:
                        {
                            offset = (ushort)((offset * 64) + ((y & 7) * 2));

                            uint d0 = StaticRAM.vram[offset + 0U];
                            uint d1 = StaticRAM.vram[offset + 1U];
                            uint d2 = StaticRAM.vram[offset + 16U];
                            uint d3 = StaticRAM.vram[offset + 17U];
                            uint d4 = StaticRAM.vram[offset + 32U];
                            uint d5 = StaticRAM.vram[offset + 33U];
                            uint d6 = StaticRAM.vram[offset + 48U];
                            uint d7 = StaticRAM.vram[offset + 49U];

                            return ((Bit.ToBit(d0 & mask)) << 0)
                                + ((Bit.ToBit(d1 & mask)) << 1)
                                + ((Bit.ToBit(d2 & mask)) << 2)
                                + ((Bit.ToBit(d3 & mask)) << 3)
                                + ((Bit.ToBit(d4 & mask)) << 4)
                                + ((Bit.ToBit(d5 & mask)) << 5)
                                + ((Bit.ToBit(d6 & mask)) << 6)
                                + ((Bit.ToBit(d7 & mask)) << 7);
                        }
                    default:
                        throw new InvalidOperationException();
                }
            }

            public void reset()
            {
                t.x = 0;
                t.mosaic_y = 0;
                t.mosaic_countdown = 0;
                regs.tiledata_addr = 0;
                regs.screen_addr = 0;
                regs.screen_size = 0;
                regs.mosaic = 0;
                regs.tile_size = Convert.ToBoolean(0);
                regs.mode = 0;
                regs.priority0 = 0;
                regs.priority1 = 0;
                regs.main_enabled = Convert.ToBoolean(0);
                regs.sub_enabled = Convert.ToBoolean(0);
                regs.hoffset = 0;
                regs.voffset = 0;
                output.main.palette = 0;
                output.main.priority = 0;
                output.sub.palette = 0;
                output.sub.priority = 0;
            }

            public void serialize(Serializer s)
            {
                s.integer(id, "id");

                s.integer(t.x, "t.x");
                s.integer(t.mosaic_y, "t.mosaic_y");
                s.integer(t.mosaic_countdown, "t.mosaic_countdown");

                s.integer(regs.tiledata_addr, "regs.tiledata_addr");
                s.integer(regs.screen_addr, "regs.screen_addr");
                s.integer(regs.screen_size, "regs.screen_size");
                s.integer(regs.mosaic, "regs.mosaic");
                s.integer(regs.tile_size, "regs.tile_size");

                s.integer(regs.mode, "regs.mode");
                s.integer(regs.priority0, "regs.priority0");
                s.integer(regs.priority1, "regs.priority1");

                s.integer(regs.main_enabled, "regs.main_enabled");
                s.integer(regs.sub_enabled, "regs.sub_enabled");

                s.integer(regs.hoffset, "regs.hoffset");
                s.integer(regs.voffset, "regs.voffset");

                s.integer(output.main.priority, "output.main.priority");
                s.integer(output.main.palette, "output.main.palette");
                s.integer(output.main.tile, "output.main.tile");

                s.integer(output.sub.priority, "output.sub.priority");
                s.integer(output.sub.palette, "output.sub.palette");
                s.integer(output.sub.tile, "output.sub.tile");
            }

            public Background(PPU self_, uint id_)
            {
                self = self_;
                id = id_;

                for (uint m = 0; m < 16; m++)
                {
                    for (uint x = 0; x < 4096; x++)
                    {
                        mosaic_table[m, x] = (ushort)((x / (m + 1)) * (m + 1));
                    }
                }
            }

            private static ushort[,] mosaic_table = new ushort[16, 4096];

            private int clip(int n)
            {   //13-bit sign extend: --s---nnnnnnnnnn -> ssssssnnnnnnnnnn
                return Convert.ToBoolean(n & 0x2000) ? (n | ~1023) : (n & 1023);
            }

            private void run_mode7(uint x, uint y)
            {
                int a = Bit.sclip(16, self.regs.m7a);
                int b = Bit.sclip(16, self.regs.m7b);
                int c = Bit.sclip(16, self.regs.m7c);
                int d = Bit.sclip(16, self.regs.m7d);

                int cx = Bit.sclip(13, self.regs.m7x);
                int cy = Bit.sclip(13, self.regs.m7y);
                int hoffset = Bit.sclip(13, self.regs.mode7_hoffset);
                int voffset = Bit.sclip(13, self.regs.mode7_voffset);

                if (self.regs.mode7_hflip)
                {
                    x = 255 - x;
                }
                if (self.regs.mode7_vflip)
                {
                    y = 255 - y;
                }

                uint mosaic_x = 0;
                uint mosaic_y = 0;
                if (id == (int)ID.BG1)
                {
                    mosaic_x = mosaic_table[self.bg1.regs.mosaic, x];
                    mosaic_y = mosaic_table[self.bg1.regs.mosaic, y];
                }
                else if (id == (int)ID.BG2)
                {
                    mosaic_x = mosaic_table[self.bg2.regs.mosaic, x];
                    mosaic_y = mosaic_table[self.bg1.regs.mosaic, y];  //BG2 vertical mosaic uses BG1 mosaic size
                }

                int psx = (int)(((a * clip(hoffset - cx)) & ~63) + ((b * clip(voffset - cy)) & ~63) + ((b * mosaic_y) & ~63) + (cx << 8));
                int psy = (int)(((c * clip(hoffset - cx)) & ~63) + ((d * clip(voffset - cy)) & ~63) + ((d * mosaic_y) & ~63) + (cy << 8));

                int px = (int)(psx + (a * mosaic_x));
                int py = (int)(psy + (c * mosaic_x));

                //mask pseudo-FP bits
                px >>= 8;
                py >>= 8;

                uint tile;
                uint palette = 0;
                switch (self.regs.mode7_repeat)
                {
                    //screen repetition outside of screen area
                    case 0:
                    case 1:
                        {
                            px &= 1023;
                            py &= 1023;
                            tile = StaticRAM.vram[(uint)(((py >> 3) * 128 + (px >> 3)) << 1)];
                            palette = StaticRAM.vram[(uint)((((tile << 6) + ((py & 7) << 3) + (px & 7)) << 1) + 1)];
                        }
                        break;
                    //palette color 0 outside of screen area
                    case 2:
                        {
                            if (px < 0 || px > 1023 || py < 0 || py > 1023)
                            {
                                palette = 0;
                            }
                            else
                            {
                                px &= 1023;
                                py &= 1023;
                                tile = StaticRAM.vram[(uint)(((py >> 3) * 128 + (px >> 3)) << 1)];
                                palette = StaticRAM.vram[(uint)((((tile << 6) + ((py & 7) << 3) + (px & 7)) << 1) + 1)];
                            }
                        }
                        break;
                    //character 0 repetition outside of screen area
                    case 3:
                        {
                            if (px < 0 || px > 1023 || py < 0 || py > 1023)
                            {
                                tile = 0;
                            }
                            else
                            {
                                px &= 1023;
                                py &= 1023;
                                tile = StaticRAM.vram[(uint)(((py >> 3) * 128 + (px >> 3)) << 1)];
                            }
                            palette = StaticRAM.vram[(uint)((((tile << 6) + ((py & 7) << 3) + (px & 7)) << 1) + 1)];
                        }
                        break;
                }

                uint priority = 0;
                if (id == (int)ID.BG1)
                {
                    priority = regs.priority0;
                }
                else if (id == (int)ID.BG2)
                {
                    priority = (Convert.ToBoolean(palette & 0x80) ? regs.priority1 : regs.priority0);
                    palette &= 0x7f;
                }

                if (palette == 0)
                {
                    return;
                }

                if (regs.main_enabled)
                {
                    output.main.palette = palette;
                    output.main.priority = priority;
                }

                if (regs.sub_enabled)
                {
                    output.sub.palette = palette;
                    output.sub.priority = priority;
                }
            }
        }
    }
}
#endif