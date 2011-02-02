#if ACCURACY
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
            public enum Screen { Main, Sub }

            public Regs regs = new Regs();
            public Output output = new Output();

            public int x;
            public int y;

            public uint mosaic_vcounter;
            public uint mosaic_voffset;
            public uint mosaic_hcounter;
            public uint mosaic_hoffset;

            public uint mosaic_priority;
            public byte mosaic_palette;
            public ushort mosaic_tile;

            public uint tile_counter;
            public uint tile;
            public uint priority;
            public uint palette_number;
            public uint palette_index;
            public byte[] data = new byte[8];

            public void frame()
            {

            }

            public void scanline()
            {
                bool hires = (self.regs.bgmode == 5 || self.regs.bgmode == 6);
                x = -7;
                y = self.PPUCounter.vcounter();
                tile_counter = (7 - (regs.hoffset & 7)) << Convert.ToInt32(hires);
                for (uint n = 0; n < 8; n++) data[n] = 0;

                if (self.PPUCounter.vcounter() == 1)
                {
                    mosaic_vcounter = regs.mosaic + 1;
                    mosaic_voffset = 1;
                }
                else if (--mosaic_vcounter == 0)
                {
                    mosaic_vcounter = regs.mosaic + 1;
                    mosaic_voffset += regs.mosaic + 1;
                }

                mosaic_hcounter = regs.mosaic + 1;
                mosaic_hoffset = 0;
            }

            public void run(bool screen)
            {
                if (self.PPUCounter.vcounter() == 0)
                {
                    return;
                }
                bool hires = (self.regs.bgmode == 5 || self.regs.bgmode == 6);

                if (Convert.ToUInt32(screen) == (uint)Screen.Sub)
                {
                    output.main.priority = 0;
                    output.sub.priority = 0;
                    if (hires == false)
                    {
                        return;
                    }
                }

                if (regs.mode == (uint)Mode.Inactive)
                {
                    return;
                }
                if (regs.main_enable == false && regs.sub_enable == false)
                {
                    return;
                }

                if (regs.mode == (uint)Mode.Mode7)
                {
                    run_mode7();
                    return;
                }

                if (tile_counter-- == 0)
                {
                    tile_counter = 7;
                    get_tile();
                }

                byte palette = (byte)get_tile_color();
                if (x == 0)
                {
                    mosaic_hcounter = 1;
                }
                if (x >= 0 && --mosaic_hcounter == 0)
                {
                    mosaic_hcounter = regs.mosaic + 1;
                    mosaic_priority = priority;
                    mosaic_palette = (byte)(Convert.ToBoolean(palette) ? palette_index + palette : 0);
                    mosaic_tile = (ushort)tile;
                }
                if (Convert.ToUInt32(screen) == (uint)Screen.Main)
                {
                    x++;
                }
                if (mosaic_palette == 0)
                {
                    return;
                }

                if (hires == false)
                {
                    if (regs.main_enable)
                    {
                        output.main.priority = mosaic_priority;
                        output.main.palette = mosaic_palette;
                        output.main.tile = mosaic_tile;
                    }

                    if (regs.sub_enable)
                    {
                        output.sub.priority = mosaic_priority;
                        output.sub.palette = mosaic_palette;
                        output.sub.tile = mosaic_tile;
                    }
                }
                else if (Convert.ToUInt32(screen) == (uint)Screen.Main)
                {
                    if (regs.main_enable)
                    {
                        output.main.priority = mosaic_priority;
                        output.main.palette = mosaic_palette;
                        output.main.tile = mosaic_tile;
                    }
                }
                else if (Convert.ToUInt32(screen) == (uint)Screen.Sub)
                {
                    if (regs.sub_enable)
                    {
                        output.sub.priority = mosaic_priority;
                        output.sub.palette = mosaic_palette;
                        output.sub.tile = mosaic_tile;
                    }
                }
            }

            public void get_tile()
            {
                bool hires = (self.regs.bgmode == 5 || self.regs.bgmode == 6);

                uint color_depth = (uint)(regs.mode == (uint)Mode.BPP2 ? 0 : regs.mode == (uint)Mode.BPP4 ? 1 : 2);
                uint palette_offset = (self.regs.bgmode == 0 ? (id << 5) : 0);
                uint palette_size = (uint)(2 << (int)color_depth);
                uint tile_mask = (uint)(0x0fff >> (int)color_depth);
                uint tiledata_index = regs.tiledata_addr >> (int)(4 + color_depth);

                uint tile_height = (uint)(Convert.ToUInt32(regs.tile_size) == (uint)TileSize.Size8x8 ? 3 : 4);
                uint tile_width = (!hires ? tile_height : 4);

                uint width = (uint)(256 << Convert.ToInt32(hires));

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

                uint px = (uint)(x << Convert.ToInt32(hires));
                uint py = (regs.mosaic == 0 ? (uint)y : mosaic_voffset);

                uint hscroll = regs.hoffset;
                uint vscroll = regs.voffset;
                if (hires)
                {
                    hscroll <<= 1;
                    if (self.regs.interlace)
                    {
                        py = (uint)((py << 1) + Convert.ToInt32(self.PPUCounter.field()));
                    }
                }

                uint hoffset = hscroll + px;
                uint voffset = vscroll + py;

                if (self.regs.bgmode == 2 || self.regs.bgmode == 4 || self.regs.bgmode == 6)
                {
                    ushort offset_x = (ushort)(x + (hscroll & 7));

                    if (offset_x >= 8)
                    {
                        uint hval = self.bg3.get_tile((uint)((offset_x - 8) + (self.bg3.regs.hoffset & ~7)), self.bg3.regs.voffset + 0);
                        uint vval = self.bg3.get_tile((uint)((offset_x - 8) + (self.bg3.regs.hoffset & ~7)), self.bg3.regs.voffset + 8);
                        uint valid_mask = (uint)(id == (uint)ID.BG1 ? 0x2000 : 0x4000);

                        if (self.regs.bgmode == 4)
                        {
                            if (Convert.ToBoolean(hval & valid_mask))
                            {
                                if ((hval & 0x8000) == 0)
                                {
                                    hoffset = (uint)(offset_x + (hval & ~7));
                                }
                                else
                                {
                                    voffset = (uint)(y + hval);
                                }
                            }
                        }
                        else
                        {
                            if (Convert.ToBoolean(hval & valid_mask))
                            {
                                hoffset = (uint)(offset_x + (hval & ~7));
                            }
                            if (Convert.ToBoolean(vval & valid_mask))
                            {
                                voffset = (uint)(y + vval);
                            }
                        }
                    }
                }

                hoffset &= mask_x;
                voffset &= mask_y;

                uint screen_x = (uint)(Convert.ToBoolean(regs.screen_size & 1) ? 32 << 5 : 0);
                uint screen_y = (uint)(Convert.ToBoolean(regs.screen_size & 2) ? 32 << 5 : 0);
                if (regs.screen_size == 3)
                {
                    screen_y <<= 1;
                }

                uint tx = hoffset >> (int)tile_width;
                uint ty = voffset >> (int)tile_height;

                ushort offset = (ushort)(((ty & 0x1f) << 5) + (tx & 0x1f));
                if (Convert.ToBoolean(tx & 0x20))
                {
                    offset += (ushort)screen_x;
                }
                if (Convert.ToBoolean(ty & 0x20))
                {
                    offset += (ushort)screen_y;
                }

                ushort addr = (ushort)(regs.screen_addr + (offset << 1));
                tile = (uint)((StaticRAM.vram[(uint)(addr + 0)] << 0) + (StaticRAM.vram[(uint)(addr + 1)] << 8));
                bool mirror_y = Convert.ToBoolean(tile & 0x8000);
                bool mirror_x = Convert.ToBoolean(tile & 0x4000);
                priority = (Convert.ToBoolean(tile & 0x2000) ? regs.priority1 : regs.priority0);
                palette_number = (tile >> 10) & 7;
                palette_index = palette_offset + (palette_number << (int)palette_size);

                if (tile_width == 4 && Convert.ToBoolean(hoffset & 8) != mirror_x)
                {
                    tile += 1;
                }
                if (tile_height == 4 && Convert.ToBoolean(voffset & 8) != mirror_y)
                {
                    tile += 16;
                }
                ushort character = (ushort)(((tile & 0x03ff) + tiledata_index) & tile_mask);

                if (mirror_y)
                {
                    voffset ^= 7;
                }
                offset = (ushort)((character << (int)(4 + color_depth)) + ((voffset & 7) << 1));

                if (regs.mode >= (uint)Mode.BPP2)
                {
                    data[0] = StaticRAM.vram[(uint)(offset + 0)];
                    data[1] = StaticRAM.vram[(uint)(offset + 1)];
                }
                if (regs.mode >= (uint)Mode.BPP4)
                {
                    data[2] = StaticRAM.vram[(uint)(offset + 16)];
                    data[3] = StaticRAM.vram[(uint)(offset + 17)];
                }
                if (regs.mode >= (uint)Mode.BPP8)
                {
                    data[4] = StaticRAM.vram[(uint)(offset + 32)];
                    data[5] = StaticRAM.vram[(uint)(offset + 33)];
                    data[6] = StaticRAM.vram[(uint)(offset + 48)];
                    data[7] = StaticRAM.vram[(uint)(offset + 49)];
                }

                if (mirror_x)
                {
                    for (uint n = 0; n < 8; n++)
                    {
                        //reverse data bits in data[n]: 01234567 -> 76543210
                        data[n] = (byte)(((data[n] >> 4) & 0x0f) | ((data[n] << 4) & 0xf0));
                        data[n] = (byte)(((data[n] >> 2) & 0x33) | ((data[n] << 2) & 0xcc));
                        data[n] = (byte)(((data[n] >> 1) & 0x55) | ((data[n] << 1) & 0xaa));
                    }
                }
            }

            public uint get_tile_color()
            {
                uint color = 0;
                if (regs.mode >= (uint)Mode.BPP2)
                {
                    color += Convert.ToBoolean(data[0] & 0x80) ? 0x01U : 0U;
                    data[0] <<= 1;
                    color += Convert.ToBoolean(data[1] & 0x80) ? 0x02U : 0U;
                    data[1] <<= 1;
                }
                if (regs.mode >= (uint)Mode.BPP4)
                {
                    color += Convert.ToBoolean(data[2] & 0x80) ? 0x04U : 0U;
                    data[2] <<= 1;
                    color += Convert.ToBoolean(data[3] & 0x80) ? 0x08U : 0U;
                    data[3] <<= 1;
                }
                if (regs.mode >= (uint)Mode.BPP8)
                {
                    color += Convert.ToBoolean(data[4] & 0x80) ? 0x10U : 0U;
                    data[4] <<= 1;
                    color += Convert.ToBoolean(data[5] & 0x80) ? 0x20U : 0U;
                    data[5] <<= 1;
                    color += Convert.ToBoolean(data[6] & 0x80) ? 0x40U : 0U;
                    data[6] <<= 1;
                    color += Convert.ToBoolean(data[7] & 0x80) ? 0x80U : 0U;
                    data[7] <<= 1;
                }
                return color;
            }

            public uint get_tile(uint x, uint y)
            {
                bool hires = (self.regs.bgmode == 5 || self.regs.bgmode == 6);
                uint tile_height = (Convert.ToUInt32(regs.tile_size) == (uint)TileSize.Size8x8 ? 3U : 4U);
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

                uint screen_x = (uint)(Convert.ToBoolean(regs.screen_size & 1) ? (32 << 5) : 0);
                uint screen_y = (uint)(Convert.ToBoolean(regs.screen_size & 2) ? (32 << 5) : 0);
                if (regs.screen_size == 3)
                {
                    screen_y <<= 1;
                }

                x = (x & mask_x) >> (int)tile_width;
                y = (y & mask_y) >> (int)tile_height;

                ushort offset = (ushort)(((y & 0x1f) << 5) + (x & 0x1f));
                if (Convert.ToBoolean(x & 0x20))
                {
                    offset += (ushort)screen_x;
                }
                if (Convert.ToBoolean(y & 0x20))
                {
                    offset += (ushort)screen_y;
                }

                ushort addr = (ushort)(regs.screen_addr + (offset << 1));
                return (uint)((StaticRAM.vram[(uint)(addr + 0)] << 0) + (StaticRAM.vram[(uint)(addr + 1)] << 8));
            }

            public void reset()
            {
                regs.tiledata_addr = 0;
                regs.screen_addr = 0;
                regs.screen_size = 0;
                regs.mosaic = 0;
                regs.tile_size = Convert.ToBoolean(0);
                regs.mode = 0;
                regs.priority0 = 0;
                regs.priority1 = 0;
                regs.main_enable = Convert.ToBoolean(0);
                regs.sub_enable = Convert.ToBoolean(0);
                regs.hoffset = 0;
                regs.voffset = 0;

                output.main.palette = 0;
                output.main.priority = 0;
                output.sub.palette = 0;
                output.sub.priority = 0;

                x = 0;
                y = 0;

                mosaic_vcounter = 0;
                mosaic_voffset = 0;
                mosaic_hcounter = 0;
                mosaic_hoffset = 0;

                mosaic_priority = 0;
                mosaic_palette = 0;
                mosaic_tile = 0;

                tile_counter = 0;
                tile = 0;
                priority = 0;
                palette_number = 0;
                palette_index = 0;
                for (uint n = 0; n < 8; n++)
                {
                    data[n] = 0;
                }
            }

            public void serialize(Serializer s)
            {
                s.integer(id, "id");

                s.integer(regs.tiledata_addr, "regs.tiledata_addr");
                s.integer(regs.screen_addr, "regs.screen_addr");
                s.integer(regs.screen_size, "regs.screen_size");
                s.integer(regs.mosaic, "regs.mosaic");
                s.integer(regs.tile_size, "regs.tile_size");

                s.integer(regs.mode, "regs.mode");
                s.integer(regs.priority0, "regs.priority0");
                s.integer(regs.priority1, "regs.priority1");

                s.integer(regs.main_enable, "regs.main_enabled");
                s.integer(regs.sub_enable, "regs.sub_enabled");

                s.integer(regs.hoffset, "regs.hoffset");
                s.integer(regs.voffset, "regs.voffset");

                s.integer(output.main.priority, "output.main.priority");
                s.integer(output.main.palette, "output.main.palette");
                s.integer(output.main.tile, "output.main.tile");

                s.integer(output.sub.priority, "output.sub.priority");
                s.integer(output.sub.palette, "output.sub.palette");
                s.integer(output.sub.tile, "output.sub.tile");

                s.integer(x, "x");
                s.integer(y, "y");

                s.integer(mosaic_vcounter, "mosaic_vcounter");
                s.integer(mosaic_voffset, "mosaic_voffset");
                s.integer(mosaic_hcounter, "mosaic_hcounter");
                s.integer(mosaic_hoffset, "mosaic_hoffset");

                s.integer(mosaic_priority, "mosaic_priority");
                s.integer(mosaic_palette, "mosaic_palette");
                s.integer(mosaic_tile, "mosaic_tile");

                s.integer(tile_counter, "tile_counter");
                s.integer(tile, "tile");
                s.integer(priority, "priority");
                s.integer(palette_number, "palette_number");
                s.integer(palette_index, "palette_index");
                s.array(data, "data");
            }

            public Background(PPU self_, uint id_)
            {
                self = self_;
                id = id_;
            }

            private int clip(int n)
            {   //13-bit sign extend: --s---nnnnnnnnnn -> ssssssnnnnnnnnnn
                return Convert.ToBoolean(n & 0x2000) ? (n | ~1023) : (n & 1023);
            }

            private void run_mode7()
            {
                int a = Bit.sclip(16, self.regs.m7a);
                int b = Bit.sclip(16, self.regs.m7b);
                int c = Bit.sclip(16, self.regs.m7c);
                int d = Bit.sclip(16, self.regs.m7d);

                int cx = Bit.sclip(13, self.regs.m7x);
                int cy = Bit.sclip(13, self.regs.m7y);
                int hoffset = Bit.sclip(13, self.regs.mode7_hoffset);
                int voffset = Bit.sclip(13, self.regs.mode7_voffset);

                if (Convert.ToBoolean(this.x++ & ~255))
                {
                    return;
                }
                uint x = mosaic_hoffset;
                uint y = self.bg1.mosaic_voffset;  //BG2 vertical mosaic uses BG1 mosaic size

                if (--mosaic_hcounter == 0)
                {
                    mosaic_hcounter = regs.mosaic + 1;
                    mosaic_hoffset += regs.mosaic + 1;
                }

                if (self.regs.mode7_hflip)
                {
                    x = 255 - x;
                }
                if (self.regs.mode7_vflip)
                {
                    y = 255 - y;
                }

                int psx = (int)(((a * clip(hoffset - cx)) & ~63) + ((b * clip(voffset - cy)) & ~63) + ((b * y) & ~63) + (cx << 8));
                int psy = (int)(((c * clip(hoffset - cx)) & ~63) + ((d * clip(voffset - cy)) & ~63) + ((d * y) & ~63) + (cy << 8));

                int px = (int)(psx + (a * x));
                int py = (int)(psy + (c * x));

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
                            break;
                        }
                    //palette color 0 outside of screen area
                    case 2:
                        {
                            if (Convert.ToBoolean((px | py) & ~1023))
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
                            break;
                        }
                    //character 0 repetition outside of screen area
                    case 3:
                        {
                            if (Convert.ToBoolean((px | py) & ~1023))
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
                            break;
                        }
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

                if (regs.main_enable)
                {
                    output.main.palette = (byte)palette;
                    output.main.priority = priority;
                    output.main.tile = 0;
                }

                if (regs.sub_enable)
                {
                    output.sub.palette = (byte)palette;
                    output.sub.priority = priority;
                    output.main.tile = 0;
                }
            }
        }
    }
}
#endif