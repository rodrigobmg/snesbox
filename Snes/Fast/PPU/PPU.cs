#if FAST_PPU
using System;
using System.Collections;
using System.Linq;
using Nall;

namespace Snes
{
    partial class PPU : IPPUCounter, IProcessor, IMMIO
    {
        public static PPU ppu = new PPU();

        public void step(uint clocks)
        {
            Processor.clock += clocks;
        }

        public IEnumerable synchronize_cpu()
        {
            if (Processor.clock >= 0 && Scheduler.scheduler.sync != Scheduler.SynchronizeMode.All)
            {
                yield return CPU.cpu.Processor.thread;
            }
        }

        public ushort get_vram_address()
        {
            ushort addr = regs.vram_addr;
            switch (regs.vram_mapping)
            {
                case 0:
                    break;  //direct mapping
                case 1:
                    addr = (ushort)((addr & 0xff00) | ((addr & 0x001f) << 3) | ((addr >> 5) & 7));
                    break;
                case 2:
                    addr = (ushort)((addr & 0xfe00) | ((addr & 0x003f) << 3) | ((addr >> 6) & 7));
                    break;
                case 3:
                    addr = (ushort)((addr & 0xfc00) | ((addr & 0x007f) << 3) | ((addr >> 7) & 7));
                    break;
            }
            return (ushort)(addr << 1);
        }

        public byte vram_mmio_read(ushort addr)
        {
            byte data;

            if (regs.display_disabled == true)
            {
                data = StaticRAM.vram[addr];
            }
            else
            {
                ushort v = CPU.cpu.PPUCounter.vcounter();
                ushort h = CPU.cpu.PPUCounter.hcounter();
                ushort ls = (ushort)(((System.system.region == System.Region.NTSC ? 525 : 625) >> 1) - 1);
                if (interlace() && !CPU.cpu.PPUCounter.field())
                {
                    ls++;
                }

                if (v == ls && h == 1362)
                {
                    data = 0x00;
                }
                else if (v < (!overscan() ? 224 : 239))
                {
                    data = 0x00;
                }
                else if (v == (!overscan() ? 224 : 239))
                {
                    if (h == 1362)
                    {
                        data = StaticRAM.vram[addr];
                    }
                    else
                    {
                        data = 0x00;
                    }
                }
                else
                {
                    data = StaticRAM.vram[addr];
                }
            }

            return data;
        }

        public void vram_mmio_write(ushort addr, byte data)
        {
            if (regs.display_disabled == true)
            {
                StaticRAM.vram[addr] = data;
            }
            else
            {
                ushort v = CPU.cpu.PPUCounter.vcounter();
                ushort h = CPU.cpu.PPUCounter.hcounter();
                if (v == 0)
                {
                    if (h <= 4)
                    {
                        StaticRAM.vram[addr] = data;
                    }
                    else if (h == 6)
                    {
                        StaticRAM.vram[addr] = CPU.cpu.regs.mdr;
                    }
                    else
                    {
                        //no write
                    }
                }
                else if (v < (!overscan() ? 225 : 240))
                {
                    //no write
                }
                else if (v == (!overscan() ? 225 : 240))
                {
                    if (h <= 4)
                    {
                        //no write
                    }
                    else
                    {
                        StaticRAM.vram[addr] = data;
                    }
                }
                else
                {
                    StaticRAM.vram[addr] = data;
                }
            }
        }

        public byte oam_mmio_read(ushort addr)
        {
            addr &= 0x03ff;
            if (Convert.ToBoolean(addr & 0x0200))
            {
                addr &= 0x021f;
            }
            byte data;

            if (regs.display_disabled == true)
            {
                data = StaticRAM.oam[addr];
            }
            else
            {
                if (CPU.cpu.PPUCounter.vcounter() < (!overscan() ? 225 : 240))
                {
                    data = StaticRAM.oam[regs.ioamaddr];
                }
                else
                {
                    data = StaticRAM.oam[addr];
                }
            }

            return data;
        }

        public void oam_mmio_write(ushort addr, byte data)
        {
            addr &= 0x03ff;
            if (Convert.ToBoolean(addr & 0x0200))
            {
                addr &= 0x021f;
            }

            sprite_list_valid = false;

            if (regs.display_disabled == true)
            {
                StaticRAM.oam[addr] = data;
                update_sprite_list(addr, data);
            }
            else
            {
                if (CPU.cpu.PPUCounter.vcounter() < (!overscan() ? 225 : 240))
                {
                    StaticRAM.oam[regs.ioamaddr] = data;
                    update_sprite_list(regs.ioamaddr, data);
                }
                else
                {
                    StaticRAM.oam[addr] = data;
                    update_sprite_list(addr, data);
                }
            }
        }

        public byte cgram_mmio_read(ushort addr)
        {
            addr &= 0x01ff;
            byte data;

            if (Convert.ToBoolean(1) || regs.display_disabled == true)
            {
                data = StaticRAM.cgram[addr];
            }
            else
            {
                ushort v = CPU.cpu.PPUCounter.vcounter();
                ushort h = CPU.cpu.PPUCounter.vcounter();
                if (v < (!overscan() ? 225 : 240) && h >= 128 && h < 1096)
                {
                    data = (byte)(StaticRAM.cgram[regs.icgramaddr] & 0x7f);
                }
                else
                {
                    data = StaticRAM.cgram[addr];
                }
            }

            if (Convert.ToBoolean(addr & 1))
            {
                data &= 0x7f;
            }
            return data;
        }

        public void cgram_mmio_write(ushort addr, byte data)
        {
            addr &= 0x01ff;
            if (Convert.ToBoolean(addr & 1))
            {
                data &= 0x7f;
            }

            if (Convert.ToBoolean(1) || regs.display_disabled == true)
            {
                StaticRAM.cgram[addr] = data;
            }
            else
            {
                ushort v = CPU.cpu.PPUCounter.vcounter();
                ushort h = CPU.cpu.PPUCounter.vcounter();
                if (v < (!overscan() ? 225 : 240) && h >= 128 && h < 1096)
                {
                    StaticRAM.cgram[regs.icgramaddr] = (byte)(data & 0x7f);
                }
                else
                {
                    StaticRAM.cgram[addr] = data;
                }
            }
        }

        public Regs regs = new Regs();

        public void mmio_w2100(byte value)
        {
            if (regs.display_disabled == true && CPU.cpu.PPUCounter.vcounter() == (!overscan() ? 225 : 240))
            {
                regs.oam_addr = (ushort)(regs.oam_baseaddr << 1);
                regs.oam_firstsprite = (byte)((regs.oam_priority == false) ? 0 : (regs.oam_addr >> 2) & 127);
            }

            regs.display_disabled = !!Convert.ToBoolean(value & 0x80);
            regs.display_brightness = (byte)(value & 15);
        }  //INIDISP

        public void mmio_w2101(byte value)
        {
            regs.oam_basesize = (byte)((value >> 5) & 7);
            regs.oam_nameselect = (byte)((value >> 3) & 3);
            regs.oam_tdaddr = (ushort)((value & 3) << 14);
        }  //OBSEL

        public void mmio_w2102(byte data)
        {
            regs.oam_baseaddr = (ushort)((regs.oam_baseaddr & ~0xff) | (data << 0));
            regs.oam_baseaddr &= 0x01ff;
            regs.oam_addr = (ushort)(regs.oam_baseaddr << 1);
            regs.oam_firstsprite = (byte)((regs.oam_priority == false) ? 0 : (regs.oam_addr >> 2) & 127);
        }  //OAMADDL

        public void mmio_w2103(byte data)
        {
            regs.oam_priority = !!Convert.ToBoolean(data & 0x80);
            regs.oam_baseaddr = (ushort)((regs.oam_baseaddr & 0xff) | (data << 8));
            regs.oam_baseaddr &= 0x01ff;
            regs.oam_addr = (ushort)(regs.oam_baseaddr << 1);
            regs.oam_firstsprite = (byte)((regs.oam_priority == false) ? 0 : (regs.oam_addr >> 2) & 127);
        }  //OAMADDH

        public void mmio_w2104(byte data)
        {
            if (Convert.ToBoolean(regs.oam_addr & 0x0200))
            {
                oam_mmio_write(regs.oam_addr, data);
            }
            else if ((regs.oam_addr & 1) == 0)
            {
                regs.oam_latchdata = data;
            }
            else
            {
                oam_mmio_write((ushort)((regs.oam_addr & ~1) + 0), regs.oam_latchdata);
                oam_mmio_write((ushort)((regs.oam_addr & ~1) + 1), data);
            }

            regs.oam_addr++;
            regs.oam_addr &= 0x03ff;
            regs.oam_firstsprite = (byte)((regs.oam_priority == false) ? 0 : (regs.oam_addr >> 2) & 127);
        }  //OAMDATA

        public void mmio_w2105(byte value)
        {
            regs.bg_tilesize[(int)ID.BG4] = !!Convert.ToBoolean(value & 0x80);
            regs.bg_tilesize[(int)ID.BG3] = !!Convert.ToBoolean(value & 0x40);
            regs.bg_tilesize[(int)ID.BG2] = !!Convert.ToBoolean(value & 0x20);
            regs.bg_tilesize[(int)ID.BG1] = !!Convert.ToBoolean(value & 0x10);
            regs.bg3_priority = !!Convert.ToBoolean(value & 0x08);
            regs.bg_mode = (byte)(value & 7);
        }  //BGMODE

        public void mmio_w2106(byte value)
        {
            regs.mosaic_size = (byte)((value >> 4) & 15);
            regs.mosaic_enabled[(int)ID.BG4] = !!Convert.ToBoolean(value & 0x08);
            regs.mosaic_enabled[(int)ID.BG3] = !!Convert.ToBoolean(value & 0x04);
            regs.mosaic_enabled[(int)ID.BG2] = !!Convert.ToBoolean(value & 0x02);
            regs.mosaic_enabled[(int)ID.BG1] = !!Convert.ToBoolean(value & 0x01);
        }  //MOSAIC

        public void mmio_w2107(byte value)
        {
            regs.bg_scaddr[(int)ID.BG1] = (ushort)((value & 0x7c) << 9);
            regs.bg_scsize[(int)ID.BG1] = (byte)(value & 3);
        }  //BG1SC

        public void mmio_w2108(byte value)
        {
            regs.bg_scaddr[(int)ID.BG2] = (ushort)((value & 0x7c) << 9);
            regs.bg_scsize[(int)ID.BG2] = (byte)(value & 3);
        }  //BG2SC

        public void mmio_w2109(byte value)
        {
            regs.bg_scaddr[(int)ID.BG3] = (ushort)((value & 0x7c) << 9);
            regs.bg_scsize[(int)ID.BG3] = (byte)(value & 3);
        }  //BG3SC

        public void mmio_w210a(byte value)
        {
            regs.bg_scaddr[(int)ID.BG4] = (ushort)((value & 0x7c) << 9);
            regs.bg_scsize[(int)ID.BG4] = (byte)(value & 3);
        }  //BG4SC

        public void mmio_w210b(byte value)
        {
            regs.bg_tdaddr[(int)ID.BG1] = (ushort)((value & 0x07) << 13);
            regs.bg_tdaddr[(int)ID.BG2] = (ushort)((value & 0x70) << 9);
        }  //BG12NBA

        public void mmio_w210c(byte value)
        {
            regs.bg_tdaddr[(int)ID.BG3] = (ushort)((value & 0x07) << 13);
            regs.bg_tdaddr[(int)ID.BG4] = (ushort)((value & 0x70) << 9);
        }  //BG34NBA

        public void mmio_w210d(byte value)
        {
            regs.m7_hofs = (ushort)((value << 8) | regs.m7_latch);
            regs.m7_latch = value;

            regs.bg_hofs[(int)ID.BG1] = (ushort)((value << 8) | (regs.bg_ofslatch & ~7) | ((regs.bg_hofs[(int)ID.BG1] >> 8) & 7));
            regs.bg_ofslatch = value;
        }  //BG1HOFS

        public void mmio_w210e(byte value)
        {
            regs.m7_vofs = (ushort)((value << 8) | regs.m7_latch);
            regs.m7_latch = value;

            regs.bg_vofs[(int)ID.BG1] = (ushort)((value << 8) | (regs.bg_ofslatch));
            regs.bg_ofslatch = value;
        }  //BG1VOFS

        public void mmio_w210f(byte value)
        {
            regs.bg_hofs[(int)ID.BG2] = (ushort)((value << 8) | (regs.bg_ofslatch & ~7) | ((regs.bg_hofs[(int)ID.BG2] >> 8) & 7));
            regs.bg_ofslatch = value;
        }  //BG2HOFS

        public void mmio_w2110(byte value)
        {
            regs.bg_vofs[(int)ID.BG2] = (ushort)((value << 8) | (regs.bg_ofslatch));
            regs.bg_ofslatch = value;
        }  //BG2VOFS

        public void mmio_w2111(byte value)
        {
            regs.bg_hofs[(int)ID.BG3] = (ushort)((value << 8) | (regs.bg_ofslatch & ~7) | ((regs.bg_hofs[(int)ID.BG3] >> 8) & 7));
            regs.bg_ofslatch = value;
        }  //BG3HOFS

        public void mmio_w2112(byte value)
        {
            regs.bg_vofs[(int)ID.BG3] = (ushort)((value << 8) | (regs.bg_ofslatch));
            regs.bg_ofslatch = value;
        }  //BG3VOFS

        public void mmio_w2113(byte value)
        {
            regs.bg_hofs[(int)ID.BG4] = (ushort)((value << 8) | (regs.bg_ofslatch & ~7) | ((regs.bg_hofs[(int)ID.BG4] >> 8) & 7));
            regs.bg_ofslatch = value;
        }  //BG4HOFS

        public void mmio_w2114(byte value)
        {
            regs.bg_vofs[(int)ID.BG4] = (ushort)((value << 8) | (regs.bg_ofslatch));
            regs.bg_ofslatch = value;
        }  //BG4VOFS

        public void mmio_w2115(byte value)
        {
            regs.vram_incmode = !!Convert.ToBoolean(value & 0x80);
            regs.vram_mapping = (byte)((value >> 2) & 3);
            switch (value & 3)
            {
                case 0:
                    regs.vram_incsize = 1;
                    break;
                case 1:
                    regs.vram_incsize = 32;
                    break;
                case 2:
                    regs.vram_incsize = 128;
                    break;
                case 3:
                    regs.vram_incsize = 128;
                    break;
            }
        }  //VMAIN

        public void mmio_w2116(byte value)
        {
            regs.vram_addr = (ushort)((regs.vram_addr & 0xff00) | value);
            ushort addr = get_vram_address();
            regs.vram_readbuffer = vram_mmio_read((ushort)(addr + 0));
            regs.vram_readbuffer |= (ushort)(vram_mmio_read((ushort)(addr + 1)) << 8);
        }  //VMADDL

        public void mmio_w2117(byte value)
        {
            regs.vram_addr = (ushort)((value << 8) | (regs.vram_addr & 0x00ff));
            ushort addr = get_vram_address();
            regs.vram_readbuffer = vram_mmio_read((ushort)(addr + 0));
            regs.vram_readbuffer |= (ushort)(vram_mmio_read((ushort)(addr + 1)) << 8);
        }  //VMADDH

        public void mmio_w2118(byte value)
        {
            ushort addr = get_vram_address();
            vram_mmio_write(addr, value);
            bg_tiledata_state[(int)Tile.T2BIT][(addr >> 4)] = 1;
            bg_tiledata_state[(int)Tile.T4BIT][(addr >> 5)] = 1;
            bg_tiledata_state[(int)Tile.T8BIT][(addr >> 6)] = 1;

            if (regs.vram_incmode == Convert.ToBoolean(0))
            {
                regs.vram_addr += regs.vram_incsize;
            }
        }  //VMDATAL

        public void mmio_w2119(byte value)
        {
            ushort addr = (ushort)(get_vram_address() + 1);
            vram_mmio_write(addr, value);
            bg_tiledata_state[(int)Tile.T2BIT][(addr >> 4)] = 1;
            bg_tiledata_state[(int)Tile.T4BIT][(addr >> 5)] = 1;
            bg_tiledata_state[(int)Tile.T8BIT][(addr >> 6)] = 1;

            if (regs.vram_incmode == Convert.ToBoolean(1))
            {
                regs.vram_addr += regs.vram_incsize;
            }
        }  //VMDATAH

        public void mmio_w211a(byte value)
        {
            regs.mode7_repeat = (byte)((value >> 6) & 3);
            regs.mode7_vflip = !!Convert.ToBoolean(value & 0x02);
            regs.mode7_hflip = !!Convert.ToBoolean(value & 0x01);
        }  //M7SEL

        public void mmio_w211b(byte value)
        {
            regs.m7a = (ushort)((value << 8) | regs.m7_latch);
            regs.m7_latch = value;
        }  //M7A

        public void mmio_w211c(byte value)
        {
            regs.m7b = (ushort)((value << 8) | regs.m7_latch);
            regs.m7_latch = value;
        }  //M7B

        public void mmio_w211d(byte value)
        {
            regs.m7c = (ushort)((value << 8) | regs.m7_latch);
            regs.m7_latch = value;
        }  //M7C

        public void mmio_w211e(byte value)
        {
            regs.m7d = (ushort)((value << 8) | regs.m7_latch);
            regs.m7_latch = value;
        }  //M7D

        public void mmio_w211f(byte value)
        {
            regs.m7x = (ushort)((value << 8) | regs.m7_latch);
            regs.m7_latch = value;
        }  //M7X

        public void mmio_w2120(byte value)
        {
            regs.m7y = (ushort)((value << 8) | regs.m7_latch);
            regs.m7_latch = value;
        }  //M7Y

        public void mmio_w2121(byte value)
        {
            regs.cgram_addr = (ushort)(value << 1);
        }  //CGADD

        public void mmio_w2122(byte value)
        {
            if (!Convert.ToBoolean(regs.cgram_addr & 1))
            {
                regs.cgram_latchdata = value;
            }
            else
            {
                cgram_mmio_write((ushort)(regs.cgram_addr & 0x01fe), regs.cgram_latchdata);
                cgram_mmio_write((ushort)((regs.cgram_addr & 0x01fe) + 1), (byte)(value & 0x7f));
            }
            regs.cgram_addr++;
            regs.cgram_addr &= 0x01ff;
        }  //CGDATA

        public void mmio_w2123(byte value)
        {
            regs.window2_enabled[(int)ID.BG2] = !!Convert.ToBoolean(value & 0x80);
            regs.window2_invert[(int)ID.BG2] = !!Convert.ToBoolean(value & 0x40);
            regs.window1_enabled[(int)ID.BG2] = !!Convert.ToBoolean(value & 0x20);
            regs.window1_invert[(int)ID.BG2] = !!Convert.ToBoolean(value & 0x10);
            regs.window2_enabled[(int)ID.BG1] = !!Convert.ToBoolean(value & 0x08);
            regs.window2_invert[(int)ID.BG1] = !!Convert.ToBoolean(value & 0x04);
            regs.window1_enabled[(int)ID.BG1] = !!Convert.ToBoolean(value & 0x02);
            regs.window1_invert[(int)ID.BG1] = !!Convert.ToBoolean(value & 0x01);
        }  //W12SEL

        public void mmio_w2124(byte value)
        {
            regs.window2_enabled[(int)ID.BG4] = !!Convert.ToBoolean(value & 0x80);
            regs.window2_invert[(int)ID.BG4] = !!Convert.ToBoolean(value & 0x40);
            regs.window1_enabled[(int)ID.BG4] = !!Convert.ToBoolean(value & 0x20);
            regs.window1_invert[(int)ID.BG4] = !!Convert.ToBoolean(value & 0x10);
            regs.window2_enabled[(int)ID.BG3] = !!Convert.ToBoolean(value & 0x08);
            regs.window2_invert[(int)ID.BG3] = !!Convert.ToBoolean(value & 0x04);
            regs.window1_enabled[(int)ID.BG3] = !!Convert.ToBoolean(value & 0x02);
            regs.window1_invert[(int)ID.BG3] = !!Convert.ToBoolean(value & 0x01);
        }  //W34SEL

        public void mmio_w2125(byte value)
        {
            regs.window2_enabled[(int)ID.COL] = !!Convert.ToBoolean(value & 0x80);
            regs.window2_invert[(int)ID.COL] = !!Convert.ToBoolean(value & 0x40);
            regs.window1_enabled[(int)ID.COL] = !!Convert.ToBoolean(value & 0x20);
            regs.window1_invert[(int)ID.COL] = !!Convert.ToBoolean(value & 0x10);
            regs.window2_enabled[(int)ID.OAM] = !!Convert.ToBoolean(value & 0x08);
            regs.window2_invert[(int)ID.OAM] = !!Convert.ToBoolean(value & 0x04);
            regs.window1_enabled[(int)ID.OAM] = !!Convert.ToBoolean(value & 0x02);
            regs.window1_invert[(int)ID.OAM] = !!Convert.ToBoolean(value & 0x01);
        }  //WOBJSEL

        public void mmio_w2126(byte value)
        {
            regs.window1_left = value;
        }  //WH0

        public void mmio_w2127(byte value)
        {
            regs.window1_right = value;
        }  //WH1

        public void mmio_w2128(byte value)
        {
            regs.window2_left = value;
        }  //WH2

        public void mmio_w2129(byte value)
        {
            regs.window2_right = value;
        }  //WH3

        public void mmio_w212a(byte value)
        {
            regs.window_mask[(int)ID.BG4] = (byte)((value >> 6) & 3);
            regs.window_mask[(int)ID.BG3] = (byte)((value >> 4) & 3);
            regs.window_mask[(int)ID.BG2] = (byte)((value >> 2) & 3);
            regs.window_mask[(int)ID.BG1] = (byte)((value) & 3);
        }  //WBGLOG

        public void mmio_w212b(byte value)
        {
            regs.window_mask[(int)ID.COL] = (byte)((value >> 2) & 3);
            regs.window_mask[(int)ID.OAM] = (byte)((value) & 3);
        }  //WOBJLOG

        public void mmio_w212c(byte value)
        {
            regs.bg_enabled[(int)ID.OAM] = !!Convert.ToBoolean(value & 0x10);
            regs.bg_enabled[(int)ID.BG4] = !!Convert.ToBoolean(value & 0x08);
            regs.bg_enabled[(int)ID.BG3] = !!Convert.ToBoolean(value & 0x04);
            regs.bg_enabled[(int)ID.BG2] = !!Convert.ToBoolean(value & 0x02);
            regs.bg_enabled[(int)ID.BG1] = !!Convert.ToBoolean(value & 0x01);
        }  //TM

        public void mmio_w212d(byte value)
        {
            regs.bgsub_enabled[(int)ID.OAM] = !!Convert.ToBoolean(value & 0x10);
            regs.bgsub_enabled[(int)ID.BG4] = !!Convert.ToBoolean(value & 0x08);
            regs.bgsub_enabled[(int)ID.BG3] = !!Convert.ToBoolean(value & 0x04);
            regs.bgsub_enabled[(int)ID.BG2] = !!Convert.ToBoolean(value & 0x02);
            regs.bgsub_enabled[(int)ID.BG1] = !!Convert.ToBoolean(value & 0x01);
        }  //TS

        public void mmio_w212e(byte value)
        {
            regs.window_enabled[(int)ID.OAM] = !!Convert.ToBoolean(value & 0x10);
            regs.window_enabled[(int)ID.BG4] = !!Convert.ToBoolean(value & 0x08);
            regs.window_enabled[(int)ID.BG3] = !!Convert.ToBoolean(value & 0x04);
            regs.window_enabled[(int)ID.BG2] = !!Convert.ToBoolean(value & 0x02);
            regs.window_enabled[(int)ID.BG1] = !!Convert.ToBoolean(value & 0x01);
        }  //TMW

        public void mmio_w212f(byte value)
        {
            regs.sub_window_enabled[(int)ID.OAM] = !!Convert.ToBoolean(value & 0x10);
            regs.sub_window_enabled[(int)ID.BG4] = !!Convert.ToBoolean(value & 0x08);
            regs.sub_window_enabled[(int)ID.BG3] = !!Convert.ToBoolean(value & 0x04);
            regs.sub_window_enabled[(int)ID.BG2] = !!Convert.ToBoolean(value & 0x02);
            regs.sub_window_enabled[(int)ID.BG1] = !!Convert.ToBoolean(value & 0x01);
        }  //TSW

        public void mmio_w2130(byte value)
        {
            regs.color_mask = (byte)((value >> 6) & 3);
            regs.colorsub_mask = (byte)((value >> 4) & 3);
            regs.addsub_mode = !!Convert.ToBoolean(value & 0x02);
            regs.direct_color = !!Convert.ToBoolean(value & 0x01);
        }  //CGWSEL

        public void mmio_w2131(byte value)
        {
            regs.color_mode = !!Convert.ToBoolean(value & 0x80);
            regs.color_halve = !!Convert.ToBoolean(value & 0x40);
            regs.color_enabled[(int)ID.BACK] = !!Convert.ToBoolean(value & 0x20);
            regs.color_enabled[(int)ID.OAM] = !!Convert.ToBoolean(value & 0x10);
            regs.color_enabled[(int)ID.BG4] = !!Convert.ToBoolean(value & 0x08);
            regs.color_enabled[(int)ID.BG3] = !!Convert.ToBoolean(value & 0x04);
            regs.color_enabled[(int)ID.BG2] = !!Convert.ToBoolean(value & 0x02);
            regs.color_enabled[(int)ID.BG1] = !!Convert.ToBoolean(value & 0x01);
        }  //CGADDSUB

        public void mmio_w2132(byte value)
        {
            if (Convert.ToBoolean(value & 0x80))
            {
                regs.color_b = (byte)(value & 0x1f);
            }
            if (Convert.ToBoolean(value & 0x40))
            {
                regs.color_g = (byte)(value & 0x1f);
            }
            if (Convert.ToBoolean(value & 0x20))
            {
                regs.color_r = (byte)(value & 0x1f);
            }

            regs.color_rgb = (ushort)((regs.color_r)
                           | (regs.color_g << 5)
                           | (regs.color_b << 10));
        }  //COLDATA

        public void mmio_w2133(byte value)
        {
            regs.mode7_extbg = !!Convert.ToBoolean(value & 0x40);
            regs.pseudo_hires = !!Convert.ToBoolean(value & 0x08);
            regs.overscan = !!Convert.ToBoolean(value & 0x04);
            regs.oam_interlace = !!Convert.ToBoolean(value & 0x02);
            regs.interlace = !!Convert.ToBoolean(value & 0x01);

            display.overscan = regs.overscan;
            sprite_list_valid = false;
        }  //SETINI

        public byte mmio_r2134()
        {
            uint r;
            r = (uint)((short)regs.m7a * (sbyte)(regs.m7b >> 8));
            regs.ppu1_mdr = (byte)r;
            return regs.ppu1_mdr;
        }  //MPYL

        public byte mmio_r2135()
        {
            uint r;
            r = (uint)((short)regs.m7a * (sbyte)(regs.m7b >> 8));
            regs.ppu1_mdr = (byte)(r >> 8);
            return regs.ppu1_mdr;
        }  //MPYM

        public byte mmio_r2136()
        {
            uint r;
            r = (uint)((short)regs.m7a * (sbyte)(regs.m7b >> 8));
            regs.ppu1_mdr = (byte)(r >> 16);
            return regs.ppu1_mdr;
        }  //MPYH

        public byte mmio_r2137()
        {
            if (Convert.ToBoolean(CPU.cpu.pio() & 0x80))
            {
                latch_counters();
            }
            return CPU.cpu.regs.mdr;
        }  //SLHV

        public byte mmio_r2138()
        {
            regs.ppu1_mdr = oam_mmio_read(regs.oam_addr);

            regs.oam_addr++;
            regs.oam_addr &= 0x03ff;
            regs.oam_firstsprite = (byte)((regs.oam_priority == false) ? 0 : (regs.oam_addr >> 2) & 127);

            return regs.ppu1_mdr;
        }  //OAMDATAREAD

        public byte mmio_r2139()
        {
            ushort addr = get_vram_address();
            regs.ppu1_mdr = (byte)regs.vram_readbuffer;
            if (regs.vram_incmode == Convert.ToBoolean(0))
            {
                addr &= 0xfffe;
                regs.vram_readbuffer = vram_mmio_read((ushort)(addr + 0));
                regs.vram_readbuffer |= (ushort)(vram_mmio_read((ushort)(addr + 1)) << 8);
                regs.vram_addr += regs.vram_incsize;
            }
            return regs.ppu1_mdr;
        }  //VMDATALREAD

        public byte mmio_r213a()
        {
            ushort addr = (ushort)(get_vram_address() + 1);
            regs.ppu1_mdr = (byte)(regs.vram_readbuffer >> 8);
            if (regs.vram_incmode == Convert.ToBoolean(1))
            {
                addr &= 0xfffe;
                regs.vram_readbuffer = vram_mmio_read((ushort)(addr + 0));
                regs.vram_readbuffer |= (ushort)(vram_mmio_read((ushort)(addr + 1)) << 8);
                regs.vram_addr += regs.vram_incsize;
            }
            return regs.ppu1_mdr;
        }  //VMDATAHREAD

        public byte mmio_r213b()
        {
            if (!Convert.ToBoolean(regs.cgram_addr & 1))
            {
                regs.ppu2_mdr = (byte)(cgram_mmio_read(regs.cgram_addr) & 0xff);
            }
            else
            {
                regs.ppu2_mdr &= 0x80;
                regs.ppu2_mdr |= (byte)(cgram_mmio_read(regs.cgram_addr) & 0x7f);
            }
            regs.cgram_addr++;
            regs.cgram_addr &= 0x01ff;
            return regs.ppu2_mdr;
        }  //CGDATAREAD

        public byte mmio_r213c()
        {
            if (!regs.latch_hcounter)
            {
                regs.ppu2_mdr = (byte)(regs.hcounter & 0xff);
            }
            else
            {
                regs.ppu2_mdr &= 0xfe;
                regs.ppu2_mdr |= (byte)((regs.hcounter >> 8) & 1);
            }
            regs.latch_hcounter ^= Convert.ToBoolean(1);
            return regs.ppu2_mdr;
        }  //OPHCT

        public byte mmio_r213d()
        {
            if (!regs.latch_vcounter)
            {
                regs.ppu2_mdr = (byte)(regs.vcounter & 0xff);
            }
            else
            {
                regs.ppu2_mdr &= 0xfe;
                regs.ppu2_mdr |= (byte)((regs.vcounter >> 8) & 1);
            }
            regs.latch_vcounter ^= Convert.ToBoolean(1);
            return regs.ppu2_mdr;
        }  //OPVCT

        public byte mmio_r213e()
        {
            byte r = 0x00;
            r |= (byte)((regs.time_over) ? 0x80 : 0x00);
            r |= (byte)((regs.range_over) ? 0x40 : 0x00);
            r |= (byte)((regs.ppu1_mdr & 0x10));
            r |= (byte)((ppu1_version & 0x0f));
            regs.ppu1_mdr = r;
            return regs.ppu1_mdr;
        }  //STAT77

        public byte mmio_r213f()
        {
            byte r = 0x00;
            regs.latch_hcounter = Convert.ToBoolean(0);
            regs.latch_vcounter = Convert.ToBoolean(0);

            r |= (byte)(Convert.ToUInt32(CPU.cpu.PPUCounter.field()) << 7);
            if (!Convert.ToBoolean(CPU.cpu.pio() & 0x80))
            {
                r |= 0x40;
            }
            else if (regs.counters_latched == true)
            {
                r |= 0x40;
                regs.counters_latched = false;
            }
            r |= (byte)((regs.ppu2_mdr & 0x20));
            r |= (byte)((region << 4)); //0 = NTSC, 1 = PAL
            r |= (byte)((ppu2_version & 0x0f));
            regs.ppu2_mdr = r;
            return regs.ppu2_mdr;
        }  //STAT78

        public IEnumerable mmio_read(uint addr, Result result)
        {
            foreach (var e in CPU.cpu.synchronize_ppu())
            {
                yield return e;
            };

            switch (addr & 0xffff)
            {
                case 0x2104:
                case 0x2105:
                case 0x2106:
                case 0x2108:
                case 0x2109:
                case 0x210a:
                case 0x2114:
                case 0x2115:
                case 0x2116:
                case 0x2118:
                case 0x2119:
                case 0x211a:
                case 0x2124:
                case 0x2125:
                case 0x2126:
                case 0x2128:
                case 0x2129:
                case 0x212a:
                    result.Value = regs.ppu1_mdr;
                    yield break;
                case 0x2134:
                    result.Value = mmio_r2134();  //MPYL
                    yield break;
                case 0x2135:
                    result.Value = mmio_r2135();  //MPYM
                    yield break;
                case 0x2136:
                    result.Value = mmio_r2136();  //MPYH
                    yield break;
                case 0x2137:
                    result.Value = mmio_r2137();  //SLHV
                    yield break;
                case 0x2138:
                    result.Value = mmio_r2138();  //OAMDATAREAD
                    yield break;
                case 0x2139:
                    result.Value = mmio_r2139();  //VMDATALREAD
                    yield break;
                case 0x213a:
                    result.Value = mmio_r213a();  //VMDATAHREAD
                    yield break;
                case 0x213b:
                    result.Value = mmio_r213b();  //CGDATAREAD
                    yield break;
                case 0x213c:
                    result.Value = mmio_r213c();  //OPHCT
                    yield break;
                case 0x213d:
                    result.Value = mmio_r213d();  //OPVCT
                    yield break;
                case 0x213e:
                    result.Value = mmio_r213e();  //STAT77
                    yield break;
                case 0x213f:
                    result.Value = mmio_r213f();  //STAT78
                    yield break;
            }

            result.Value = CPU.cpu.regs.mdr;
        }

        public IEnumerable mmio_write(uint addr, byte data)
        {
            foreach (var e in CPU.cpu.synchronize_ppu())
            {
                yield return e;
            };

            switch (addr & 0xffff)
            {
                case 0x2100:
                    mmio_w2100(data);
                    yield break;  //INIDISP
                case 0x2101:
                    mmio_w2101(data);
                    yield break;  //OBSEL
                case 0x2102:
                    mmio_w2102(data);
                    yield break;  //OAMADDL
                case 0x2103:
                    mmio_w2103(data);
                    yield break;  //OAMADDH
                case 0x2104:
                    mmio_w2104(data);
                    yield break;  //OAMDATA
                case 0x2105:
                    mmio_w2105(data);
                    yield break;  //(int)ID.BGMODE
                case 0x2106:
                    mmio_w2106(data);
                    yield break;  //MOSAIC
                case 0x2107:
                    mmio_w2107(data);
                    yield break;  //(int)ID.BG1SC
                case 0x2108:
                    mmio_w2108(data);
                    yield break;  //(int)ID.BG2SC
                case 0x2109:
                    mmio_w2109(data);
                    yield break;  //(int)ID.BG3SC
                case 0x210a:
                    mmio_w210a(data);
                    yield break;  //(int)ID.BG4SC
                case 0x210b:
                    mmio_w210b(data);
                    yield break;  //(int)ID.BG12NBA
                case 0x210c:
                    mmio_w210c(data);
                    yield break;  //(int)ID.BG34NBA
                case 0x210d:
                    mmio_w210d(data);
                    yield break;  //(int)ID.BG1HOFS
                case 0x210e:
                    mmio_w210e(data);
                    yield break;  //(int)ID.BG1VOFS
                case 0x210f:
                    mmio_w210f(data);
                    yield break;  //(int)ID.BG2HOFS
                case 0x2110:
                    mmio_w2110(data);
                    yield break;  //(int)ID.BG2VOFS
                case 0x2111:
                    mmio_w2111(data);
                    yield break;  //(int)ID.BG3HOFS
                case 0x2112:
                    mmio_w2112(data);
                    yield break;  //(int)ID.BG3VOFS
                case 0x2113:
                    mmio_w2113(data);
                    yield break;  //(int)ID.BG4HOFS
                case 0x2114:
                    mmio_w2114(data);
                    yield break;  //(int)ID.BG4VOFS
                case 0x2115:
                    mmio_w2115(data);
                    yield break;  //VMAIN
                case 0x2116:
                    mmio_w2116(data);
                    yield break;  //VMADDL
                case 0x2117:
                    mmio_w2117(data);
                    yield break;  //VMADDH
                case 0x2118:
                    mmio_w2118(data);
                    yield break;  //VMDATAL
                case 0x2119:
                    mmio_w2119(data);
                    yield break;  //VMDATAH
                case 0x211a:
                    mmio_w211a(data);
                    yield break;  //M7SEL
                case 0x211b:
                    mmio_w211b(data);
                    yield break;  //M7A
                case 0x211c:
                    mmio_w211c(data);
                    yield break;  //M7B
                case 0x211d:
                    mmio_w211d(data);
                    yield break;  //M7C
                case 0x211e:
                    mmio_w211e(data);
                    yield break;  //M7D
                case 0x211f:
                    mmio_w211f(data);
                    yield break;  //M7X
                case 0x2120:
                    mmio_w2120(data);
                    yield break;  //M7Y
                case 0x2121:
                    mmio_w2121(data);
                    yield break;  //CGADD
                case 0x2122:
                    mmio_w2122(data);
                    yield break;  //CGDATA
                case 0x2123:
                    mmio_w2123(data);
                    yield break;  //W12SEL
                case 0x2124:
                    mmio_w2124(data);
                    yield break;  //W34SEL
                case 0x2125:
                    mmio_w2125(data);
                    yield break;  //WOBJSEL
                case 0x2126:
                    mmio_w2126(data);
                    yield break;  //WH0
                case 0x2127:
                    mmio_w2127(data);
                    yield break;  //WH1
                case 0x2128:
                    mmio_w2128(data);
                    yield break;  //WH2
                case 0x2129:
                    mmio_w2129(data);
                    yield break;  //WH3
                case 0x212a:
                    mmio_w212a(data);
                    yield break;  //W(int)ID.BGLOG
                case 0x212b:
                    mmio_w212b(data);
                    yield break;  //WOBJLOG
                case 0x212c:
                    mmio_w212c(data);
                    yield break;  //TM
                case 0x212d:
                    mmio_w212d(data);
                    yield break;  //TS
                case 0x212e:
                    mmio_w212e(data);
                    yield break;  //TMW
                case 0x212f:
                    mmio_w212f(data);
                    yield break;  //TSW
                case 0x2130:
                    mmio_w2130(data);
                    yield break;  //CGWSEL
                case 0x2131:
                    mmio_w2131(data);
                    yield break;  //CGADDSUB
                case 0x2132:
                    mmio_w2132(data);
                    yield break;  //COLDATA
                case 0x2133:
                    mmio_w2133(data);
                    yield break;  //SETINI
            }
        }

        public void latch_counters()
        {
            regs.hcounter = CPU.cpu.PPUCounter.hdot();
            regs.vcounter = CPU.cpu.PPUCounter.vcounter();
            regs.counters_latched = true;
        }

        //render.cpp
        public void render_line_mode0()
        {
            render_line_bg(0, (uint)ID.BG1, (uint)ColorDepth.D4, 8, 11);
            render_line_bg(0, (uint)ID.BG2, (uint)ColorDepth.D4, 7, 10);
            render_line_bg(0, (uint)ID.BG3, (uint)ColorDepth.D4, 2, 5);
            render_line_bg(0, (uint)ID.BG4, (uint)ColorDepth.D4, 1, 4);
            render_line_oam(3, 6, 9, 12);
        }

        public void render_line_mode1()
        {
            if (regs.bg3_priority)
            {
                render_line_bg(1, (uint)ID.BG1, (uint)ColorDepth.D16, 5, 8);
                render_line_bg(1, (uint)ID.BG2, (uint)ColorDepth.D16, 4, 7);
                render_line_bg(1, (uint)ID.BG3, (uint)ColorDepth.D4, 1, 10);
                render_line_oam(2, 3, 6, 9);
            }
            else
            {
                render_line_bg(1, (uint)ID.BG1, (uint)ColorDepth.D16, 6, 9);
                render_line_bg(1, (uint)ID.BG2, (uint)ColorDepth.D16, 5, 8);
                render_line_bg(1, (uint)ID.BG3, (uint)ColorDepth.D4, 1, 3);
                render_line_oam(2, 4, 7, 10);
            }
        }

        public void render_line_mode2()
        {
            render_line_bg(2, (uint)ID.BG1, (uint)ColorDepth.D16, 3, 7);
            render_line_bg(2, (uint)ID.BG2, (uint)ColorDepth.D16, 1, 5);
            render_line_oam(2, 4, 6, 8);
        }

        public void render_line_mode3()
        {
            render_line_bg(3, (uint)ID.BG1, (uint)ColorDepth.D256, 3, 7);
            render_line_bg(3, (uint)ID.BG2, (uint)ColorDepth.D16, 1, 5);
            render_line_oam(2, 4, 6, 8);
        }

        public void render_line_mode4()
        {
            render_line_bg(4, (uint)ID.BG1, (uint)ColorDepth.D256, 3, 7);
            render_line_bg(4, (uint)ID.BG2, (uint)ColorDepth.D4, 1, 5);
            render_line_oam(2, 4, 6, 8);
        }

        public void render_line_mode5()
        {
            render_line_bg(5, (uint)ID.BG1, (uint)ColorDepth.D16, 3, 7);
            render_line_bg(5, (uint)ID.BG2, (uint)ColorDepth.D4, 1, 5);
            render_line_oam(2, 4, 6, 8);
        }

        public void render_line_mode6()
        {
            render_line_bg(6, (uint)ID.BG1, (uint)ColorDepth.D16, 2, 5);
            render_line_oam(1, 3, 4, 6);
        }

        public void render_line_mode7()
        {
            if (regs.mode7_extbg == false)
            {
                render_line_mode7((uint)ID.BG1, 2, 2);
                render_line_oam(1, 3, 4, 5);
            }
            else
            {
                render_line_mode7((uint)ID.BG1, 3, 3);
                render_line_mode7((uint)ID.BG2, 1, 5);
                render_line_oam(2, 4, 6, 7);
            }
        }

        //cache.cpp
        public enum ColorDepth { D4 = 0, D16 = 1, D256 = 2 };
        public enum Tile { T2BIT = 0, T4BIT = 1, T8BIT = 2 };

        public Pixel[] pixel_cache = new Pixel[256];

        public byte[][] bg_tiledata = new byte[3][];
        public byte[][] bg_tiledata_state = new byte[3][];  //0 = valid, 1 = dirty

        private void render_bg_tile_line_2bpp(byte mask, ref ArraySegment<byte> dest, ref byte col, byte d0, byte d1)
        {
            col = (byte)(Bit.ToBit((uint)(d0 & mask)) << 0);
            col += (byte)(Bit.ToBit((uint)(d1 & mask)) << 1);
            dest.Array[dest.Offset] = col;
            dest = new ArraySegment<byte>(dest.Array, dest.Offset + 1, dest.Count - 1);
        }

        private void render_bg_tile_line_4bpp(byte mask, ref ArraySegment<byte> dest, ref byte col, byte d0, byte d1, byte d2, byte d3)
        {
            col = (byte)(Bit.ToBit((uint)(d0 & mask)) << 0);
            col += (byte)(Bit.ToBit((uint)(d1 & mask)) << 1);
            col += (byte)(Bit.ToBit((uint)(d2 & mask)) << 2);
            col += (byte)(Bit.ToBit((uint)(d3 & mask)) << 3);
            dest.Array[dest.Offset] = col;
            dest = new ArraySegment<byte>(dest.Array, dest.Offset + 1, dest.Count - 1);
        }

        private void render_bg_tile_line_8bpp(byte mask, ref ArraySegment<byte> dest, ref byte col, byte d0, byte d1, byte d2, byte d3, byte d4, byte d5, byte d6, byte d7)
        {
            col = (byte)(Bit.ToBit((uint)(d0 & mask)) << 0);
            col += (byte)(Bit.ToBit((uint)(d1 & mask)) << 1);
            col += (byte)(Bit.ToBit((uint)(d2 & mask)) << 2);
            col += (byte)(Bit.ToBit((uint)(d3 & mask)) << 3);
            col += (byte)(Bit.ToBit((uint)(d4 & mask)) << 4);
            col += (byte)(Bit.ToBit((uint)(d5 & mask)) << 5);
            col += (byte)(Bit.ToBit((uint)(d6 & mask)) << 6);
            col += (byte)(Bit.ToBit((uint)(d7 & mask)) << 7);
            dest.Array[dest.Offset] = col;
            dest = new ArraySegment<byte>(dest.Array, dest.Offset + 1, dest.Count - 1);
        }

        public void render_bg_tile(uint color_depth, ushort tile_num)
        {
            byte col = 0, d0, d1, d2, d3, d4, d5, d6, d7;

            if (color_depth == (uint)ColorDepth.D4)
            {
                ArraySegment<byte> dest = new ArraySegment<byte>(bg_tiledata[(int)Tile.T2BIT], tile_num * 64, bg_tiledata[(int)Tile.T2BIT].Length - tile_num * 64);
                uint pos = (uint)(tile_num * 16);
                uint y = 8;
                while (Convert.ToBoolean(y--))
                {
                    d0 = StaticRAM.vram[pos];
                    d1 = StaticRAM.vram[pos + 1];
                    render_bg_tile_line_2bpp(0x80, ref dest, ref col, d0, d1);
                    render_bg_tile_line_2bpp(0x40, ref dest, ref col, d0, d1);
                    render_bg_tile_line_2bpp(0x20, ref dest, ref col, d0, d1);
                    render_bg_tile_line_2bpp(0x10, ref dest, ref col, d0, d1);
                    render_bg_tile_line_2bpp(0x08, ref dest, ref col, d0, d1);
                    render_bg_tile_line_2bpp(0x04, ref dest, ref col, d0, d1);
                    render_bg_tile_line_2bpp(0x02, ref dest, ref col, d0, d1);
                    render_bg_tile_line_2bpp(0x01, ref dest, ref col, d0, d1);
                    pos += 2;
                }
                bg_tiledata_state[(int)Tile.T2BIT][tile_num] = 0;
            }

            if (color_depth == (uint)ColorDepth.D16)
            {
                ArraySegment<byte> dest = new ArraySegment<byte>(bg_tiledata[(int)Tile.T4BIT], tile_num * 64, bg_tiledata[(int)Tile.T4BIT].Length - tile_num * 64);
                uint pos = (uint)(tile_num * 32);
                uint y = 8;
                while (Convert.ToBoolean(y--))
                {
                    d0 = StaticRAM.vram[pos];
                    d1 = StaticRAM.vram[pos + 1];
                    d2 = StaticRAM.vram[pos + 16];
                    d3 = StaticRAM.vram[pos + 17];
                    render_bg_tile_line_4bpp(0x80, ref dest, ref col, d0, d1, d2, d3);
                    render_bg_tile_line_4bpp(0x40, ref dest, ref col, d0, d1, d2, d3);
                    render_bg_tile_line_4bpp(0x20, ref dest, ref col, d0, d1, d2, d3);
                    render_bg_tile_line_4bpp(0x10, ref dest, ref col, d0, d1, d2, d3);
                    render_bg_tile_line_4bpp(0x08, ref dest, ref col, d0, d1, d2, d3);
                    render_bg_tile_line_4bpp(0x04, ref dest, ref col, d0, d1, d2, d3);
                    render_bg_tile_line_4bpp(0x02, ref dest, ref col, d0, d1, d2, d3);
                    render_bg_tile_line_4bpp(0x01, ref dest, ref col, d0, d1, d2, d3);
                    pos += 2;
                }
                bg_tiledata_state[(int)Tile.T4BIT][tile_num] = 0;
            }

            if (color_depth == (uint)ColorDepth.D256)
            {
                ArraySegment<byte> dest = new ArraySegment<byte>(bg_tiledata[(int)Tile.T8BIT], tile_num * 64, bg_tiledata[(int)Tile.T8BIT].Length - tile_num * 64);
                uint pos = (uint)(tile_num * 64);
                uint y = 8;
                while (Convert.ToBoolean(y--))
                {
                    d0 = StaticRAM.vram[pos];
                    d1 = StaticRAM.vram[pos + 1];
                    d2 = StaticRAM.vram[pos + 16];
                    d3 = StaticRAM.vram[pos + 17];
                    d4 = StaticRAM.vram[pos + 32];
                    d5 = StaticRAM.vram[pos + 33];
                    d6 = StaticRAM.vram[pos + 48];
                    d7 = StaticRAM.vram[pos + 49];
                    render_bg_tile_line_8bpp(0x80, ref dest, ref col, d0, d1, d2, d3, d4, d5, d6, d7);
                    render_bg_tile_line_8bpp(0x40, ref dest, ref col, d0, d1, d2, d3, d4, d5, d6, d7);
                    render_bg_tile_line_8bpp(0x20, ref dest, ref col, d0, d1, d2, d3, d4, d5, d6, d7);
                    render_bg_tile_line_8bpp(0x10, ref dest, ref col, d0, d1, d2, d3, d4, d5, d6, d7);
                    render_bg_tile_line_8bpp(0x08, ref dest, ref col, d0, d1, d2, d3, d4, d5, d6, d7);
                    render_bg_tile_line_8bpp(0x04, ref dest, ref col, d0, d1, d2, d3, d4, d5, d6, d7);
                    render_bg_tile_line_8bpp(0x02, ref dest, ref col, d0, d1, d2, d3, d4, d5, d6, d7);
                    render_bg_tile_line_8bpp(0x01, ref dest, ref col, d0, d1, d2, d3, d4, d5, d6, d7);
                    pos += 2;
                }
                bg_tiledata_state[(int)Tile.T8BIT][tile_num] = 0;
            }
        }

        public void flush_pixel_cache()
        {
            ushort main = get_palette(0);
            ushort sub = (regs.pseudo_hires || regs.bg_mode == 5 || regs.bg_mode == 6)
                        ? main
                        : regs.color_rgb;

            uint i = 255;
            do
            {
                pixel_cache[i].src_main = main;
                pixel_cache[i].src_sub = sub;
                pixel_cache[i].bg_main = (int)ID.BACK;
                pixel_cache[i].bg_sub = (int)ID.BACK;
                pixel_cache[i].ce_main = Convert.ToByte(false);
                pixel_cache[i].ce_sub = Convert.ToByte(false);
                pixel_cache[i].pri_main = 0;
                pixel_cache[i].pri_sub = 0;
            }
            while (Convert.ToBoolean(i--));
        }

        public void alloc_tiledata_cache()
        {
            bg_tiledata[(int)Tile.T2BIT] = new byte[262144];
            bg_tiledata[(int)Tile.T4BIT] = new byte[131072];
            bg_tiledata[(int)Tile.T8BIT] = new byte[65536];
            bg_tiledata_state[(int)Tile.T2BIT] = new byte[4096];
            bg_tiledata_state[(int)Tile.T4BIT] = new byte[2048];
            bg_tiledata_state[(int)Tile.T8BIT] = new byte[1024];
        }

        public void flush_tiledata_cache()
        {
            for (uint i = 0; i < 4096; i++)
            {
                bg_tiledata_state[(int)Tile.T2BIT][i] = 1;
            }
            for (uint i = 0; i < 2048; i++)
            {
                bg_tiledata_state[(int)Tile.T4BIT][i] = 1;
            }
            for (uint i = 0; i < 1024; i++)
            {
                bg_tiledata_state[(int)Tile.T8BIT][i] = 1;
            }
        }

        //windows.cpp
        public Window[] window = new Window[6];

        public void build_window_table(byte bg, bool screen)
        {
            bool set = Convert.ToBoolean(1), clr = Convert.ToBoolean(0);
            byte[] table = (screen == Convert.ToBoolean(0) ? window[bg].main : window[bg].sub);

            if (bg != (byte)ID.COL)
            {
                if (screen == Convert.ToBoolean(0) && regs.window_enabled[bg] == false)
                {
                    Array.Clear(table, 0, 256);
                    return;
                }
                if (screen == Convert.ToBoolean(1) && regs.sub_window_enabled[bg] == false)
                {
                    Array.Clear(table, 0, 256);
                    return;
                }
            }
            else
            {
                switch (screen == Convert.ToBoolean(0) ? regs.color_mask : regs.colorsub_mask)
                {
                    case 0:
                        Array.Copy(Enumerable.Repeat<byte>(1, 256).ToArray(), table, table.Length);
                        return;  //always
                    case 3:
                        Array.Clear(table, 0, 256);
                        return;  //never
                    case 1:
                        set = Convert.ToBoolean(1); clr = Convert.ToBoolean(0);
                        break;        //inside window only
                    case 2:
                        set = Convert.ToBoolean(0); clr = Convert.ToBoolean(1);
                        break;        //outside window only
                }
            }

            ushort window1_left = regs.window1_left;
            ushort window1_right = regs.window1_right;
            ushort window2_left = regs.window2_left;
            ushort window2_right = regs.window2_right;

            if (regs.window1_enabled[bg] == false && regs.window2_enabled[bg] == false)
            {
                Array.Copy(Enumerable.Repeat<byte>(Convert.ToByte(clr), 256).ToArray(), table, table.Length);
                return;
            }

            if (regs.window1_enabled[bg] == true && regs.window2_enabled[bg] == false)
            {
                if (regs.window1_invert[bg] == true)
                {
                    set ^= clr ^= set ^= clr;
                }
                for (uint x = 0; x < 256; x++)
                {
                    table[x] = Convert.ToByte((x >= window1_left && x <= window1_right) ? set : clr);
                }
                return;
            }

            if (regs.window1_enabled[bg] == false && regs.window2_enabled[bg] == true)
            {
                if (regs.window2_invert[bg] == true)
                {
                    set ^= clr ^= set ^= clr;
                }
                for (uint x = 0; x < 256; x++)
                {
                    table[x] = Convert.ToByte((x >= window2_left && x <= window2_right) ? set : clr);
                }
                return;
            }

            for (uint x = 0; x < 256; x++)
            {
                bool w1_mask = (x >= window1_left && x <= window1_right) ^ regs.window1_invert[bg];
                bool w2_mask = (x >= window2_left && x <= window2_right) ^ regs.window2_invert[bg];

                switch (regs.window_mask[bg])
                {
                    case 0:
                        table[x] = Convert.ToByte((w1_mask | w2_mask) == Convert.ToBoolean(1) ? set : clr);
                        break;  //or
                    case 1:
                        table[x] = Convert.ToByte((w1_mask & w2_mask) == Convert.ToBoolean(1) ? set : clr);
                        break;  //and
                    case 2:
                        table[x] = Convert.ToByte((w1_mask ^ w2_mask) == Convert.ToBoolean(1) ? set : clr);
                        break;  //xor
                    case 3:
                        table[x] = Convert.ToByte((w1_mask ^ w2_mask) == Convert.ToBoolean(0) ? set : clr);
                        break;  //xnor
                }
            }
        }

        public void build_window_tables(byte bg)
        {
            build_window_table(bg, Convert.ToBoolean(0));
            build_window_table(bg, Convert.ToBoolean(1));
        }

        //bg.cpp
        public BackgroundInfo[] bg_info = new BackgroundInfo[4];

        public void update_bg_info()
        {
            uint hires = Convert.ToUInt32(regs.bg_mode == 5 || regs.bg_mode == 6);
            uint width = (!Convert.ToBoolean(hires) ? 256U : 512U);

            for (uint bg = 0; bg < 4; bg++)
            {
                bg_info[bg].th = (ushort)(regs.bg_tilesize[bg] ? 4 : 3);
                bg_info[bg].tw = (ushort)(Convert.ToBoolean(hires) ? 4 : bg_info[bg].th);

                bg_info[bg].mx = (ushort)(bg_info[bg].th == 4 ? (width << 1) : width);
                bg_info[bg].my = bg_info[bg].mx;
                if (Convert.ToBoolean(regs.bg_scsize[bg] & 0x01))
                {
                    bg_info[bg].mx <<= 1;
                }
                if (Convert.ToBoolean(regs.bg_scsize[bg] & 0x02))
                {
                    bg_info[bg].my <<= 1;
                }
                bg_info[bg].mx--;
                bg_info[bg].my--;

                bg_info[bg].scy = (ushort)(Convert.ToBoolean(regs.bg_scsize[bg] & 0x02) ? (32 << 5) : 0);
                bg_info[bg].scx = (ushort)(Convert.ToBoolean(regs.bg_scsize[bg] & 0x01) ? (32 << 5) : 0);
                if (regs.bg_scsize[bg] == 3)
                {
                    bg_info[bg].scy <<= 1;
                }
            }
        }

        public ushort bg_get_tile(uint bg, ushort x, ushort y)
        {
            x = (ushort)((x & bg_info[bg].mx) >> bg_info[bg].tw);
            y = (ushort)((y & bg_info[bg].my) >> bg_info[bg].th);

            ushort pos = (ushort)(((y & 0x1f) << 5) + (x & 0x1f));
            if (Convert.ToBoolean(y & 0x20))
            {
                pos += bg_info[bg].scy;
            }
            if (Convert.ToBoolean(x & 0x20))
            {
                pos += bg_info[bg].scx;
            }

            ushort addr = (ushort)(regs.bg_scaddr[bg] + (pos << 1));
            return (ushort)(StaticRAM.vram[addr] + (StaticRAM.vram[(uint)(addr + 1)] << 8));
        }

        private void setpixel_main(uint x, ushort tile_pri, uint bg, ushort col)
        {
            if (pixel_cache[x].pri_main < tile_pri)
            {
                pixel_cache[x].pri_main = (byte)tile_pri;
                pixel_cache[x].bg_main = (byte)bg;
                pixel_cache[x].src_main = col;
                pixel_cache[x].ce_main = Convert.ToByte(false);
            }
        }

        private void setpixel_sub(uint x, ushort tile_pri, uint bg, ushort col)
        {
            if (pixel_cache[x].pri_sub < tile_pri)
            {
                pixel_cache[x].pri_sub = (byte)tile_pri;
                pixel_cache[x].bg_sub = (byte)bg;
                pixel_cache[x].src_sub = col;
                pixel_cache[x].ce_sub = Convert.ToByte(false);
            }
        }

        public void render_line_bg(uint mode, uint bg, uint color_depth, byte pri0_pos, byte pri1_pos)
        {
            if (regs.bg_enabled[bg] == false && regs.bgsub_enabled[bg] == false)
            {
                return;
            }

            bool bg_enabled = regs.bg_enabled[bg];
            bool bgsub_enabled = regs.bgsub_enabled[bg];

            ushort opt_valid_bit = (ushort)((bg == (uint)ID.BG1) ? 0x2000 : (bg == (uint)ID.BG2) ? 0x4000 : 0x0000);
            byte bgpal_index = (byte)((mode == 0 ? (bg << 5) : 0));

            byte pal_size = (byte)(2 << (int)color_depth);       //<<2 (*4), <<4 (*16), <<8 (*256)
            ushort tile_mask = (ushort)(0x0fff >> (int)color_depth);  //0x0fff, 0x07ff, 0x03ff
            //4 + color_depth = >>(4-6) -- / {16, 32, 64 } bytes/tile
            //index is a tile number count to add to base tile number
            uint tiledata_index = (uint)(regs.bg_tdaddr[bg] >> (int)(4 + color_depth));

            byte[] bg_td = bg_tiledata[color_depth];
            byte[] bg_td_state = bg_tiledata_state[color_depth];

            byte tile_width = (byte)bg_info[bg].tw;
            byte tile_height = (byte)bg_info[bg].th;
            ushort mask_x = bg_info[bg].mx;  //screen width  mask
            ushort mask_y = bg_info[bg].my;  //screen height mask

            ushort y = regs.bg_y[bg];
            ushort hscroll = regs.bg_hofs[bg];
            ushort vscroll = regs.bg_vofs[bg];

            uint hires = Convert.ToUInt32(mode == 5 || mode == 6);
            uint width = (uint)(!Convert.ToBoolean(hires) ? 256 : 512);

            if (Convert.ToBoolean(hires))
            {
                hscroll <<= 1;
                if (regs.interlace)
                {
                    y = (ushort)((y << 1) + Convert.ToUInt32(PPUCounter.field()));
                }
            }

            ushort hval = 0, vval = 0;
            ushort tile_pri = 0, tile_num;
            byte pal_index = 0, pal_num = 0;
            ushort hoffset, voffset, opt_x, col;
            bool mirror_x = false, mirror_y;

            ArraySegment<byte> tile_ptr = new ArraySegment<byte>();
            ushort[] mtable = mosaic_table[regs.mosaic_enabled[bg] ? regs.mosaic_size : 0];
            bool is_opt_mode = (mode == 2 || mode == 4 || mode == 6);
            bool is_direct_color_mode = (regs.direct_color == true && bg == (uint)ID.BG1 && (mode == 3 || mode == 4));

            build_window_tables((byte)bg);
            byte[] wt_main = window[bg].main;
            byte[] wt_sub = window[bg].sub;

            ushort prev_x = 0xffff, prev_y = 0xffff, prev_optx = 0xffff;
            for (ushort x = 0; x < width; x++)
            {
                hoffset = (ushort)(mtable[x] + hscroll);
                voffset = (ushort)(y + vscroll);

                if (is_opt_mode)
                {
                    opt_x = (ushort)(x + (hscroll & 7));

                    //tile 0 is unaffected by OPT mode...
                    if (opt_x >= 8)
                    {
                        //cache tile data in hval, vval if possible
                        if ((opt_x >> 3) != (prev_optx >> 3))
                        {
                            prev_optx = opt_x;

                            hval = bg_get_tile((uint)ID.BG3, (ushort)((opt_x - 8) + (regs.bg_hofs[(int)ID.BG3] & ~7)), regs.bg_vofs[(int)ID.BG3]);
                            if (mode != 4)
                            {
                                vval = bg_get_tile((uint)ID.BG3, (ushort)((opt_x - 8) + (regs.bg_hofs[(int)ID.BG3] & ~7)), (ushort)(regs.bg_vofs[(int)ID.BG3] + 8));
                            }
                        }

                        if (mode == 4)
                        {
                            if (Convert.ToBoolean(hval & opt_valid_bit))
                            {
                                if (!Convert.ToBoolean(hval & 0x8000))
                                {
                                    hoffset = (ushort)(opt_x + (hval & ~7));
                                }
                                else
                                {
                                    voffset = (ushort)(y + hval);
                                }
                            }
                        }
                        else
                        {
                            if (Convert.ToBoolean(hval & opt_valid_bit))
                            {
                                hoffset = (ushort)(opt_x + (hval & ~7));
                            }
                            if (Convert.ToBoolean(vval & opt_valid_bit))
                            {
                                voffset = (ushort)(y + vval);
                            }
                        }
                    }
                }

                hoffset &= mask_x;
                voffset &= mask_y;

                if ((hoffset >> 3) != prev_x || (voffset >> 3) != prev_y)
                {
                    prev_x = (ushort)(hoffset >> 3);
                    prev_y = (ushort)(voffset >> 3);

                    tile_num = bg_get_tile(bg, hoffset, voffset);  //format = vhopppcc cccccccc
                    mirror_y = Convert.ToBoolean(tile_num & 0x8000);
                    mirror_x = Convert.ToBoolean(tile_num & 0x4000);
                    tile_pri = Convert.ToBoolean(tile_num & 0x2000) ? pri1_pos : pri0_pos;
                    pal_num = (byte)((tile_num >> 10) & 7);
                    pal_index = (byte)(bgpal_index + (pal_num << pal_size));

                    if (tile_width == 4)
                    {  //16x16 horizontal tile mirroring
                        if (Convert.ToBoolean(hoffset & 8) != mirror_x)
                        {
                            tile_num++;
                        }
                    }

                    if (tile_height == 4)
                    {  //16x16 vertical tile mirroring
                        if (Convert.ToBoolean(voffset & 8) != mirror_y)
                        {
                            tile_num += 16;
                        }
                    }

                    tile_num &= 0x03ff;
                    tile_num += (ushort)tiledata_index;
                    tile_num &= tile_mask;

                    if (bg_td_state[tile_num] == 1)
                    {
                        render_bg_tile(color_depth, tile_num);
                    }

                    if (mirror_y)
                    {
                        voffset ^= 7;  //invert y tile pos
                    }
                    tile_ptr = new ArraySegment<byte>(bg_td, (tile_num * 64) + ((voffset & 7) * 8), bg_td.Length - ((tile_num * 64) + ((voffset & 7) * 8)));
                }

                if (mirror_x)
                {
                    hoffset ^= 7;  //invert x tile pos
                }
                col = tile_ptr.Array[tile_ptr.Offset + (hoffset & 7)];
                if (Convert.ToBoolean(col))
                {
                    if (is_direct_color_mode)
                    {
                        col = get_direct_color(pal_num, (byte)col);
                    }
                    else
                    {
                        col = get_palette((byte)(col + pal_index));
                    }

                    if (!Convert.ToBoolean(hires))
                    {
                        if (bg_enabled == true && !Convert.ToBoolean(wt_main[x]))
                        {
                            setpixel_main(x, tile_pri, bg, col);
                        }
                        if (bgsub_enabled == true && !Convert.ToBoolean(wt_sub[x]))
                        {
                            setpixel_sub(x, tile_pri, bg, col);
                        }
                    }
                    else
                    {
                        int hx = x >> 1;
                        if (Convert.ToBoolean(x & 1))
                        {
                            if (bg_enabled == true && !Convert.ToBoolean(wt_main[hx]))
                            {
                                setpixel_main((uint)hx, tile_pri, bg, col);
                            }
                        }
                        else
                        {
                            if (bgsub_enabled == true && !Convert.ToBoolean(wt_sub[hx]))
                            {
                                setpixel_sub((uint)hx, tile_pri, bg, col);
                            }
                        }
                    }
                }
            }
        }

        //oam.cpp
        public SpriteItem[] sprite_list = new SpriteItem[128];
        public bool sprite_list_valid;
        public uint active_sprite;

        public byte[] oam_itemlist = new byte[32];
        public OamTileItem[] oam_tilelist = new OamTileItem[34];

        public const int OAM_PRI_NONE = 4;
        public byte[] oam_line_pal = new byte[256], oam_line_pri = new byte[256];

        public void update_sprite_list(uint addr, byte data)
        {
            if (addr < 0x0200)
            {
                uint i = addr >> 2;
                switch (addr & 3)
                {
                    case 0:
                        sprite_list[i].x = (ushort)((sprite_list[i].x & 0x0100) | data);
                        break;
                    case 1:
                        sprite_list[i].y = (ushort)((data + 1) & 0xff);
                        break;
                    case 2:
                        sprite_list[i].character = data;
                        break;
                    case 3:
                        sprite_list[i].vflip = Convert.ToBoolean(data & 0x80);
                        sprite_list[i].hflip = Convert.ToBoolean(data & 0x40);
                        sprite_list[i].priority = (byte)((data >> 4) & 3);
                        sprite_list[i].palette = (byte)((data >> 1) & 7);
                        sprite_list[i].use_nameselect = Convert.ToBoolean(data & 0x01);
                        break;
                }
            }
            else
            {
                uint i = (addr & 0x1f) << 2;
                sprite_list[i + 0].x = (ushort)(((data & 0x01) << 8) | (sprite_list[i + 0].x & 0xff));
                sprite_list[i + 0].size = Convert.ToBoolean(data & 0x02);
                sprite_list[i + 1].x = (ushort)(((data & 0x04) << 6) | (sprite_list[i + 1].x & 0xff));
                sprite_list[i + 1].size = Convert.ToBoolean(data & 0x08);
                sprite_list[i + 2].x = (ushort)(((data & 0x10) << 4) | (sprite_list[i + 2].x & 0xff));
                sprite_list[i + 2].size = Convert.ToBoolean(data & 0x20);
                sprite_list[i + 3].x = (ushort)(((data & 0x40) << 2) | (sprite_list[i + 3].x & 0xff));
                sprite_list[i + 3].size = Convert.ToBoolean(data & 0x80);
            }
        }

        public void build_sprite_list()
        {
            if (sprite_list_valid == true)
            {
                return;
            }
            sprite_list_valid = true;

            for (uint i = 0; i < 128; i++)
            {
                bool size = sprite_list[i].size;

                switch (cache.oam_basesize)
                {
                    case 0: sprite_list[i].width = (!size) ? (byte)8 : (byte)16;
                        sprite_list[i].height = (!size) ? (byte)8 : (byte)16;
                        break;
                    case 1: sprite_list[i].width = (!size) ? (byte)8 : (byte)32;
                        sprite_list[i].height = (!size) ? (byte)8 : (byte)32;
                        break;
                    case 2: sprite_list[i].width = (!size) ? (byte)8 : (byte)64;
                        sprite_list[i].height = (!size) ? (byte)8 : (byte)64;
                        break;
                    case 3: sprite_list[i].width = (!size) ? (byte)16 : (byte)32;
                        sprite_list[i].height = (!size) ? (byte)16 : (byte)32;
                        break;
                    case 4: sprite_list[i].width = (!size) ? (byte)16 : (byte)64;
                        sprite_list[i].height = (!size) ? (byte)16 : (byte)64;
                        break;
                    case 5: sprite_list[i].width = (!size) ? (byte)32 : (byte)64;
                        sprite_list[i].height = (!size) ? (byte)32 : (byte)64;
                        break;
                    case 6: sprite_list[i].width = (!size) ? (byte)16 : (byte)32;
                        sprite_list[i].height = (!size) ? (byte)32 : (byte)64;
                        if (regs.oam_interlace && !size)
                        {
                            sprite_list[i].height = 16;
                        }
                        //32x64 height is not affected by oam_interlace setting
                        break;
                    case 7: sprite_list[i].width = (!size) ? (byte)16 : (byte)32;
                        sprite_list[i].height = (!size) ? (byte)32 : (byte)32;
                        if (regs.oam_interlace && !size)
                        {
                            sprite_list[i].height = 16;
                        }
                        break;
                }
            }
        }

        public bool is_sprite_on_scanline()
        {   //if sprite is entirely offscreen and doesn't wrap around to the left side of the screen,
            //then it is not counted. this *should* be 256, and not 255, even though dot 256 is offscreen.
            SpriteItem spr = sprite_list[active_sprite];
            if (spr.x > 256 && (spr.x + spr.width - 1) < 512)
            {
                return false;
            }

            int spr_height = (regs.oam_interlace == false) ? (spr.height) : (spr.height >> 1);
            if (line >= spr.y && line < (spr.y + spr_height))
            {
                return true;
            }
            if ((spr.y + spr_height) >= 256 && line < ((spr.y + spr_height) & 255))
            {
                return true;
            }
            return false;
        }

        public void load_oam_tiles()
        {
            SpriteItem spr = sprite_list[active_sprite];
            ushort tile_width = (ushort)(spr.width >> 3);
            int x = spr.x;
            int y = (int)((line - spr.y) & 0xff);
            if (regs.oam_interlace == true)
            {
                y <<= 1;
            }

            if (spr.vflip == true)
            {
                if (spr.width == spr.height)
                {
                    y = (spr.height - 1) - y;
                }
                else
                {
                    y = (y < spr.width) ? ((spr.width - 1) - y) : (spr.width + ((spr.width - 1) - (y - spr.width)));
                }
            }

            if (regs.oam_interlace == true)
            {
                y = (spr.vflip == false) ? (y + Convert.ToInt32(PPUCounter.field())) : (y - Convert.ToInt32(PPUCounter.field()));
            }

            x &= 511;
            y &= 255;

            ushort tdaddr = cache.oam_tdaddr;
            ushort chrx = (ushort)((spr.character) & 15);
            ushort chry = (ushort)((spr.character >> 4) & 15);
            if (spr.use_nameselect == true)
            {
                tdaddr += (ushort)((256 * 32) + (cache.oam_nameselect << 13));
            }
            chry += (ushort)(y >> 3);
            chry &= 15;
            chry <<= 4;

            for (uint tx = 0; tx < tile_width; tx++)
            {
                uint sx = (uint)((x + (tx << 3)) & 511);
                //ignore sprites that are offscreen, x==256 is a special case that loads all tiles in OBJ
                if (x != 256 && sx >= 256 && (sx + 7) < 512)
                {
                    continue;
                }

                if (regs.oam_tilecount++ >= 34)
                {
                    break;
                }
                uint n = (uint)(regs.oam_tilecount - 1);
                oam_tilelist[n].x = (ushort)sx;
                oam_tilelist[n].y = (ushort)y;
                oam_tilelist[n].pri = spr.priority;
                oam_tilelist[n].pal = (ushort)(128 + (spr.palette << 4));
                oam_tilelist[n].hflip = spr.hflip;

                uint mx = (uint)((spr.hflip == false) ? tx : ((tile_width - 1) - tx));
                uint pos = tdaddr + ((chry + ((chrx + mx) & 15)) << 5);
                oam_tilelist[n].tile = (ushort)((pos >> 5) & 0x07ff);
            }
        }

        public void render_oam_tile(int tile_num)
        {
            OamTileItem t = oam_tilelist[tile_num];
            byte[] oam_td = bg_tiledata[(int)ColorDepth.D16];
            byte[] oam_td_state = bg_tiledata_state[(int)ColorDepth.D16];

            if (oam_td_state[t.tile] == 1)
            {
                render_bg_tile((uint)ColorDepth.D16, t.tile);
            }

            uint sx = t.x;
            ArraySegment<byte> tile_ptr = new ArraySegment<byte>(oam_td, (t.tile << 6) + ((t.y & 7) << 3), oam_td.Length - ((t.tile << 6) + ((t.y & 7) << 3)));
            for (uint x = 0; x < 8; x++)
            {
                sx &= 511;
                if (sx < 256)
                {
                    uint col = tile_ptr.Array[tile_ptr.Offset + ((t.hflip == false) ? x : (7 - x))];
                    if (Convert.ToBoolean(col))
                    {
                        col += t.pal;
                        oam_line_pal[sx] = (byte)col;
                        oam_line_pri[sx] = (byte)t.pri;
                    }
                }
                sx++;
            }
        }

        public void render_line_oam_rto()
        {
            build_sprite_list();

            regs.oam_itemcount = 0;
            regs.oam_tilecount = 0;
            oam_line_pri = Enumerable.Repeat<byte>(OAM_PRI_NONE, 256).ToArray();
            oam_itemlist = Enumerable.Repeat<byte>(0xff, 32).ToArray();
            for (int s = 0; s < 34; s++)
            {
                oam_tilelist[s].tile = 0xffff;
            }

            for (int s = 0; s < 128; s++)
            {
                active_sprite = (uint)((s + regs.oam_firstsprite) & 127);
                if (is_sprite_on_scanline() == false)
                {
                    continue;
                }
                if (regs.oam_itemcount++ >= 32)
                {
                    break;
                }
                oam_itemlist[regs.oam_itemcount - 1] = (byte)((s + regs.oam_firstsprite) & 127);
            }

            //TODO: remove this hack
            if (regs.oam_itemcount > 0 && (regs.oam_itemcount > oam_itemlist.Length))
            {
                regs.ioamaddr = (ushort)(0x0200 + (0 >> 2));
            }
            else if (regs.oam_itemcount > 0 && oam_itemlist[regs.oam_itemcount - 1] != 0xff)
            {
                regs.ioamaddr = (ushort)(0x0200 + (oam_itemlist[regs.oam_itemcount - 1] >> 2));
            }

            for (int s = 31; s >= 0; s--)
            {
                if (oam_itemlist[s] == 0xff)
                {
                    continue;
                }
                active_sprite = oam_itemlist[s];
                load_oam_tiles();
            }

            regs.time_over |= (regs.oam_tilecount > 34);
            regs.range_over |= (regs.oam_itemcount > 32);
        }

        private void setpixel_main(int x, uint pri)
        {
            if (pixel_cache[x].pri_main < pri)
            {
                pixel_cache[x].pri_main = (byte)pri;
                pixel_cache[x].bg_main = (byte)ID.OAM;
                pixel_cache[x].src_main = get_palette(oam_line_pal[x]);
                pixel_cache[x].ce_main = Convert.ToByte(oam_line_pal[x] < 192);
            }
        }
        private void setpixel_sub(int x, uint pri)
        {
            if (pixel_cache[x].pri_sub < pri)
            {
                pixel_cache[x].pri_sub = (byte)pri;
                pixel_cache[x].bg_sub = (byte)ID.OAM;
                pixel_cache[x].src_sub = get_palette(oam_line_pal[x]);
                pixel_cache[x].ce_sub = Convert.ToByte(oam_line_pal[x] < 192);
            }
        }

        public void render_line_oam(byte pri0_pos, byte pri1_pos, byte pri2_pos, byte pri3_pos)
        {
            if (regs.bg_enabled[(int)ID.OAM] == false && regs.bgsub_enabled[(int)ID.OAM] == false)
            {
                return;
            }

            for (uint s = 0; s < 34; s++)
            {
                if (oam_tilelist[s].tile == 0xffff)
                {
                    continue;
                }
                render_oam_tile((int)s);
            }

            bool bg_enabled = regs.bg_enabled[(int)ID.OAM];
            bool bgsub_enabled = regs.bgsub_enabled[(int)ID.OAM];

            build_window_tables((byte)ID.OAM);
            byte[] wt_main = window[(int)ID.OAM].main;
            byte[] wt_sub = window[(int)ID.OAM].sub;

            uint[] pri_tbl = { pri0_pos, pri1_pos, pri2_pos, pri3_pos };
            for (int x = 0; x < 256; x++)
            {
                if (oam_line_pri[x] == OAM_PRI_NONE)
                {
                    continue;
                }

                uint pri = pri_tbl[oam_line_pri[x]];
                if (bg_enabled == true && !Convert.ToBoolean(wt_main[x]))
                {
                    setpixel_main(x, pri);
                }
                if (bgsub_enabled == true && !Convert.ToBoolean(wt_sub[x]))
                {
                    setpixel_sub(x, pri);
                }
            }
        }

        private int CLIP(int x)
        {
            return Convert.ToBoolean((x) & 0x2000) ? ((x) | ~0x03ff) : ((x) & 0x03ff);
        }

        public void render_line_mode7(uint bg, byte pri0_pos, byte pri1_pos)
        {
            if (regs.bg_enabled[bg] == false && regs.bgsub_enabled[bg] == false)
            {
                return;
            }

            int px, py;
            int tx, ty, tile, palette = 0;

            int a = Bit.sclip(16, cache.m7a);
            int b = Bit.sclip(16, cache.m7b);
            int c = Bit.sclip(16, cache.m7c);
            int d = Bit.sclip(16, cache.m7d);

            int cx = Bit.sclip(13, cache.m7x);
            int cy = Bit.sclip(13, cache.m7y);
            int hofs = Bit.sclip(13, cache.m7_hofs);
            int vofs = Bit.sclip(13, cache.m7_vofs);

            int _pri, _x;
            bool _bg_enabled = regs.bg_enabled[bg];
            bool _bgsub_enabled = regs.bgsub_enabled[bg];

            build_window_tables((byte)bg);
            byte[] wt_main = window[bg].main;
            byte[] wt_sub = window[bg].sub;

            int y = (int)(regs.mode7_vflip == false ? line : 255 - line);

            ushort[] mtable_x, mtable_y;
            if (bg == (uint)ID.BG1)
            {
                mtable_x = (ushort[])mosaic_table[(regs.mosaic_enabled[(int)ID.BG1] == true) ? regs.mosaic_size : 0];
                mtable_y = (ushort[])mosaic_table[(regs.mosaic_enabled[(int)ID.BG1] == true) ? regs.mosaic_size : 0];
            }
            else
            {  //bg == BG2
                //Mode7 EXTBG BG2 uses BG1 mosaic enable to control vertical mosaic,
                //and BG2 mosaic enable to control horizontal mosaic...
                mtable_x = (ushort[])mosaic_table[(regs.mosaic_enabled[(int)ID.BG2] == true) ? regs.mosaic_size : 0];
                mtable_y = (ushort[])mosaic_table[(regs.mosaic_enabled[(int)ID.BG1] == true) ? regs.mosaic_size : 0];
            }

            int psx = ((a * CLIP(hofs - cx)) & ~63) + ((b * CLIP(vofs - cy)) & ~63) + ((b * mtable_y[y]) & ~63) + (cx << 8);
            int psy = ((c * CLIP(hofs - cx)) & ~63) + ((d * CLIP(vofs - cy)) & ~63) + ((d * mtable_y[y]) & ~63) + (cy << 8);
            for (int x = 0; x < 256; x++)
            {
                px = psx + (a * mtable_x[x]);
                py = psy + (c * mtable_x[x]);

                //mask floating-point bits (low 8 bits)
                px >>= 8;
                py >>= 8;

                switch (regs.mode7_repeat)
                {
                    case 0:    //screen repetition outside of screen area
                    case 1:
                        {  //same as case 0
                            px &= 1023;
                            py &= 1023;
                            tx = ((px >> 3) & 127);
                            ty = ((py >> 3) & 127);
                            tile = StaticRAM.vram[(uint)((ty * 128 + tx) << 1)];
                            palette = StaticRAM.vram[(uint)((((tile << 6) + ((py & 7) << 3) + (px & 7)) << 1) + 1)];
                        }
                        break;
                    case 2:
                        {  //palette color 0 outside of screen area
                            if (px < 0 || px > 1023 || py < 0 || py > 1023)
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
                        }
                        break;
                    case 3:
                        {  //character 0 repetition outside of screen area
                            if (px < 0 || px > 1023 || py < 0 || py > 1023)
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
                        }
                        break;
                }

                if (bg == (uint)ID.BG1)
                {
                    _pri = pri0_pos;
                }
                else
                {
                    _pri = Convert.ToBoolean(palette >> 7) ? pri1_pos : pri0_pos;
                    palette &= 0x7f;
                }

                if (!Convert.ToBoolean(palette))
                {
                    continue;
                }

                _x = (regs.mode7_hflip == false) ? (x) : (255 - x);

                uint col;
                if (regs.direct_color == true && bg == (uint)ID.BG1)
                {
                    //direct color mode does not apply to bg2, as it is only 128 colors...
                    col = get_direct_color(0, (byte)palette);
                }
                else
                {
                    col = get_palette((byte)palette);
                }

                if (regs.bg_enabled[bg] == true && !Convert.ToBoolean(wt_main[_x]))
                {
                    if (pixel_cache[_x].pri_main < _pri)
                    {
                        pixel_cache[_x].pri_main = (byte)_pri;
                        pixel_cache[_x].bg_main = (byte)bg;
                        pixel_cache[_x].src_main = (ushort)col;
                        pixel_cache[_x].ce_main = Convert.ToByte(false);
                    }
                }
                if (regs.bgsub_enabled[bg] == true && !Convert.ToBoolean(wt_sub[_x]))
                {
                    if (pixel_cache[_x].pri_sub < _pri)
                    {
                        pixel_cache[_x].pri_sub = (byte)_pri;
                        pixel_cache[_x].bg_sub = (byte)bg;
                        pixel_cache[_x].src_sub = (ushort)col;
                        pixel_cache[_x].ce_sub = Convert.ToByte(false);
                    }
                }
            }
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

        public ushort get_palette(byte index)
        {
            uint addr = (uint)(index << 1);
            return (ushort)(StaticRAM.cgram[addr] + (StaticRAM.cgram[addr + 1] << 8));
        }

        public ushort get_direct_color(byte p, byte t)
        {
            return (ushort)(((t & 7) << 2) | ((p & 1) << 1) |
                (((t >> 3) & 7) << 7) | (((p >> 1) & 1) << 6) |
                ((t >> 6) << 13) | ((p >> 2) << 12));
        }

        public ushort get_pixel_normal(uint x)
        {
            Pixel p = pixel_cache[x];
            ushort src_main, src_sub;
            byte bg_sub;
            src_main = p.src_main;

            if (!regs.addsub_mode)
            {
                bg_sub = (byte)ID.BACK;
                src_sub = regs.color_rgb;
            }
            else
            {
                bg_sub = p.bg_sub;
                src_sub = p.src_sub;
            }

            if (!Convert.ToBoolean(window[(int)ID.COL].main[x]))
            {
                if (!Convert.ToBoolean(window[(int)ID.COL].sub[x]))
                {
                    return 0x0000;
                }
                src_main = 0x0000;
            }

            if (!Convert.ToBoolean(p.ce_main) && regs.color_enabled[p.bg_main] && Convert.ToBoolean(window[(int)ID.COL].sub[x]))
            {
                bool halve = false;
                if (regs.color_halve && Convert.ToBoolean(window[(int)ID.COL].main[x]))
                {
                    if (regs.addsub_mode && bg_sub == (byte)ID.BACK) { }
                    else
                    {
                        halve = true;
                    }
                }
                return addsub(src_main, src_sub, halve);
            }

            return src_main;
        }

        public ushort get_pixel_swap(uint x)
        {
            Pixel p = pixel_cache[x];
            ushort src_main, src_sub;
            byte bg_sub;
            src_main = p.src_sub;

            if (!regs.addsub_mode)
            {
                bg_sub = (byte)ID.BACK;
                src_sub = regs.color_rgb;
            }
            else
            {
                bg_sub = p.bg_main;
                src_sub = p.src_main;
            }

            if (!Convert.ToBoolean(window[(int)ID.COL].main[x]))
            {
                if (!Convert.ToBoolean(window[(int)ID.COL].sub[x]))
                {
                    return 0x0000;
                }
                src_main = 0x0000;
            }

            if (!Convert.ToBoolean(p.ce_sub) && regs.color_enabled[p.bg_sub] && Convert.ToBoolean(window[(int)ID.COL].sub[x]))
            {
                bool halve = false;
                if (regs.color_halve && Convert.ToBoolean(window[(int)ID.COL].main[x]))
                {
                    if (regs.addsub_mode && bg_sub == (byte)ID.BACK) { }
                    else
                    {
                        halve = true;
                    }
                }
                return addsub(src_main, src_sub, halve);
            }

            return src_main;
        }

        public void render_line_output()
        {
            ArraySegment<ushort> ptr = new ArraySegment<ushort>(output.Array, (int)(output.Offset + ((line * 1024) + ((interlace() && PPUCounter.field()) ? 512 : 0))), (int)(output.Count - ((line * 1024) + ((interlace() && PPUCounter.field()) ? 512 : 0))));
            ushort[] luma = light_table[regs.display_brightness];
            ushort curr, prev;

            int ptr_offset = 0;
            if (!regs.pseudo_hires && regs.bg_mode != 5 && regs.bg_mode != 6)
            {
                for (uint x = 0; x < 256; x++)
                {
                    curr = luma[get_pixel_normal(x)];
                    ptr.Array[ptr.Offset + ptr_offset++] = curr;
                }
            }
            else
            {
                prev = 0;
                for (uint x = 0; x < 256; x++)
                {
                    curr = luma[get_pixel_swap(x)];
                    ptr.Array[ptr.Offset + ptr_offset++] = (ushort)((prev + curr - ((prev ^ curr) & 0x0421)) >> 1);
                    prev = curr;

                    curr = luma[get_pixel_normal(x)];
                    ptr.Array[ptr.Offset + ptr_offset++] = (ushort)((prev + curr - ((prev ^ curr) & 0x0421)) >> 1);
                    prev = curr;
                }
            }
        }

        public void render_line_clear()
        {
            ArraySegment<ushort> ptr = new ArraySegment<ushort>(output.Array, (int)(output.Offset + ((line * 1024) + ((interlace() && PPUCounter.field()) ? 512 : 0))), (int)(output.Count - ((line * 1024) + ((interlace() && PPUCounter.field()) ? 512 : 0))));
            ushort width = (ushort)((!regs.pseudo_hires && regs.bg_mode != 5 && regs.bg_mode != 6) ? 256 : 512);
            Array.Clear(ptr.Array, ptr.Offset, width * 2);
        }

        public ushort[] surface;
        public ArraySegment<ushort> output;

        public byte ppu1_version;
        public byte ppu2_version;

        public IEnumerable add_clocks(uint clocks)
        {
            foreach (var e in PPUCounter.tick(clocks))
            {
                yield return e;
            };
            step(clocks);
            foreach (var e in synchronize_cpu())
            {
                yield return e;
            };
        }

        public byte region;
        public uint line;

        public enum Region { NTSC = 0, PAL = 1 };
        public enum ID { BG1 = 0, BG2 = 1, BG3 = 2, BG4 = 3, OAM = 4, BACK = 5, COL = 5 };
        public enum SC { S32x32 = 0, S64x32 = 1, S32x64 = 2, S64x64 = 3 };

        public Display display = new Display();

        public Cache cache = new Cache();

        public bool interlace()
        {
            return display.interlace;
        }

        public bool overscan()
        {
            return display.overscan;
        }

        public bool hires()
        {
            return (regs.pseudo_hires || regs.bg_mode == 5 || regs.bg_mode == 6);
        }

        public ushort[][] light_table = new ushort[16][];
        public ushort[][] mosaic_table = new ushort[16][];

        public void render_line()
        {
            if (regs.display_disabled == true)
            {
                render_line_clear();
                return;
            }

            flush_pixel_cache();
            build_window_tables((byte)ID.COL);
            update_bg_info();

            switch (regs.bg_mode)
            {
                case 0:
                    render_line_mode0();
                    break;
                case 1:
                    render_line_mode1();
                    break;
                case 2:
                    render_line_mode2();
                    break;
                case 3:
                    render_line_mode3();
                    break;
                case 4:
                    render_line_mode4();
                    break;
                case 5:
                    render_line_mode5();
                    break;
                case 6:
                    render_line_mode6();
                    break;
                case 7:
                    render_line_mode7();
                    break;
            }

            render_line_output();
        }

        //required functions
        public void scanline()
        {
            line = PPUCounter.vcounter();

            if (line == 0)
            {
                frame();

                //RTO flag reset
                regs.time_over = false;
                regs.range_over = false;
            }

            if (line == 1)
            {
                //mosaic reset
                for (int bg = (int)ID.BG1; bg <= (int)ID.BG4; bg++)
                {
                    regs.bg_y[bg] = 1;
                }
                regs.mosaic_countdown = (ushort)(regs.mosaic_size + 1);
                regs.mosaic_countdown--;
            }
            else
            {
                for (int bg = (int)ID.BG1; bg <= (int)ID.BG4; bg++)
                {
                    if (!regs.mosaic_enabled[bg] || !Convert.ToBoolean(regs.mosaic_countdown))
                    {
                        regs.bg_y[bg] = (ushort)line;
                    }
                }
                if (!Convert.ToBoolean(regs.mosaic_countdown))
                {
                    regs.mosaic_countdown = (ushort)(regs.mosaic_size + 1);
                }
                regs.mosaic_countdown--;
            }
        }

        public void render_scanline()
        {
            if (line >= 1 && line < (!overscan() ? 225 : 240))
            {
                render_line_oam_rto();
                render_line();
            }
        }

        public void frame()
        {
            System.system.frame();

            if (PPUCounter.field() == Convert.ToBoolean(0))
            {
                display.interlace = regs.interlace;
                regs.scanlines = (regs.overscan == false) ? (ushort)224 : (ushort)239;
            }
        }

        public IEnumerable enter()
        {
            while (true)
            {
                if (Scheduler.scheduler.sync == Scheduler.SynchronizeMode.All)
                {
                    yield return Scheduler.ExitReason.SynchronizeEvent;
                }

                //H =    0 (initialize)
                scanline();
                foreach (var e in add_clocks(10))
                {
                    yield return e;
                };

                //H =   10 (cache mode7 registers + OAM address reset)
                cache.m7_hofs = regs.m7_hofs;
                cache.m7_vofs = regs.m7_vofs;
                cache.m7a = regs.m7a;
                cache.m7b = regs.m7b;
                cache.m7c = regs.m7c;
                cache.m7d = regs.m7d;
                cache.m7x = regs.m7x;
                cache.m7y = regs.m7y;
                if (PPUCounter.vcounter() == (!overscan() ? 225 : 240))
                {
                    if (regs.display_disabled == false)
                    {
                        regs.oam_addr = (ushort)(regs.oam_baseaddr << 1);
                        regs.oam_firstsprite = (regs.oam_priority == false) ? (byte)0 : (byte)((regs.oam_addr >> 2) & 127);
                    }
                }
                foreach (var e in add_clocks(502))
                {
                    yield return e;
                };

                //H =  512 (render)
                render_scanline();
                foreach (var e in add_clocks(640))
                {
                    yield return e;
                };

                //H = 1152 (cache OBSEL)
                if (cache.oam_basesize != regs.oam_basesize)
                {
                    cache.oam_basesize = regs.oam_basesize;
                    sprite_list_valid = false;
                }
                cache.oam_nameselect = regs.oam_nameselect;
                cache.oam_tdaddr = regs.oam_tdaddr;
                foreach (var e in add_clocks(PPUCounter.lineclocks() - 1152U))
                {
                    yield return e;
                };  //seek to start of next scanline

            }
        }

        public void power()
        {
            ppu1_version = (byte)Configuration.config.ppu1.version;
            ppu2_version = (byte)Configuration.config.ppu2.version;

            for (uint i = 0; i < StaticRAM.vram.size(); i++)
            {
                StaticRAM.vram[i] = 0x00;
            }
            for (uint i = 0; i < StaticRAM.oam.size(); i++)
            {
                StaticRAM.oam[i] = 0x00;
            }
            for (uint i = 0; i < StaticRAM.cgram.size(); i++)
            {
                StaticRAM.cgram[i] = 0x00;
            }
            flush_tiledata_cache();

            region = (byte)(System.system.region == System.Region.NTSC ? 0 : 1);  //0 = NTSC, 1 = PAL

            regs.ioamaddr = 0x0000;
            regs.icgramaddr = 0x01ff;

            //$2100
            regs.display_disabled = true;
            regs.display_brightness = 15;

            //$2101
            regs.oam_basesize = 0;
            regs.oam_nameselect = 0;
            regs.oam_tdaddr = 0x0000;

            cache.oam_basesize = 0;
            cache.oam_nameselect = 0;
            cache.oam_tdaddr = 0x0000;

            //$2102-$2103
            regs.oam_baseaddr = 0x0000;
            regs.oam_addr = 0x0000;
            regs.oam_priority = false;
            regs.oam_firstsprite = 0;

            //$2104
            regs.oam_latchdata = 0x00;

            //$2105
            regs.bg_tilesize[(int)ID.BG1] = Convert.ToBoolean(0);
            regs.bg_tilesize[(int)ID.BG2] = Convert.ToBoolean(0);
            regs.bg_tilesize[(int)ID.BG3] = Convert.ToBoolean(0);
            regs.bg_tilesize[(int)ID.BG4] = Convert.ToBoolean(0);
            regs.bg3_priority = Convert.ToBoolean(0);
            regs.bg_mode = 0;

            //$2106
            regs.mosaic_size = 0;
            regs.mosaic_enabled[(int)ID.BG1] = false;
            regs.mosaic_enabled[(int)ID.BG2] = false;
            regs.mosaic_enabled[(int)ID.BG3] = false;
            regs.mosaic_enabled[(int)ID.BG4] = false;
            regs.mosaic_countdown = 0;

            //$2107-$210a
            regs.bg_scaddr[(int)ID.BG1] = 0x0000;
            regs.bg_scaddr[(int)ID.BG2] = 0x0000;
            regs.bg_scaddr[(int)ID.BG3] = 0x0000;
            regs.bg_scaddr[(int)ID.BG4] = 0x0000;
            regs.bg_scsize[(int)ID.BG1] = (byte)SC.S32x32;
            regs.bg_scsize[(int)ID.BG2] = (byte)SC.S32x32;
            regs.bg_scsize[(int)ID.BG3] = (byte)SC.S32x32;
            regs.bg_scsize[(int)ID.BG4] = (byte)SC.S32x32;

            //$210b-$210c
            regs.bg_tdaddr[(int)ID.BG1] = 0x0000;
            regs.bg_tdaddr[(int)ID.BG2] = 0x0000;
            regs.bg_tdaddr[(int)ID.BG3] = 0x0000;
            regs.bg_tdaddr[(int)ID.BG4] = 0x0000;

            //$210d-$2114
            regs.bg_ofslatch = 0x00;
            regs.m7_hofs = regs.m7_vofs = 0x0000;
            regs.bg_hofs[(int)ID.BG1] = regs.bg_vofs[(int)ID.BG1] = 0x0000;
            regs.bg_hofs[(int)ID.BG2] = regs.bg_vofs[(int)ID.BG2] = 0x0000;
            regs.bg_hofs[(int)ID.BG3] = regs.bg_vofs[(int)ID.BG3] = 0x0000;
            regs.bg_hofs[(int)ID.BG4] = regs.bg_vofs[(int)ID.BG4] = 0x0000;

            //$2115
            regs.vram_incmode = Convert.ToBoolean(1);
            regs.vram_mapping = 0;
            regs.vram_incsize = 1;

            //$2116-$2117
            regs.vram_addr = 0x0000;

            //$211a
            regs.mode7_repeat = 0;
            regs.mode7_vflip = false;
            regs.mode7_hflip = false;

            //$211b-$2120
            regs.m7_latch = 0x00;
            regs.m7a = 0x0000;
            regs.m7b = 0x0000;
            regs.m7c = 0x0000;
            regs.m7d = 0x0000;
            regs.m7x = 0x0000;
            regs.m7y = 0x0000;

            //$2121
            regs.cgram_addr = 0x0000;

            //$2122
            regs.cgram_latchdata = 0x00;

            //$2123-$2125
            regs.window1_enabled[(int)ID.BG1] = false;
            regs.window1_enabled[(int)ID.BG2] = false;
            regs.window1_enabled[(int)ID.BG3] = false;
            regs.window1_enabled[(int)ID.BG4] = false;
            regs.window1_enabled[(int)ID.OAM] = false;
            regs.window1_enabled[(int)ID.COL] = false;

            regs.window1_invert[(int)ID.BG1] = false;
            regs.window1_invert[(int)ID.BG2] = false;
            regs.window1_invert[(int)ID.BG3] = false;
            regs.window1_invert[(int)ID.BG4] = false;
            regs.window1_invert[(int)ID.OAM] = false;
            regs.window1_invert[(int)ID.COL] = false;

            regs.window2_enabled[(int)ID.BG1] = false;
            regs.window2_enabled[(int)ID.BG2] = false;
            regs.window2_enabled[(int)ID.BG3] = false;
            regs.window2_enabled[(int)ID.BG4] = false;
            regs.window2_enabled[(int)ID.OAM] = false;
            regs.window2_enabled[(int)ID.COL] = false;

            regs.window2_invert[(int)ID.BG1] = false;
            regs.window2_invert[(int)ID.BG2] = false;
            regs.window2_invert[(int)ID.BG3] = false;
            regs.window2_invert[(int)ID.BG4] = false;
            regs.window2_invert[(int)ID.OAM] = false;
            regs.window2_invert[(int)ID.COL] = false;

            //$2126-$2129
            regs.window1_left = 0x00;
            regs.window1_right = 0x00;
            regs.window2_left = 0x00;
            regs.window2_right = 0x00;

            //$212a-$212b
            regs.window_mask[(int)ID.BG1] = 0;
            regs.window_mask[(int)ID.BG2] = 0;
            regs.window_mask[(int)ID.BG3] = 0;
            regs.window_mask[(int)ID.BG4] = 0;
            regs.window_mask[(int)ID.OAM] = 0;
            regs.window_mask[(int)ID.COL] = 0;

            //$212c-$212d
            regs.bg_enabled[(int)ID.BG1] = false;
            regs.bg_enabled[(int)ID.BG2] = false;
            regs.bg_enabled[(int)ID.BG3] = false;
            regs.bg_enabled[(int)ID.BG4] = false;
            regs.bg_enabled[(int)ID.OAM] = false;
            regs.bgsub_enabled[(int)ID.BG1] = false;
            regs.bgsub_enabled[(int)ID.BG2] = false;
            regs.bgsub_enabled[(int)ID.BG3] = false;
            regs.bgsub_enabled[(int)ID.BG4] = false;
            regs.bgsub_enabled[(int)ID.OAM] = false;

            //$212e-$212f
            regs.window_enabled[(int)ID.BG1] = false;
            regs.window_enabled[(int)ID.BG2] = false;
            regs.window_enabled[(int)ID.BG3] = false;
            regs.window_enabled[(int)ID.BG4] = false;
            regs.window_enabled[(int)ID.OAM] = false;
            regs.sub_window_enabled[(int)ID.BG1] = false;
            regs.sub_window_enabled[(int)ID.BG2] = false;
            regs.sub_window_enabled[(int)ID.BG3] = false;
            regs.sub_window_enabled[(int)ID.BG4] = false;
            regs.sub_window_enabled[(int)ID.OAM] = false;

            //$2130
            regs.color_mask = 0;
            regs.colorsub_mask = 0;
            regs.addsub_mode = false;
            regs.direct_color = false;

            //$2131
            regs.color_mode = Convert.ToBoolean(0);
            regs.color_halve = false;
            regs.color_enabled[(int)ID.BACK] = false;
            regs.color_enabled[(int)ID.OAM] = false;
            regs.color_enabled[(int)ID.BG4] = false;
            regs.color_enabled[(int)ID.BG3] = false;
            regs.color_enabled[(int)ID.BG2] = false;
            regs.color_enabled[(int)ID.BG1] = false;

            //$2132
            regs.color_r = 0x00;
            regs.color_g = 0x00;
            regs.color_b = 0x00;
            regs.color_rgb = 0x0000;

            //$2133
            regs.mode7_extbg = false;
            regs.pseudo_hires = false;
            regs.overscan = false;
            regs.scanlines = 224;
            regs.oam_interlace = false;
            regs.interlace = false;

            //$2137
            regs.hcounter = 0;
            regs.vcounter = 0;
            regs.latch_hcounter = Convert.ToBoolean(0);
            regs.latch_vcounter = Convert.ToBoolean(0);
            regs.counters_latched = false;

            //$2139-$213a
            regs.vram_readbuffer = 0x0000;

            //$213e
            regs.time_over = false;
            regs.range_over = false;

            reset();
        }

        public void reset()
        {
            Processor.create(enter(), System.system.cpu_frequency);
            PPUCounter.reset();
            Array.Clear(surface, 0, surface.Length);

            frame();

            //$2100
            regs.display_disabled = true;

            display.interlace = false;
            display.overscan = false;
            regs.scanlines = 224;

            Utility.InstantiateArrayElements(sprite_list);
            sprite_list_valid = false;

            //open bus support
            regs.ppu1_mdr = 0xff;
            regs.ppu2_mdr = 0xff;

            //bg line counters
            regs.bg_y[0] = 0;
            regs.bg_y[1] = 0;
            regs.bg_y[2] = 0;
            regs.bg_y[3] = 0;
        }

        public void serialize(Serializer s)
        {
            Processor.serialize(s);
            PPUCounter.serialize(s);

            s.integer(ppu1_version, "ppu1_version");
            s.integer(ppu2_version, "ppu2_version");

            s.integer(region, "region");
            s.integer(line, "line");

            s.integer(display.interlace, "display.interlace");
            s.integer(display.overscan, "display.overscan");

            s.integer(cache.oam_basesize, "cache.oam_basesize");
            s.integer(cache.oam_nameselect, "cache.oam_nameselect");
            s.integer(cache.oam_tdaddr, "cache.oam_tdaddr");

            s.integer(regs.ppu1_mdr, "regs.ppu1_mdr");
            s.integer(regs.ppu2_mdr, "regs.ppu2_mdr");
            for (uint n = 0; n < 4; n++)
            {
                s.integer(regs.bg_y[n], "regs.bg_y[n]");
            }

            s.integer(regs.ioamaddr, "regs.ioamaddr");
            s.integer(regs.icgramaddr, "regs.icgramaddr");

            s.integer(regs.display_disabled, "regs.display_disabled");
            s.integer(regs.display_brightness, "regs.display_brightness");

            s.integer(regs.oam_basesize, "regs.oam_basesize");
            s.integer(regs.oam_nameselect, "regs.oam_nameselect");
            s.integer(regs.oam_tdaddr, "regs.oam_tdaddr");

            s.integer(regs.oam_baseaddr, "regs.oam_baseaddr");
            s.integer(regs.oam_addr, "regs.oam_addr");
            s.integer(regs.oam_priority, "regs.oam_priority");
            s.integer(regs.oam_firstsprite, "regs.oam_firstsprite");

            s.integer(regs.oam_latchdata, "regs.oam_latchdata");

            for (uint n = 0; n < 4; n++)
            {
                s.integer(regs.bg_tilesize[n], "regs.bg_tilesize[n]");
            }
            s.integer(regs.bg3_priority, "regs.bg3_priority");
            s.integer(regs.bg_mode, "regs.bg_mode");

            s.integer(regs.mosaic_size, "regs.mosaic_size");
            for (uint n = 0; n < 4; n++)
            {
                s.integer(regs.mosaic_enabled[n], "regs.mosaic_enabled[n]");
            }
            s.integer(regs.mosaic_countdown, "regs.mosaic_countdown");

            for (uint n = 0; n < 4; n++)
            {
                s.integer(regs.bg_scaddr[n], "regs.bg_scaddr[n]");
            }
            for (uint n = 0; n < 4; n++)
            {
                s.integer(regs.bg_scsize[n], "regs.bg_scsize[n]");
            }

            for (uint n = 0; n < 4; n++)
            {
                s.integer(regs.bg_tdaddr[n], "regs.bg_tdaddr[n]");
            }

            s.integer(regs.bg_ofslatch, "regs.bg_ofslatch");
            s.integer(regs.m7_hofs, "regs.m7_hofs");
            s.integer(regs.m7_vofs, "regs.m7_vofs");
            for (uint n = 0; n < 4; n++)
            {
                s.integer(regs.bg_hofs[n], "regs.bg_hofs[n]");
            }
            for (uint n = 0; n < 4; n++)
            {
                s.integer(regs.bg_vofs[n], "regs.bg_vofs[n]");
            }

            s.integer(regs.vram_incmode, "regs.vram_incmode");
            s.integer(regs.vram_mapping, "regs.vram_mapping");
            s.integer(regs.vram_incsize, "regs.vram_incsize");

            s.integer(regs.vram_addr, "regs.vram_addr");

            s.integer(regs.mode7_repeat, "regs.mode7_repeat");
            s.integer(regs.mode7_vflip, "regs.mode7_vflip");
            s.integer(regs.mode7_hflip, "regs.mode7_hflip");

            s.integer(regs.m7_latch, "regs.m7_latch");
            s.integer(regs.m7a, "regs.m7a");
            s.integer(regs.m7b, "regs.m7b");
            s.integer(regs.m7c, "regs.m7c");
            s.integer(regs.m7d, "regs.m7d");
            s.integer(regs.m7x, "regs.m7x");
            s.integer(regs.m7y, "regs.m7y");

            s.integer(regs.cgram_addr, "regs.cgram_addr");

            s.integer(regs.cgram_latchdata, "regs.cgram_latchdata");

            for (uint n = 0; n < 6; n++)
            {
                s.integer(regs.window1_enabled[n], "regs.window1_enabled[n]");
            }
            for (uint n = 0; n < 6; n++)
            {
                s.integer(regs.window1_invert[n], "regs.window1_invert [n]");
            }
            for (uint n = 0; n < 6; n++)
            {
                s.integer(regs.window2_enabled[n], "regs.window2_enabled[n]");
            }
            for (uint n = 0; n < 6; n++)
            {
                s.integer(regs.window2_invert[n], "regs.window2_invert [n]");
            }

            s.integer(regs.window1_left, "regs.window1_left");
            s.integer(regs.window1_right, "regs.window1_right");
            s.integer(regs.window2_left, "regs.window2_left");
            s.integer(regs.window2_right, "regs.window2_right");

            for (uint n = 0; n < 6; n++)
            {
                s.integer(regs.window_mask[n], "regs.window_mask[n]");
            }
            for (uint n = 0; n < 5; n++)
            {
                s.integer(regs.bg_enabled[n], "regs.bg_enabled[n]");
            }
            for (uint n = 0; n < 5; n++)
            {
                s.integer(regs.bgsub_enabled[n], "regs.bgsub_enabled[n]");
            }
            for (uint n = 0; n < 5; n++)
            {
                s.integer(regs.window_enabled[n], "regs.window_enabled[n]");
            }
            for (uint n = 0; n < 5; n++)
            {
                s.integer(regs.sub_window_enabled[n], "regs.sub_window_enabled[n]");
            }

            s.integer(regs.color_mask, "regs.color_mask");
            s.integer(regs.colorsub_mask, "regs.colorsub_mask");
            s.integer(regs.addsub_mode, "regs.addsub_mode");
            s.integer(regs.direct_color, "regs.direct_color");

            s.integer(regs.color_mode, "regs.color_mode");
            s.integer(regs.color_halve, "regs.color_halve");
            for (uint n = 0; n < 6; n++)
            {
                s.integer(regs.color_enabled[n], "regs.color_enabled[n]");
            }

            s.integer(regs.color_r, "regs.color_r");
            s.integer(regs.color_g, "regs.color_g");
            s.integer(regs.color_b, "regs.color_b");
            s.integer(regs.color_rgb, "regs.color_rgb");

            s.integer(regs.mode7_extbg, "regs.mode7_extbg");
            s.integer(regs.pseudo_hires, "regs.pseudo_hires");
            s.integer(regs.overscan, "regs.overscan");
            s.integer(regs.scanlines, "regs.scanlines");
            s.integer(regs.oam_interlace, "regs.oam_interlace");
            s.integer(regs.interlace, "regs.interlace");

            s.integer(regs.hcounter, "regs.hcounter");
            s.integer(regs.vcounter, "regs.vcounter");
            s.integer(regs.latch_hcounter, "regs.latch_hcounter");
            s.integer(regs.latch_vcounter, "regs.latch_vcounter");
            s.integer(regs.counters_latched, "regs.counters_latched");

            s.integer(regs.vram_readbuffer, "regs.vram_readbuffer");

            s.integer(regs.time_over, "regs.time_over");
            s.integer(regs.range_over, "regs.range_over");
            s.integer(regs.oam_itemcount, "regs.oam_itemcount");
            s.integer(regs.oam_tilecount, "regs.oam_tilecount");

            for (uint n = 0; n < 256; n++)
            {
                s.integer(pixel_cache[n].src_main, "pixel_cache[n].src_main");
                s.integer(pixel_cache[n].src_sub, "pixel_cache[n].src_sub");
                s.integer(pixel_cache[n].bg_main, "pixel_cache[n].bg_main");
                s.integer(pixel_cache[n].bg_sub, "pixel_cache[n].bg_sub");
                s.integer(pixel_cache[n].ce_main, "pixel_cache[n].ce_main");
                s.integer(pixel_cache[n].ce_sub, "pixel_cache[n].ce_sub");
                s.integer(pixel_cache[n].pri_main, "pixel_cache[n].pri_main");
                s.integer(pixel_cache[n].pri_sub, "pixel_cache[n].pri_sub");
            }

            //better to just take a small speed hit than store all of bg_tiledata[3][] ...
            flush_tiledata_cache();

            for (uint n = 0; n < 6; n++)
            {
                s.array(window[n].main, 256, "window[n].main");
                s.array(window[n].sub, 256, "window[n].sub");
            }

            for (uint n = 0; n < 4; n++)
            {
                s.integer(bg_info[n].tw, "bg_info[n].tw");
                s.integer(bg_info[n].th, "bg_info[n].th");
                s.integer(bg_info[n].mx, "bg_info[n].mx");
                s.integer(bg_info[n].my, "bg_info[n].my");
                s.integer(bg_info[n].scx, "bg_info[n].scx");
                s.integer(bg_info[n].scy, "bg_info[n].scy");
            }

            for (uint n = 0; n < 128; n++)
            {
                s.integer(sprite_list[n].width, "sprite_list[n].width");
                s.integer(sprite_list[n].height, "sprite_list[n].height");
                s.integer(sprite_list[n].x, "sprite_list[n].x");
                s.integer(sprite_list[n].y, "sprite_list[n].y");
                s.integer(sprite_list[n].character, "sprite_list[n].character");
                s.integer(sprite_list[n].use_nameselect, "sprite_list[n].use_nameselect");
                s.integer(sprite_list[n].vflip, "sprite_list[n].vflip");
                s.integer(sprite_list[n].hflip, "sprite_list[n].hflip");
                s.integer(sprite_list[n].palette, "sprite_list[n].palette");
                s.integer(sprite_list[n].priority, "sprite_list[n].priority");
                s.integer(sprite_list[n].size, "sprite_list[n].size");
            }
            s.integer(sprite_list_valid, "sprite_list_valid");
            s.integer(active_sprite, "active_sprite");

            s.array(oam_itemlist, 32, "oam_itemlist");

            for (uint n = 0; n < 34; n++)
            {
                s.integer(oam_tilelist[n].x, "oam_tilelist[n].x");
                s.integer(oam_tilelist[n].y, "oam_tilelist[n].y");
                s.integer(oam_tilelist[n].pri, "oam_tilelist[n].pri");
                s.integer(oam_tilelist[n].pal, "oam_tilelist[n].pal");
                s.integer(oam_tilelist[n].tile, "oam_tilelist[n].tile");
                s.integer(oam_tilelist[n].hflip, "oam_tilelist[n].hflip");
            }

            s.array(oam_line_pal, 256, "oam_line_pal");
            s.array(oam_line_pri, 256, "oam_line_pri");
        }



        public PPU()
        {
            Utility.InstantiateArrayElements(pixel_cache);
            Utility.InstantiateArrayElements(window);
            Utility.InstantiateArrayElements(bg_info);
            Utility.InstantiateArrayElements(sprite_list);
            Utility.InstantiateArrayElements(oam_tilelist);

            //TODO: remove this hack
            surface = new ushort[1024 * 1024];
            output = new ArraySegment<ushort>(surface, 16 * 512, surface.Length - (16 * 512));

            alloc_tiledata_cache();

            for (uint l = 0; l < 16; l++)
            {
                mosaic_table[l] = new ushort[4096];
                for (uint i = 0; i < 4096; i++)
                {
                    mosaic_table[l][i] = (ushort)((i / (l + 1)) * (l + 1));
                }
            }

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
                            light_table[l][(r << 10) + (g << 5) + b] = (ushort)((ab << 10) + (ag << 5) + ar);
                        }
                    }
                }
            }
        }

        private PPUCounter _ppuCounter = new PPUCounter();
        public PPUCounter PPUCounter
        {
            get
            {
                return _ppuCounter;
            }
        }

        private Processor _processor = new Processor();
        public Processor Processor
        {
            get
            {
                return _processor;
            }
        }
    }
}
#endif
