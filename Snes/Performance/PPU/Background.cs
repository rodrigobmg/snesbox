#if PERFORMANCE
using System;
using Nall;

namespace Snes
{
    partial class PPU
    {
        private partial class Background
        {
            public enum ID { BG1, BG2, BG3, BG4 }
            public enum Mode { BPP2, BPP4, BPP8, Mode7, Inactive }
            public enum ScreenSize { Size32x32, Size32x64, Size64x32, Size64x64 }
            public enum TileSize { Size8x8, Size16x16 }

            public bool priority0_enable;
            public bool priority1_enable;

            public Regs regs = new Regs();

            public ushort[][] mosaic_table;

            public uint id;
            public uint opt_valid_bit;

            public bool hires;
            public int width;

            public uint tile_width;
            public uint tile_height;

            public uint mask_x;
            public uint mask_y;

            public uint scx;
            public uint scy;

            public uint hscroll;
            public uint vscroll;

            public uint mosaic_vcounter;
            public uint mosaic_voffset;

            public LayerWindow window = new LayerWindow();

            public uint get_tile(uint hoffset, uint voffset)
            {
                uint tile_x = (hoffset & mask_x) >> (int)tile_width;
                uint tile_y = (voffset & mask_y) >> (int)tile_height;

                uint tile_pos = ((tile_y & 0x1f) << 5) + (tile_x & 0x1f);
                if (Convert.ToBoolean(tile_y & 0x20))
                {
                    tile_pos += scy;
                }
                if (Convert.ToBoolean(tile_x & 0x20))
                {
                    tile_pos += scx;
                }

                ushort tiledata_addr = (ushort)(regs.screen_addr + (tile_pos << 1));
                return (uint)((StaticRAM.vram[tiledata_addr + 0U] << 0) + (StaticRAM.vram[tiledata_addr + 1U] << 8));
            }

            public void offset_per_tile(uint x, uint y, ref uint hoffset, ref uint voffset)
            {
                uint opt_x = (x + (hscroll & 7)), hval, vval = default(uint);
                if (opt_x >= 8)
                {
                    hval = self.bg3.get_tile((uint)((opt_x - 8) + (self.bg3.regs.hoffset & ~7)), self.bg3.regs.voffset + 0U);
                    if (self.regs.bgmode != 4)
                    {
                        vval = self.bg3.get_tile((uint)((opt_x - 8) + (self.bg3.regs.hoffset & ~7)), self.bg3.regs.voffset + 8U);
                    }

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

            public void scanline()
            {
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
                if (self.regs.display_disable)
                {
                    return;
                }

                hires = (self.regs.bgmode == 5 || self.regs.bgmode == 6);
                width = !hires ? 256 : 512;

                tile_height = regs.tile_size ? 4U : 3U;
                tile_width = hires ? 4 : tile_height;

                mask_x = (uint)(tile_height == 4 ? width << 1 : width);
                mask_y = mask_x;
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

                scy = (uint)(Convert.ToBoolean(regs.screen_size & 2) ? 32 << 5 : 0);
                scx = (uint)(Convert.ToBoolean(regs.screen_size & 1) ? 32 << 5 : 0);
                if (regs.screen_size == 3)
                {
                    scy <<= 1;
                }
            }

            public void render()
            {
                if (regs.mode == (uint)Mode.Inactive)
                {
                    return;
                }
                if (regs.main_enable == false && regs.sub_enable == false)
                {
                    return;
                }

                if (regs.main_enable)
                {
                    window.render(Convert.ToBoolean(0));
                }
                if (regs.sub_enable)
                {
                    window.render(Convert.ToBoolean(1));
                }
                if (regs.mode == (uint)Mode.Mode7)
                {
                    render_mode7();
                    return;
                }

                uint priority0 = (priority0_enable ? regs.priority0 : 0);
                uint priority1 = (priority1_enable ? regs.priority1 : 0);
                if (priority0 + priority1 == 0)
                {
                    return;
                }

                uint mosaic_hcounter = 1;
                uint mosaic_palette = 0;
                uint mosaic_priority = 0;
                uint mosaic_color = 0;

                uint bgpal_index = (self.regs.bgmode == 0 ? id << 5 : 0);
                uint pal_size = 2U << (int)regs.mode;
                uint tile_mask = 0x0fffU >> (int)regs.mode;
                uint tiledata_index = regs.tiledata_addr >> (int)(4 + regs.mode);

                hscroll = regs.hoffset;
                vscroll = regs.voffset;

                uint y = (regs.mosaic == 0 ? self.PPUCounter.vcounter() : mosaic_voffset);
                if (hires)
                {
                    hscroll <<= 1;
                    if (self.regs.interlace)
                    {
                        y = (y << 1) + Convert.ToUInt32(self.PPUCounter.field());
                    }
                }

                uint tile_pri, tile_num;
                uint pal_index, pal_num;
                uint hoffset, voffset, col;
                bool mirror_x, mirror_y;

                bool is_opt_mode = (self.regs.bgmode == 2 || self.regs.bgmode == 4 || self.regs.bgmode == 6);
                bool is_direct_color_mode = (self.screen.regs.direct_color == true && id == (uint)ID.BG1 && (self.regs.bgmode == 3 || self.regs.bgmode == 4));

                int x = (int)(0 - (hscroll & 7));
                while (x < width)
                {
                    hoffset = (uint)(x + hscroll);
                    voffset = y + vscroll;
                    if (is_opt_mode)
                    {
                        offset_per_tile((uint)x, (uint)y, ref hoffset, ref voffset);
                    }
                    hoffset &= mask_x;
                    voffset &= mask_y;

                    tile_num = get_tile(hoffset, voffset);
                    mirror_y = Convert.ToBoolean(tile_num & 0x8000);
                    mirror_x = Convert.ToBoolean(tile_num & 0x4000);
                    tile_pri = Convert.ToBoolean(tile_num & 0x2000) ? priority1 : priority0;
                    pal_num = (tile_num >> 10) & 7;
                    pal_index = (bgpal_index + (pal_num << (int)pal_size)) & 0xff;

                    if (tile_width == 4 && Convert.ToBoolean(hoffset & 8) != mirror_x)
                    {
                        tile_num += 1;
                    }
                    if (tile_height == 4 && Convert.ToBoolean(voffset & 8) != mirror_y)
                    {
                        tile_num += 16;
                    }
                    tile_num = ((tile_num & 0x03ff) + tiledata_index) & tile_mask;

                    if (mirror_y)
                    {
                        voffset ^= 7;
                    }
                    uint mirror_xmask = !mirror_x ? 0U : 7U;

                    ArraySegment<byte> tiledata = self.cache.tile(regs.mode, tile_num);
                    tiledata = new ArraySegment<byte>(tiledata.Array, tiledata.Offset + (int)((voffset & 7) * 8), tiledata.Count - (int)((voffset & 7) * 8));

                    for (uint n = 0; n < 8; n++, x++)
                    {
                        if (Convert.ToBoolean(x & width))
                        {
                            continue;
                        }
                        if (--mosaic_hcounter == 0)
                        {
                            mosaic_hcounter = regs.mosaic + 1;
                            mosaic_palette = tiledata.Array[tiledata.Offset + (n ^ mirror_xmask)];
                            mosaic_priority = tile_pri;
                            if (is_direct_color_mode)
                            {
                                mosaic_color = self.screen.get_direct_color(pal_num, mosaic_palette);
                            }
                            else
                            {
                                mosaic_color = self.screen.get_palette(pal_index + mosaic_palette);
                            }
                        }
                        if (mosaic_palette == 0)
                        {
                            continue;
                        }

                        if (hires == false)
                        {
                            if (regs.main_enable && !Convert.ToBoolean(window.main[x]))
                            {
                                self.screen.output.plot_main((uint)x, mosaic_color, mosaic_priority, id);
                            }
                            if (regs.sub_enable && !Convert.ToBoolean(window.sub[x]))
                            {
                                self.screen.output.plot_sub((uint)x, mosaic_color, mosaic_priority, id);
                            }
                        }
                        else
                        {
                            int half_x = x >> 1;
                            if (Convert.ToBoolean(x & 1))
                            {
                                if (regs.main_enable && !Convert.ToBoolean(window.main[half_x]))
                                {
                                    self.screen.output.plot_main((uint)half_x, mosaic_color, mosaic_priority, id);
                                }
                            }
                            else
                            {
                                if (regs.sub_enable && !Convert.ToBoolean(window.sub[half_x]))
                                {
                                    self.screen.output.plot_sub((uint)half_x, mosaic_color, mosaic_priority, id);
                                }
                            }
                        }
                    }
                }
            }

            private int Clip(int x)
            {
                return (Convert.ToBoolean((x) & 0x2000) ? ((x) | ~0x03ff) : ((x) & 0x03ff));
            }

            public void render_mode7()
            {
                int px, py;
                int tx, ty, tile, palette = default(int);

                int a = Bit.sclip(16, self.regs.m7a);
                int b = Bit.sclip(16, self.regs.m7b);
                int c = Bit.sclip(16, self.regs.m7c);
                int d = Bit.sclip(16, self.regs.m7d);

                int cx = Bit.sclip(13, self.regs.m7x);
                int cy = Bit.sclip(13, self.regs.m7y);
                int hofs = Bit.sclip(13, self.regs.mode7_hoffset);
                int vofs = Bit.sclip(13, self.regs.mode7_voffset);

                int y = (self.regs.mode7_vflip == false ? self.PPUCounter.vcounter() : 255 - self.PPUCounter.vcounter());

                ushort[] mosaic_x, mosaic_y;
                if (id == (uint)ID.BG1)
                {
                    mosaic_x = mosaic_table[self.bg1.regs.mosaic];
                    mosaic_y = mosaic_table[self.bg1.regs.mosaic];
                }
                else
                {
                    mosaic_x = mosaic_table[self.bg2.regs.mosaic];
                    mosaic_y = mosaic_table[self.bg1.regs.mosaic];
                }

                uint priority0 = (priority0_enable ? regs.priority0 : 0);
                uint priority1 = (priority1_enable ? regs.priority1 : 0);
                if (priority0 + priority1 == 0)
                {
                    return;
                }

                int psx = ((a * Clip(hofs - cx)) & ~63) + ((b * Clip(vofs - cy)) & ~63) + ((b * mosaic_y[y]) & ~63) + (cx << 8);
                int psy = ((c * Clip(hofs - cx)) & ~63) + ((d * Clip(vofs - cy)) & ~63) + ((d * mosaic_y[y]) & ~63) + (cy << 8);
                for (int x = 0; x < 256; x++)
                {
                    px = (psx + (a * mosaic_x[x])) >> 8;
                    py = (psy + (c * mosaic_x[x])) >> 8;

                    switch (self.regs.mode7_repeat)
                    {
                        case 0:
                        case 1:
                            {
                                px &= 1023;
                                py &= 1023;
                                tx = ((px >> 3) & 127);
                                ty = ((py >> 3) & 127);
                                tile = StaticRAM.vram[(uint)((ty * 128 + tx) << 1)];
                                palette = StaticRAM.vram[(uint)((((tile << 6) + ((py & 7) << 3) + (px & 7)) << 1) + 1)];
                                break;
                            }

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
                                    tx = ((px >> 3) & 127);
                                    ty = ((py >> 3) & 127);
                                    tile = StaticRAM.vram[(uint)((ty * 128 + tx) << 1)];
                                    palette = StaticRAM.vram[(uint)((((tile << 6) + ((py & 7) << 3) + (px & 7)) << 1) + 1)];
                                }
                                break;
                            }

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
                                    tx = ((px >> 3) & 127);
                                    ty = ((py >> 3) & 127);
                                    tile = StaticRAM.vram[(uint)((ty * 128 + tx) << 1)];
                                }
                                palette = StaticRAM.vram[(uint)((((tile << 6) + ((py & 7) << 3) + (px & 7)) << 1) + 1)];
                                break;
                            }
                    }

                    uint priority;
                    if (id == (uint)ID.BG1)
                    {
                        priority = priority0;
                    }
                    else
                    {
                        priority = (Convert.ToBoolean(palette & 0x80) ? priority1 : priority0);
                        palette &= 0x7f;
                    }

                    if (palette == 0)
                    {
                        continue;
                    }
                    uint plot_x = (uint)(self.regs.mode7_hflip == false ? x : 255 - x);

                    uint color;
                    if (self.screen.regs.direct_color && id == (uint)ID.BG1)
                    {
                        color = self.screen.get_direct_color(0, (uint)palette);
                    }
                    else
                    {
                        color = self.screen.get_palette((uint)palette);
                    }

                    if (regs.main_enable && !Convert.ToBoolean(window.main[plot_x]))
                    {
                        self.screen.output.plot_main(plot_x, color, priority, id);
                    }
                    if (regs.sub_enable && !Convert.ToBoolean(window.sub[plot_x]))
                    {
                        self.screen.output.plot_sub(plot_x, color, priority, id);
                    }
                }
            }

            public void serialize(Serializer s)
            {
                s.integer(regs.mode, "regs.mode");
                s.integer(regs.priority0, "regs.priority0");
                s.integer(regs.priority1, "regs.priority1");

                s.integer(regs.tile_size, "regs.tile_size");
                s.integer(regs.mosaic, "regs.mosaic");

                s.integer(regs.screen_addr, "regs.screen_addr");
                s.integer(regs.screen_size, "regs.screen_size");
                s.integer(regs.tiledata_addr, "regs.tiledata_addr");

                s.integer(regs.hoffset, "regs.hoffset");
                s.integer(regs.voffset, "regs.voffset");

                s.integer(regs.main_enable, "regs.main_enable");
                s.integer(regs.sub_enable, "regs.sub_enable");

                s.integer(hires, "hires");
                s.integer(width, "width");

                s.integer(tile_width, "tile_width");
                s.integer(tile_height, "tile_height");

                s.integer(mask_x, "mask_x");
                s.integer(mask_y, "mask_y");

                s.integer(scx, "scx");
                s.integer(scy, "scy");

                s.integer(hscroll, "hscroll");
                s.integer(vscroll, "vscroll");

                s.integer(mosaic_vcounter, "mosaic_vcounter");
                s.integer(mosaic_voffset, "mosaic_voffset");

                window.serialize(s);
            }

            public Background(PPU self, uint id)
            {
                this.self = self;
                this.id = id;

                priority0_enable = true;
                priority1_enable = true;

                opt_valid_bit = (uint)(id == (uint)ID.BG1 ? 0x2000 : id == (uint)ID.BG2 ? 0x4000 : 0x0000);

                mosaic_table = new ushort[16][];
                for (uint m = 0; m < 16; m++)
                {
                    mosaic_table[m] = new ushort[4096];
                    for (uint x = 0; x < 4096; x++)
                    {
                        mosaic_table[m][x] = (ushort)((x / (m + 1)) * (m + 1));
                    }
                }
            }

            public PPU self;
        }
    }
}
#endif