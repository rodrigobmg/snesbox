#if PERFORMANCE
using System;
using System.Linq;
using Nall;

namespace Snes
{
    partial class PPU
    {
        private partial class Sprite
        {
            public bool priority0_enable;
            public bool priority1_enable;
            public bool priority2_enable;
            public bool priority3_enable;

            public Regs regs = new Regs();
            public List[] list = new List[128];
            public bool list_valid;

            public byte[] itemlist = new byte[32];
            public TileList[] tilelist = new TileList[34];
            public Output output = new Output();
            public LayerWindow window = new LayerWindow();

            public void frame()
            {
                regs.time_over = false;
                regs.range_over = false;
            }

            public void update_list(uint addr, byte data)
            {
                if (addr < 0x0200)
                {
                    uint i = addr >> 2;
                    switch (addr & 3)
                    {
                        case 0:
                            list[i].x = (list[i].x & 0x0100) | data;
                            break;
                        case 1:
                            list[i].y = (uint)((data + 1) & 0xff);
                            break;
                        case 2:
                            list[i].character = data;
                            break;
                        case 3:
                            list[i].vflip = Convert.ToBoolean(data & 0x80);
                            list[i].hflip = Convert.ToBoolean(data & 0x40);
                            list[i].priority = (uint)((data >> 4) & 3);
                            list[i].palette = (uint)((data >> 1) & 7);
                            list[i].use_nameselect = Convert.ToBoolean(data & 0x01);
                            break;
                    }
                }
                else
                {
                    uint i = (addr & 0x1f) << 2;
                    list[i + 0].x = (uint)((data & 0x01) << 8) | (list[i + 0].x & 0xff);
                    list[i + 0].size = Convert.ToBoolean(data & 0x02);
                    list[i + 1].x = (uint)((data & 0x04) << 6) | (list[i + 1].x & 0xff);
                    list[i + 1].size = Convert.ToBoolean(data & 0x08);
                    list[i + 2].x = (uint)((data & 0x10) << 4) | (list[i + 2].x & 0xff);
                    list[i + 2].size = Convert.ToBoolean(data & 0x20);
                    list[i + 3].x = (uint)((data & 0x40) << 2) | (list[i + 3].x & 0xff);
                    list[i + 3].size = Convert.ToBoolean(data & 0x80);
                    list_valid = false;
                }
            }

            public void address_reset()
            {
                self.regs.oam_addr = (ushort)(self.regs.oam_baseaddr << 1);
                set_first();
            }

            public void set_first()
            {
                regs.first_sprite = (uint)(self.regs.oam_priority == false ? 0 : (self.regs.oam_addr >> 2) & 127);
            }

            public bool on_scanline(uint sprite)
            {
                var s = list[sprite];
                if (s.x > 256 && (s.x + s.width - 1) < 512)
                {
                    return false;
                }
                int height = (int)(regs.interlace == false ? s.height : s.height >> 1);
                if (self.PPUCounter.vcounter() >= s.y && self.PPUCounter.vcounter() < (s.y + height))
                {
                    return true;
                }
                if ((s.y + height) >= 256 && self.PPUCounter.vcounter() < ((s.y + height) & 255))
                {
                    return true;
                }
                return false;
            }

            private static readonly uint[] width1 = { 8, 8, 8, 16, 16, 32, 16, 16 };
            private static readonly uint[] height1 = { 8, 8, 8, 16, 16, 32, 32, 32 };
            private static readonly uint[] width2 = { 16, 32, 64, 32, 64, 64, 32, 32 };
            private static readonly uint[] height2 = { 16, 32, 64, 32, 64, 64, 64, 32 };

            public void render()
            {
                if (list_valid == false)
                {
                    list_valid = true;
                    for (uint i = 0; i < 128; i++)
                    {
                        if (list[i].size == Convert.ToBoolean(0))
                        {
                            list[i].width = width1[regs.base_size];
                            list[i].height = height1[regs.base_size];
                        }
                        else
                        {
                            list[i].width = width2[regs.base_size];
                            list[i].height = height2[regs.base_size];
                            if (regs.interlace && regs.base_size >= 6)
                            {
                                list[i].height = 16;
                            }
                        }
                    }
                }

                uint itemcount = 0;
                uint tilecount = 0;
                Array.Copy(Enumerable.Repeat((byte)0xff, 256).ToArray(), output.priority, 256);
                Array.Copy(Enumerable.Repeat((byte)0xff, 32).ToArray(), itemlist, 32);
                for (uint i = 0; i < 34; i++)
                {
                    tilelist[i].tile = 0xffff;
                }

                for (uint i = 0; i < 128; i++)
                {
                    uint s = (regs.first_sprite + i) & 127;
                    if (on_scanline(s) == false)
                    {
                        continue;
                    }
                    if (itemcount++ >= 32)
                    {
                        break;
                    }
                    itemlist[itemcount - 1] = (byte)s;
                }

                for (int i = 31; i >= 0; i--)
                {
                    if (itemlist[i] == 0xff)
                    {
                        continue;
                    }
                    var s = list[itemlist[i]];
                    uint tile_width = s.width >> 3;
                    int x = (int)s.x;
                    int y = (int)((self.PPUCounter.vcounter() - s.y) & 0xff);
                    if (regs.interlace)
                    {
                        y <<= 1;
                    }

                    if (s.vflip)
                    {
                        if (s.width == s.height)
                        {
                            y = (int)((s.height - 1) - y);
                        }
                        else
                        {
                            y = (int)((y < s.width) ? ((s.width - 1) - y) : (s.width + ((s.width - 1) - (y - s.width))));
                        }
                    }

                    if (regs.interlace)
                    {
                        y = (s.vflip == false) ? (y + Convert.ToInt32(self.PPUCounter.field())) : (y - Convert.ToInt32(self.PPUCounter.field()));
                    }

                    x &= 511;
                    y &= 255;

                    ushort tdaddr = (ushort)(regs.tiledata_addr);
                    ushort chrx = (ushort)((s.character >> 0) & 15);
                    ushort chry = (ushort)((s.character >> 4) & 15);
                    if (s.use_nameselect)
                    {
                        tdaddr += (ushort)((256 * 32) + (regs.nameselect << 13));
                    }
                    chry += (ushort)((y >> 3));
                    chry &= 15;
                    chry <<= 4;

                    for (uint tx = 0; tx < tile_width; tx++)
                    {
                        uint sx = (uint)((x + (tx << 3)) & 511);
                        if (x != 256 && sx >= 256 && (sx + 7) < 512)
                        {
                            continue;
                        }
                        if (tilecount++ >= 34)
                        {
                            break;
                        }

                        uint n = tilecount - 1;
                        tilelist[n].x = sx;
                        tilelist[n].y = (uint)y;
                        tilelist[n].priority = s.priority;
                        tilelist[n].palette = 128 + (s.palette << 4);
                        tilelist[n].hflip = s.hflip;

                        uint mx = (s.hflip == false) ? tx : ((tile_width - 1) - tx);
                        uint pos = tdaddr + ((chry + ((chrx + mx) & 15)) << 5);
                        tilelist[n].tile = (pos >> 5) & 0x07ff;
                    }
                }

                regs.time_over |= (tilecount > 34);
                regs.range_over |= (itemcount > 32);

                if (regs.main_enable == false && regs.sub_enable == false)
                {
                    return;
                }

                for (uint i = 0; i < 34; i++)
                {
                    if (tilelist[i].tile == 0xffff)
                    {
                        continue;
                    }

                    var t = tilelist[i];
                    ArraySegment<byte> tiledata = self.cache.tile_4bpp(t.tile);
                    tiledata = new ArraySegment<byte>(tiledata.Array, tiledata.Offset + (int)((t.y & 7) << 3), tiledata.Count - (int)((t.y & 7) << 3));
                    uint sx = t.x;
                    for (uint x = 0; x < 8; x++)
                    {
                        sx &= 511;
                        if (sx < 256)
                        {
                            uint color = tiledata.Array[tiledata.Offset + (t.hflip == false ? x : 7 - x)];
                            if (Convert.ToBoolean(color))
                            {
                                color += t.palette;
                                output.palette[sx] = (byte)color;
                                output.priority[sx] = (byte)t.priority;
                            }
                        }
                        sx++;
                    }
                }

                if (regs.main_enable)
                {
                    window.render(Convert.ToBoolean(0));
                }
                if (regs.sub_enable)
                {
                    window.render(Convert.ToBoolean(1));
                }

                uint priority0 = (priority0_enable ? regs.priority0 : 0);
                uint priority1 = (priority1_enable ? regs.priority1 : 0);
                uint priority2 = (priority2_enable ? regs.priority2 : 0);
                uint priority3 = (priority3_enable ? regs.priority3 : 0);
                if (priority0 + priority1 + priority2 + priority3 == 0)
                {
                    return;
                }
                uint[] priority_table = { priority0, priority1, priority2, priority3 };

                for (uint x = 0; x < 256; x++)
                {
                    if (output.priority[x] == 0xff)
                    {
                        continue;
                    }
                    uint priority = priority_table[output.priority[x]];
                    uint palette = output.palette[x];
                    uint color = self.screen.get_palette(output.palette[x]);
                    if (regs.main_enable && !Convert.ToBoolean(window.main[x]))
                    {
                        self.screen.output.plot_main(x, color, priority, 4 + Convert.ToUInt32(palette < 192));
                    }
                    if (regs.sub_enable && !Convert.ToBoolean(window.sub[x]))
                    {
                        self.screen.output.plot_sub(x, color, priority, 4 + Convert.ToUInt32(palette < 192));
                    }
                }
            }

            public void serialize(Serializer s)
            {
                s.integer(regs.priority0, "regs.priority0");
                s.integer(regs.priority1, "regs.priority1");
                s.integer(regs.priority2, "regs.priority2");
                s.integer(regs.priority3, "regs.priority3");

                s.integer(regs.base_size, "regs.base_size");
                s.integer(regs.nameselect, "regs.nameselect");
                s.integer(regs.tiledata_addr, "regs.tiledata_addr");
                s.integer(regs.first_sprite, "regs.first_sprite");

                s.integer(regs.main_enable, "regs.main_enable");
                s.integer(regs.sub_enable, "regs.sub_enable");

                s.integer(regs.interlace, "regs.interlace");

                s.integer(regs.time_over, "regs.time_over");
                s.integer(regs.range_over, "regs.range_over");

                for (uint i = 0; i < 128; i++)
                {
                    s.integer(list[i].width, "list[i].width");
                    s.integer(list[i].height, "list[i].height");
                    s.integer(list[i].x, "list[i].x");
                    s.integer(list[i].y, "list[i].y");
                    s.integer(list[i].character, "list[i].character");
                    s.integer(list[i].use_nameselect, "list[i].use_nameselect");
                    s.integer(list[i].vflip, "list[i].vflip");
                    s.integer(list[i].hflip, "list[i].hflip");
                    s.integer(list[i].palette, "list[i].palette");
                    s.integer(list[i].priority, "list[i].priority");
                    s.integer(list[i].size, "list[i].size");
                }
                s.integer(list_valid, "list_valid");

                s.array(itemlist, "itemlist");
                for (uint i = 0; i < 34; i++)
                {
                    s.integer(tilelist[i].x, "tilelist[i].x");
                    s.integer(tilelist[i].y, "tilelist[i].y");
                    s.integer(tilelist[i].priority, "tilelist[i].priority");
                    s.integer(tilelist[i].palette, "tilelist[i].palette");
                    s.integer(tilelist[i].tile, "tilelist[i].tile");
                    s.integer(tilelist[i].hflip, "tilelist[i].hflip");
                }

                s.array(output.palette, "output.palette");
                s.array(output.priority, "output.priority");

                window.serialize(s);
            }

            public Sprite(PPU self)
            {
                this.self = self;
                priority0_enable = true;
                priority1_enable = true;
                priority2_enable = true;
                priority3_enable = true;

                Utility.InstantiateArrayElements(list);
                Utility.InstantiateArrayElements(tilelist);
            }

            public PPU self;
        }
    }
}
#endif