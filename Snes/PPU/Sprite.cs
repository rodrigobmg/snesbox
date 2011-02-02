#if ACCURACY
using System;
using Nall;

namespace Snes
{
    partial class PPU
    {
        private partial class Sprite
        {
            public PPU self;
            public SpriteItem[] list = new SpriteItem[128];
            public State t = new State();
            public Regs regs = new Regs();
            public Output output = new Output();

            public void update(uint addr, byte data)
            {
                StaticRAM.oam[addr] = data;

                if (addr < 0x0200)
                {
                    uint n = addr >> 2;
                    addr &= 3;
                    if (addr == 0)
                    {
                        list[n].x = (ushort)((list[n].x & 0x100) | data);
                    }
                    else if (addr == 1)
                    {
                        list[n].y = data;
                    }
                    else if (addr == 2)
                    {
                        list[n].character = data;
                    }
                    else
                    {  //(addr == 3)
                        list[n].vflip = Convert.ToBoolean(data & 0x80);
                        list[n].hflip = Convert.ToBoolean(data & 0x40);
                        list[n].priority = (byte)((data >> 4) & 3);
                        list[n].palette = (byte)((data >> 1) & 7);
                        list[n].nameselect = Convert.ToBoolean(data & 1);
                    }
                }
                else
                {
                    uint n = (addr & 0x1f) << 2;
                    list[n + 0].x = (ushort)(((data & 0x01) << 8) | (list[n + 0].x & 0xff));
                    list[n + 0].size = Convert.ToBoolean(data & 0x02);
                    list[n + 1].x = (ushort)(((data & 0x04) << 6) | (list[n + 1].x & 0xff));
                    list[n + 1].size = Convert.ToBoolean(data & 0x08);
                    list[n + 2].x = (ushort)(((data & 0x10) << 4) | (list[n + 2].x & 0xff));
                    list[n + 2].size = Convert.ToBoolean(data & 0x20);
                    list[n + 3].x = (ushort)(((data & 0x40) << 2) | (list[n + 3].x & 0xff));
                    list[n + 3].size = Convert.ToBoolean(data & 0x80);
                }
            }

            public void address_reset()
            {
                self.regs.oam_addr = self.regs.oam_baseaddr;
                set_first_sprite();
            }

            public void set_first_sprite()
            {
                regs.first_sprite = (self.regs.oam_priority == false ? (byte)0 : (byte)((self.regs.oam_addr >> 2) & 127));
            }

            public void frame()
            {
                regs.time_over = false;
                regs.range_over = false;
            }

            public void scanline()
            {
                t.x = 0;
                t.y = self.PPUCounter.vcounter();

                t.item_count = 0;
                t.tile_count = 0;

                t.active = !t.active;
                var oam_item = t.item[Convert.ToInt32(t.active)];
                var oam_tile = t.tile[Convert.ToInt32(t.active)];

                if (t.y == (!self.regs.overscan ? 225 : 240) && self.regs.display_disable == false)
                {
                    address_reset();
                }
                if (t.y >= (!self.regs.overscan ? 224 : 239))
                {
                    return;
                }

                for (int i = 0; i < 32; i++)
                {
                    oam_item[i] = 0xff; //default to invalid
                }
                for (uint i = 0; i < 34; i++)
                {
                    oam_tile[i].x = 0xffff;  //default to invalid
                }

                for (uint i = 0; i < 128; i++)
                {
                    uint sprite = (regs.first_sprite + i) & 127;
                    if (on_scanline(list[sprite]) == false)
                    {
                        continue;
                    }
                    if (t.item_count++ >= 32)
                    {
                        break;
                    }
                    oam_item[t.item_count - 1] = (byte)sprite;
                }

                //TODO: Remove this hack
                if (t.item_count > 0 && (t.item_count > oam_item.Length))
                {
                    ppu.regs.oam_iaddr.Assign(0x0200 + (0 >> 2));
                }
                else if (t.item_count > 0 && oam_item[t.item_count - 1] != 0xff)
                {
                    ppu.regs.oam_iaddr.Assign((uint)(0x0200 + (oam_item[t.item_count - 1] >> 2)));
                }
            }

            public void run()
            {
                output.main.priority = 0;
                output.sub.priority = 0;

                var oam_tile = t.tile[Convert.ToInt32(!t.active)];
                uint[] priority_table = { regs.priority0, regs.priority1, regs.priority2, regs.priority3 };
                uint x = t.x++;

                for (uint n = 0; n < 34; n++)
                {
                    var tile = oam_tile[n];
                    if (tile.x == 0xffff)
                    {
                        break;
                    }

                    int px = (int)(x - Bit.sclip(9, tile.x));
                    if (Convert.ToBoolean(px & ~7))
                    {
                        continue;
                    }

                    uint mask = (uint)(0x80 >> (tile.hflip == false ? px : 7 - px));
                    uint color;
                    color = (Bit.ToBit(tile.d0 & mask)) << 0;
                    color |= (Bit.ToBit(tile.d1 & mask)) << 1;
                    color |= (Bit.ToBit(tile.d2 & mask)) << 2;
                    color |= (Bit.ToBit(tile.d3 & mask)) << 3;

                    if (Convert.ToBoolean(color))
                    {
                        if (regs.main_enable)
                        {
                            output.main.palette = (byte)(tile.palette + color);
                            output.main.priority = priority_table[tile.priority];
                        }

                        if (regs.sub_enable)
                        {
                            output.sub.palette = (byte)(tile.palette + color);
                            output.sub.priority = priority_table[tile.priority];
                        }
                    }
                }
            }

            public void tilefetch()
            {
                var oam_item = t.item[Convert.ToInt32(t.active)];
                var oam_tile = t.tile[Convert.ToInt32(t.active)];

                for (int i = 31; i >= 0; i--)
                {
                    if (oam_item[i] == 0xff)
                    {
                        continue;
                    }
                    var sprite = list[oam_item[i]];

                    uint tile_width = sprite.width() >> 3;
                    int x = sprite.x;
                    int y = (int)((t.y - sprite.y) & 0xff);
                    if (regs.interlace)
                    {
                        y <<= 1;
                    }

                    if (sprite.vflip)
                    {
                        if (sprite.width() == sprite.height())
                        {
                            y = (int)((sprite.height() - 1) - y);
                        }
                        else if (y < sprite.width())
                        {
                            y = (int)((sprite.width() - 1) - y);
                        }
                        else
                        {
                            y = (int)(sprite.width() + ((sprite.width() - 1) - (y - sprite.width())));
                        }
                    }

                    if (regs.interlace)
                    {
                        y = (sprite.vflip == false ? y + Convert.ToInt32(self.PPUCounter.field()) : y - Convert.ToInt32(self.PPUCounter.field()));
                    }

                    x &= 511;
                    y &= 255;

                    ushort tiledata_addr = regs.tiledata_addr;
                    ushort chrx = (ushort)((sprite.character >> 0) & 15);
                    ushort chry = (ushort)((sprite.character >> 4) & 15);
                    if (sprite.nameselect)
                    {
                        tiledata_addr += (ushort)((256 * 32) + (regs.nameselect << 13));
                    }
                    chry += (ushort)(y >> 3);
                    chry &= 15;
                    chry <<= 4;

                    for (uint tx = 0; tx < tile_width; tx++)
                    {
                        uint sx = (uint)((x + (tx << 3)) & 511);
                        if (x != 256 && sx >= 256 && (sx + 7) < 512)
                        {
                            continue;
                        }
                        if (t.tile_count++ >= 34)
                        {
                            break;
                        }

                        uint n = t.tile_count - 1;
                        oam_tile[n].x = (ushort)sx;
                        oam_tile[n].priority = sprite.priority;
                        oam_tile[n].palette = (ushort)(128 + (sprite.palette << 4));
                        oam_tile[n].hflip = sprite.hflip;

                        uint mx = (sprite.hflip == false) ? tx : ((tile_width - 1) - tx);
                        uint pos = tiledata_addr + ((chry + ((chrx + mx) & 15)) << 5);
                        ushort addr = (ushort)((pos & 0xffe0) + ((y & 7) * 2));

                        oam_tile[n].d0 = StaticRAM.vram[addr + 0U];
                        oam_tile[n].d1 = StaticRAM.vram[addr + 1U];
                        self.add_clocks(2);

                        oam_tile[n].d2 = StaticRAM.vram[addr + 16U];
                        oam_tile[n].d3 = StaticRAM.vram[addr + 17U];
                        self.add_clocks(2);
                    }
                }

                if (t.tile_count < 34)
                {
                    self.add_clocks((34 - t.tile_count) * 4);
                }
                regs.time_over |= (t.tile_count > 34);
                regs.range_over |= (t.item_count > 32);
            }

            public void reset()
            {
                for (uint i = 0; i < 128; i++)
                {
                    list[i].x = 0;
                    list[i].y = 0;
                    list[i].character = 0;
                    list[i].nameselect = Convert.ToBoolean(0);
                    list[i].vflip = Convert.ToBoolean(0);
                    list[i].hflip = Convert.ToBoolean(0);
                    list[i].priority = 0;
                    list[i].palette = 0;
                    list[i].size = Convert.ToBoolean(0);
                }

                t.x = 0;
                t.y = 0;

                t.item_count = 0;
                t.tile_count = 0;

                t.active = Convert.ToBoolean(0);
                for (uint n = 0; n < 2; n++)
                {
                    Array.Clear(t.item[n], 0, t.item[n].Length);
                    for (uint i = 0; i < 34; i++)
                    {
                        t.tile[n][i].x = 0;
                        t.tile[n][i].priority = 0;
                        t.tile[n][i].palette = 0;
                        t.tile[n][i].hflip = Convert.ToBoolean(0);
                        t.tile[n][i].d0 = 0;
                        t.tile[n][i].d1 = 0;
                        t.tile[n][i].d2 = 0;
                        t.tile[n][i].d3 = 0;
                    }
                }

                regs.main_enable = Convert.ToBoolean(0);
                regs.sub_enable = Convert.ToBoolean(0);
                regs.interlace = Convert.ToBoolean(0);

                regs.base_size = 0;
                regs.nameselect = 0;
                regs.tiledata_addr = 0;
                regs.first_sprite = 0;

                regs.priority0 = 0;
                regs.priority1 = 0;
                regs.priority2 = 0;
                regs.priority3 = 0;

                regs.time_over = Convert.ToBoolean(0);
                regs.range_over = Convert.ToBoolean(0);

                output.main.palette = 0;
                output.main.priority = 0;
                output.sub.palette = 0;
                output.sub.priority = 0;
            }

            public void serialize(Serializer s)
            {
                for (uint i = 0; i < 128; i++)
                {
                    s.integer(list[i].x, "list[i].x");
                    s.integer(list[i].y, "list[i].y");
                    s.integer(list[i].character, "list[i].character");
                    s.integer(list[i].nameselect, "list[i].nameselect");
                    s.integer(list[i].vflip, "list[i].vflip");
                    s.integer(list[i].hflip, "list[i].hflip");
                    s.integer(list[i].priority, "list[i].priority");
                    s.integer(list[i].palette, "list[i].palette");
                    s.integer(list[i].size, "list[i].size");
                }

                s.integer(t.x, "t.x");
                s.integer(t.y, "t.y");

                s.integer(t.item_count, "t.item_count");
                s.integer(t.tile_count, "t.tile_count");

                s.integer(t.active, "t.active");
                for (uint n = 0; n < 2; n++)
                {
                    s.array(t.item[n], "t.item[n]");
                    for (uint i = 0; i < 34; i++)
                    {
                        s.integer(t.tile[n][i].x, "t.tile[n][i].x");
                        s.integer(t.tile[n][i].priority, "t.tile[n][i].priority");
                        s.integer(t.tile[n][i].palette, "t.tile[n][i].palette");
                        s.integer(t.tile[n][i].hflip, "t.tile[n][i].hflip");
                        s.integer(t.tile[n][i].d0, "t.tile[n][i].d0");
                        s.integer(t.tile[n][i].d1, "t.tile[n][i].d1");
                        s.integer(t.tile[n][i].d2, "t.tile[n][i].d2");
                        s.integer(t.tile[n][i].d3, "t.tile[n][i].d3");
                    }
                }

                s.integer(regs.main_enable, "regs.main_enabled");
                s.integer(regs.sub_enable, "regs.sub_enabled");
                s.integer(regs.interlace, "regs.interlace");

                s.integer(regs.base_size, "regs.base_size");
                s.integer(regs.nameselect, "regs.nameselect");
                s.integer(regs.tiledata_addr, "regs.tiledata_addr");
                s.integer(regs.first_sprite, "regs.first_sprite");

                s.integer(regs.priority0, "regs.priority0");
                s.integer(regs.priority1, "regs.priority1");
                s.integer(regs.priority2, "regs.priority2");
                s.integer(regs.priority3, "regs.priority3");

                s.integer(regs.time_over, "regs.time_over");
                s.integer(regs.range_over, "regs.range_over");

                s.integer(output.main.priority, "output.main.priority");
                s.integer(output.main.palette, "output.main.palette");

                s.integer(output.sub.priority, "output.sub.priority");
                s.integer(output.sub.palette, "output.sub.palette");
            }

            public Sprite(PPU self_)
            {
                self = self_;

                for (int i = 0; i < list.Length; i++)
                {
                    list[i] = new SpriteItem();
                }
            }

            public bool on_scanline(SpriteItem sprite)
            {
                if (sprite.x > 256 && (sprite.x + sprite.width() - 1) < 512)
                {
                    return false;
                }
                int height = (regs.interlace == false ? (int)sprite.height() : (int)(sprite.height() >> 1));
                if (t.y >= sprite.y && t.y < (sprite.y + height))
                {
                    return true;
                }
                if ((sprite.y + height) >= 256 && t.y < ((sprite.y + height) & 255))
                {
                    return true;
                }
                return false;
            }
        }
    }
}
#endif