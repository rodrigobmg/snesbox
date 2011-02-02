#if !FAST_PPU
using System;
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

        public void synchronize_cpu()
        {
            if (Processor.clock >= 0 && Scheduler.scheduler.sync != Scheduler.SynchronizeMode.All)
            {
                Libco.Switch(CPU.cpu.Processor.thread);
            }
        }

        public void latch_counters()
        {
            CPU.cpu.synchronize_ppu();
            regs.hcounter = PPUCounter.hdot();
            regs.vcounter = PPUCounter.vcounter();
            regs.counters_latched = true;
        }

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
            return true;
        }

        public void enter()
        {
            while (true)
            {
                if (Scheduler.scheduler.sync == Scheduler.SynchronizeMode.All)
                {
                    Scheduler.scheduler.exit(Scheduler.ExitReason.SynchronizeEvent);
                }

                scanline();
                add_clocks(88);

                if (PPUCounter.vcounter() <= (!regs.overscan ? 224 : 239))
                {
                    for (uint n = 0; n < 256; n++)
                    {
                        bg1.run();
                        bg2.run();
                        bg3.run();
                        bg4.run();
                        add_clocks(2);

                        bg1.run();
                        bg2.run();
                        bg3.run();
                        bg4.run();
                        oam.run();
                        window.run();
                        screen.run();
                        add_clocks(2);
                    }

                    add_clocks(22);
                    oam.tilefetch();
                }
                else
                {
                    add_clocks(1024 + 22 + 136);
                }

                add_clocks(PPUCounter.lineclocks() - 88U - 1024U - 22U - 136U);
            }
        }

        public void power()
        {
            ppu1_version = (byte)Configuration.config.ppu1.version;
            ppu2_version = (byte)Configuration.config.ppu2.version;

            Array.Clear(StaticRAM.vram.data(), 0, (int)StaticRAM.vram.size());
            Array.Clear(StaticRAM.oam.data(), 0, (int)StaticRAM.oam.size());
            Array.Clear(StaticRAM.cgram.data(), 0, (int)StaticRAM.cgram.size());

            reset();
        }

        public void reset()
        {
            Processor.create("PPU", Enter, System.system.cpu_frequency);
            PPUCounter.reset();
            Array.Clear(surface, 0, surface.Length);

            mmio_reset();
            bg1.reset();
            bg2.reset();
            bg3.reset();
            bg4.reset();
            oam.reset();
            window.reset();
            screen.reset();

            frame();
        }

        public void serialize(Serializer s)
        {
            Processor.serialize(s);
            PPUCounter.serialize(s);

            s.integer(ppu1_version, "ppu1_version");
            s.integer(ppu2_version, "ppu2_version");

            s.integer(display.interlace, "display.interlace");
            s.integer(display.overscan, "display.overscan");

            s.integer(regs.ppu1_mdr, "regs.ppu1_mdr");
            s.integer(regs.ppu2_mdr, "regs.ppu2_mdr");

            s.integer(regs.vram_readbuffer, "regs.vram_readbuffer");
            s.integer(regs.oam_latchdata, "regs.oam_latchdata");
            s.integer(regs.cgram_latchdata, "regs.cgram_latchdata");
            s.integer(regs.bgofs_latchdata, "regs.bgofs_latchdata");
            s.integer(regs.mode7_latchdata, "regs.mode7_latchdata");
            s.integer(regs.counters_latched, "regs.counters_latched");
            s.integer(regs.latch_hcounter, "regs.latch_hcounter");
            s.integer(regs.latch_vcounter, "regs.latch_vcounter");

            s.integer(regs.ioamaddr, "regs.ioamaddr");
            s.integer(regs.icgramaddr, "regs.icgramaddr");

            s.integer(regs.display_disabled, "regs.display_disabled");
            s.integer(regs.display_brightness, "regs.display_brightness");

            s.integer(regs.oam_baseaddr, "regs.oam_baseaddr");
            s.integer(regs.oam_addr, "regs.oam_addr");
            s.integer(regs.oam_priority, "regs.oam_priority");

            s.integer(regs.bg3_priority, "regs.bg3_priority");
            s.integer(regs.bgmode, "regs.bgmode");

            s.integer(regs.mode7_hoffset, "regs.mode7_hoffset");
            s.integer(regs.mode7_voffset, "regs.mode7_voffset");

            s.integer(regs.vram_incmode, "regs.vram_incmode");
            s.integer(regs.vram_mapping, "regs.vram_mapping");
            s.integer(regs.vram_incsize, "regs.vram_incsize");

            s.integer(regs.vram_addr, "regs.vram_addr");

            s.integer(regs.mode7_repeat, "regs.mode7_repeat");
            s.integer(regs.mode7_vflip, "regs.mode7_vflip");
            s.integer(regs.mode7_hflip, "regs.mode7_hflip");

            s.integer(regs.m7a, "regs.m7a");
            s.integer(regs.m7b, "regs.m7b");
            s.integer(regs.m7c, "regs.m7c");
            s.integer(regs.m7d, "regs.m7d");
            s.integer(regs.m7x, "regs.m7x");
            s.integer(regs.m7y, "regs.m7y");

            s.integer(regs.cgram_addr, "regs.cgram_addr");

            s.integer(regs.mode7_extbg, "regs.mode7_extbg");
            s.integer(regs.pseudo_hires, "regs.pseudo_hires");
            s.integer(regs.overscan, "regs.overscan");
            s.integer(regs.interlace, "regs.interlace");

            s.integer(regs.hcounter, "regs.hcounter");
            s.integer(regs.vcounter, "regs.vcounter");

            bg1.serialize(s);
            bg2.serialize(s);
            bg3.serialize(s);
            bg4.serialize(s);
            oam.serialize(s);
            window.serialize(s);
            screen.serialize(s);
        }

        public PPU()
        {
            bg1 = new Background(this, (uint)Background.ID.BG1);
            bg2 = new Background(this, (uint)Background.ID.BG2);
            bg3 = new Background(this, (uint)Background.ID.BG3);
            bg4 = new Background(this, (uint)Background.ID.BG4);
            oam = new Sprite(this);
            window = new Window(this);
            screen = new Screen(this);
            //TODO: Remove this hack
            surface = new ushort[1024 * 1024];
            output = new ArraySegment<ushort>(surface, 16 * 512, surface.Length - (16 * 512));
        }

        private Regs regs = new Regs();

        private ushort get_vram_address()
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

        private byte vram_read(uint addr)
        {
            if (regs.display_disabled || PPUCounter.vcounter() >= (!regs.overscan ? 225 : 240))
            {
                return StaticRAM.vram[addr];
            }
            return 0x00;
        }

        private void vram_write(uint addr, byte data)
        {
            if (regs.display_disabled || PPUCounter.vcounter() >= (!regs.overscan ? 225 : 240))
            {
                StaticRAM.vram[addr] = data;
            }
        }

        private byte oam_read(uint addr)
        {
            if (!regs.display_disabled && PPUCounter.vcounter() < (!regs.overscan ? 225 : 240))
            {
                addr = regs.ioamaddr;
            }
            if (Convert.ToBoolean(addr & 0x0200))
            {
                addr &= 0x021f;
            }
            return StaticRAM.oam[addr];
        }

        private void oam_write(uint addr, byte data)
        {
            if (!regs.display_disabled && PPUCounter.vcounter() < (!regs.overscan ? 225 : 240))
            {
                addr = regs.ioamaddr;
            }
            if (Convert.ToBoolean(addr & 0x0200))
            {
                addr &= 0x021f;
            }
            StaticRAM.oam[addr] = data;
            oam.update(addr, data);
        }

        private byte cgram_read(uint addr)
        {
            return StaticRAM.cgram[addr];
        }

        private void cgram_write(uint addr, byte data)
        {
            StaticRAM.cgram[addr] = data;
        }

        private void mmio_update_video_mode()
        {
            switch (regs.bgmode)
            {
                case 0:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP2;
                        bg1.regs.priority0 = 8;
                        bg1.regs.priority1 = 11;
                        bg2.regs.mode = (uint)Background.Mode.BPP2;
                        bg2.regs.priority0 = 7;
                        bg2.regs.priority1 = 10;
                        bg3.regs.mode = (uint)Background.Mode.BPP2;
                        bg3.regs.priority0 = 2;
                        bg3.regs.priority1 = 5;
                        bg4.regs.mode = (uint)Background.Mode.BPP2;
                        bg4.regs.priority0 = 1;
                        bg4.regs.priority1 = 4;
                        oam.regs.priority0 = 3;
                        oam.regs.priority1 = 6;
                        oam.regs.priority2 = 9;
                        oam.regs.priority3 = 12;
                    }
                    break;
                case 1:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP4;
                        bg2.regs.mode = (uint)Background.Mode.BPP4;
                        bg3.regs.mode = (uint)Background.Mode.BPP2;
                        bg4.regs.mode = (uint)Background.Mode.Inactive;
                        if (regs.bg3_priority)
                        {
                            bg1.regs.priority0 = 5;
                            bg1.regs.priority1 = 8;
                            bg2.regs.priority0 = 4;
                            bg2.regs.priority1 = 7;
                            bg3.regs.priority0 = 1;
                            bg3.regs.priority1 = 10;
                            oam.regs.priority0 = 2;
                            oam.regs.priority1 = 3;
                            oam.regs.priority2 = 6;
                            oam.regs.priority3 = 9;
                        }
                        else
                        {
                            bg1.regs.priority0 = 6;
                            bg1.regs.priority1 = 9;
                            bg2.regs.priority0 = 5;
                            bg2.regs.priority1 = 8;
                            bg3.regs.priority0 = 1;
                            bg3.regs.priority1 = 3;
                            oam.regs.priority0 = 2;
                            oam.regs.priority1 = 4;
                            oam.regs.priority2 = 7;
                            oam.regs.priority3 = 10;
                        }
                    }
                    break;
                case 2:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP4;
                        bg2.regs.mode = (uint)Background.Mode.BPP4;
                        bg3.regs.mode = (uint)Background.Mode.Inactive;
                        bg4.regs.mode = (uint)Background.Mode.Inactive;
                        bg1.regs.priority0 = 3;
                        bg1.regs.priority1 = 7;
                        bg2.regs.priority0 = 1;
                        bg2.regs.priority1 = 5;
                        oam.regs.priority0 = 2;
                        oam.regs.priority1 = 4;
                        oam.regs.priority2 = 6;
                        oam.regs.priority3 = 8;
                    }
                    break;
                case 3:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP8;
                        bg2.regs.mode = (uint)Background.Mode.BPP4;
                        bg3.regs.mode = (uint)Background.Mode.Inactive;
                        bg4.regs.mode = (uint)Background.Mode.Inactive;
                        bg1.regs.priority0 = 3;
                        bg1.regs.priority1 = 7;
                        bg2.regs.priority0 = 1;
                        bg2.regs.priority1 = 5;
                        oam.regs.priority0 = 2;
                        oam.regs.priority1 = 4;
                        oam.regs.priority2 = 6;
                        oam.regs.priority3 = 8;
                    }
                    break;
                case 4:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP8;
                        bg2.regs.mode = (uint)Background.Mode.BPP2;
                        bg3.regs.mode = (uint)Background.Mode.Inactive;
                        bg4.regs.mode = (uint)Background.Mode.Inactive;
                        bg1.regs.priority0 = 3;
                        bg1.regs.priority1 = 7;
                        bg2.regs.priority0 = 1;
                        bg2.regs.priority1 = 5;
                        oam.regs.priority0 = 2;
                        oam.regs.priority1 = 4;
                        oam.regs.priority2 = 6;
                        oam.regs.priority3 = 8;
                    }
                    break;
                case 5:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP4;
                        bg2.regs.mode = (uint)Background.Mode.BPP2;
                        bg3.regs.mode = (uint)Background.Mode.Inactive;
                        bg4.regs.mode = (uint)Background.Mode.Inactive;
                        bg1.regs.priority0 = 3;
                        bg1.regs.priority1 = 7;
                        bg2.regs.priority0 = 1;
                        bg2.regs.priority1 = 5;
                        oam.regs.priority0 = 2;
                        oam.regs.priority1 = 4;
                        oam.regs.priority2 = 6;
                        oam.regs.priority3 = 8;
                    }
                    break;
                case 6:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP4;
                        bg2.regs.mode = (uint)Background.Mode.Inactive;
                        bg3.regs.mode = (uint)Background.Mode.Inactive;
                        bg4.regs.mode = (uint)Background.Mode.Inactive;
                        bg1.regs.priority0 = 2;
                        bg1.regs.priority1 = 5;
                        oam.regs.priority0 = 1;
                        oam.regs.priority1 = 3;
                        oam.regs.priority2 = 4;
                        oam.regs.priority3 = 6;
                    }
                    break;
                case 7:
                    {
                        if (regs.mode7_extbg == false)
                        {
                            bg1.regs.mode = (uint)Background.Mode.Mode7;
                            bg2.regs.mode = (uint)Background.Mode.Inactive;
                            bg3.regs.mode = (uint)Background.Mode.Inactive;
                            bg4.regs.mode = (uint)Background.Mode.Inactive;
                            bg1.regs.priority0 = 2;
                            bg1.regs.priority1 = 2;
                            oam.regs.priority0 = 1;
                            oam.regs.priority1 = 3;
                            oam.regs.priority2 = 4;
                            oam.regs.priority3 = 5;
                        }
                        else
                        {
                            bg1.regs.mode = (uint)Background.Mode.Mode7;
                            bg2.regs.mode = (uint)Background.Mode.Mode7;
                            bg3.regs.mode = (uint)Background.Mode.Inactive;
                            bg4.regs.mode = (uint)Background.Mode.Inactive;
                            bg1.regs.priority0 = 3;
                            bg1.regs.priority1 = 3;
                            bg2.regs.priority0 = 1;
                            bg2.regs.priority1 = 5;
                            oam.regs.priority0 = 2;
                            oam.regs.priority1 = 4;
                            oam.regs.priority2 = 6;
                            oam.regs.priority3 = 7;
                        }
                    }
                    break;
            }
        }

        private void mmio_w2100(byte data)
        {
            if (regs.display_disabled && PPUCounter.vcounter() == (!regs.overscan ? 225 : 240))
            {
                oam.address_reset();
            }
            regs.display_disabled = Convert.ToBoolean(data & 0x80);
            regs.display_brightness = (uint)(data & 0x0f);
        }  //INIDISP

        private void mmio_w2101(byte data)
        {
            oam.regs.base_size = (byte)((data >> 5) & 7);
            oam.regs.nameselect = (byte)((data >> 3) & 3);
            oam.regs.tiledata_addr = (ushort)((data & 3) << 14);
        }  //OBSEL

        private void mmio_w2102(byte data)
        {
            regs.oam_baseaddr &= 0x0100;
            regs.oam_baseaddr |= (ushort)(data << 0);
            oam.address_reset();
        }  //OAMADDL

        private void mmio_w2103(byte data)
        {
            regs.oam_priority = Convert.ToBoolean(data & 0x80);
            regs.oam_baseaddr &= 0x00ff;
            regs.oam_baseaddr |= (ushort)((data & 1) << 8);
            oam.address_reset();
        }  //OAMADDH

        private void mmio_w2104(byte data)
        {
            if (Convert.ToBoolean(regs.oam_addr & 0x0200))
            {
                oam_write(regs.oam_addr, data);
            }
            else if ((regs.oam_addr & 1) == 0)
            {
                regs.oam_latchdata = data;
            }
            else
            {
                oam_write((uint)((regs.oam_addr & ~1) + 0), regs.oam_latchdata);
                oam_write((uint)((regs.oam_addr & ~1) + 1), data);
            }

            regs.oam_addr = (ushort)((regs.oam_addr + 1) & 0x03ff);
            oam.regs.first_sprite = (byte)(regs.oam_priority == false ? 0 : (regs.oam_addr >> 2) & 127);
        }  //OAMDATA

        private void mmio_w2105(byte data)
        {
            bg4.regs.tile_size = Convert.ToBoolean(data & 0x80);
            bg3.regs.tile_size = Convert.ToBoolean(data & 0x40);
            bg2.regs.tile_size = Convert.ToBoolean(data & 0x20);
            bg1.regs.tile_size = Convert.ToBoolean(data & 0x10);
            regs.bg3_priority = Convert.ToBoolean(data & 0x08);
            regs.bgmode = (byte)(data & 0x07);
            mmio_update_video_mode();
        }  //BGMODE

        private void mmio_w2106(byte data)
        {
            uint mosaic_size = (uint)((data >> 4) & 15);
            bg4.regs.mosaic = (Convert.ToBoolean(data & 0x08) ? mosaic_size : 0);
            bg3.regs.mosaic = (Convert.ToBoolean(data & 0x04) ? mosaic_size : 0);
            bg2.regs.mosaic = (Convert.ToBoolean(data & 0x02) ? mosaic_size : 0);
            bg1.regs.mosaic = (Convert.ToBoolean(data & 0x01) ? mosaic_size : 0);
        }  //MOSAIC

        private void mmio_w2107(byte data)
        {
            bg1.regs.screen_addr = (uint)((data & 0x7c) << 9);
            bg1.regs.screen_size = (uint)(data & 3);
        }  //BG1SC

        private void mmio_w2108(byte data)
        {
            bg2.regs.screen_addr = (uint)((data & 0x7c) << 9);
            bg2.regs.screen_size = (uint)(data & 3);
        }  //BG2SC

        private void mmio_w2109(byte data)
        {
            bg3.regs.screen_addr = (uint)((data & 0x7c) << 9);
            bg3.regs.screen_size = (uint)(data & 3);
        }  //BG3SC

        private void mmio_w210a(byte data)
        {
            bg4.regs.screen_addr = (uint)((data & 0x7c) << 9);
            bg4.regs.screen_size = (uint)(data & 3);
        }  //BG4SC

        private void mmio_w210b(byte data)
        {
            bg1.regs.tiledata_addr = (uint)((data & 0x07) << 13);
            bg2.regs.tiledata_addr = (uint)((data & 0x70) << 9);
        }  //BG12NBA

        private void mmio_w210c(byte data)
        {
            bg3.regs.tiledata_addr = (uint)((data & 0x07) << 13);
            bg4.regs.tiledata_addr = (uint)((data & 0x70) << 9);
        }  //BG34NBA

        private void mmio_w210d(byte data)
        {
            regs.mode7_hoffset = (ushort)((data << 8) | regs.mode7_latchdata);
            regs.mode7_latchdata = data;

            bg1.regs.hoffset = (uint)(data << 8) | (uint)(regs.bgofs_latchdata & ~7) | ((bg1.regs.hoffset >> 8) & 7);
            regs.bgofs_latchdata = data;
        }  //BG1HOFS

        private void mmio_w210e(byte data)
        {
            regs.mode7_voffset = (ushort)((data << 8) | regs.mode7_latchdata);
            regs.mode7_latchdata = data;

            bg1.regs.voffset = (uint)((data << 8) | regs.bgofs_latchdata);
            regs.bgofs_latchdata = data;
        }  //BG1VOFS

        private void mmio_w210f(byte data)
        {
            bg2.regs.hoffset = (uint)(data << 8) | (uint)(regs.bgofs_latchdata & ~7) | ((bg2.regs.hoffset >> 8) & 7);
            regs.bgofs_latchdata = data;
        }  //BG2HOFS

        private void mmio_w2110(byte data)
        {
            bg2.regs.voffset = (uint)((data << 8) | regs.bgofs_latchdata);
            regs.bgofs_latchdata = data;
        }  //BG2VOFS

        private void mmio_w2111(byte data)
        {
            bg3.regs.hoffset = (uint)(data << 8) | (uint)(regs.bgofs_latchdata & ~7) | ((bg3.regs.hoffset >> 8) & 7);
            regs.bgofs_latchdata = data;
        }  //BG3HOFS

        private void mmio_w2112(byte data)
        {
            bg3.regs.voffset = (uint)((data << 8) | regs.bgofs_latchdata);
            regs.bgofs_latchdata = data;
        }  //BG3VOFS

        private void mmio_w2113(byte data)
        {
            bg4.regs.hoffset = (uint)(data << 8) | (uint)(regs.bgofs_latchdata & ~7) | ((bg4.regs.hoffset >> 8) & 7);
            regs.bgofs_latchdata = data;
        }  //BG4HOFS

        private void mmio_w2114(byte data)
        {
            bg4.regs.voffset = (uint)((data << 8) | regs.bgofs_latchdata);
            regs.bgofs_latchdata = data;
        }  //BG4VOFS

        private void mmio_w2115(byte data)
        {
            regs.vram_incmode = Convert.ToBoolean(data & 0x80);
            regs.vram_mapping = (byte)((data >> 2) & 3);
            switch (data & 3)
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

        private void mmio_w2116(byte data)
        {
            regs.vram_addr &= 0xff00;
            regs.vram_addr |= (ushort)(data << 0);
            ushort addr = get_vram_address();
            regs.vram_readbuffer = (ushort)(vram_read(addr + 0U) << 0);
            regs.vram_readbuffer |= (ushort)(vram_read(addr + 1U) << 8);
        }  //VMADDL

        private void mmio_w2117(byte data)
        {
            regs.vram_addr &= 0x00ff;
            regs.vram_addr |= (ushort)(data << 8);
            ushort addr = get_vram_address();
            regs.vram_readbuffer = (ushort)(vram_read(addr + 0U) << 0);
            regs.vram_readbuffer |= (ushort)(vram_read(addr + 1U) << 8);
        }  //VMADDH

        private void mmio_w2118(byte data)
        {
            ushort addr = (ushort)(get_vram_address() + 0);
            vram_write(addr, data);
            if (regs.vram_incmode == Convert.ToBoolean(0))
            {
                regs.vram_addr += regs.vram_incsize;
            }
        }  //VMDATAL

        private void mmio_w2119(byte data)
        {
            ushort addr = (ushort)(get_vram_address() + 1);
            vram_write(addr, data);
            if (regs.vram_incmode == Convert.ToBoolean(1))
            {
                regs.vram_addr += regs.vram_incsize;
            }
        }  //VMDATAH

        private void mmio_w211a(byte data)
        {
            regs.mode7_repeat = (byte)((data >> 6) & 3);
            regs.mode7_vflip = Convert.ToBoolean(data & 0x02);
            regs.mode7_hflip = Convert.ToBoolean(data & 0x01);
        }  //M7SEL

        private void mmio_w211b(byte data)
        {
            regs.m7a = (ushort)((data << 8) | regs.mode7_latchdata);
            regs.mode7_latchdata = data;
        }  //M7A

        private void mmio_w211c(byte data)
        {
            regs.m7b = (ushort)((data << 8) | regs.mode7_latchdata);
            regs.mode7_latchdata = data;
        }  //M7B

        private void mmio_w211d(byte data)
        {
            regs.m7c = (ushort)((data << 8) | regs.mode7_latchdata);
            regs.mode7_latchdata = data;
        }  //M7C

        private void mmio_w211e(byte data)
        {
            regs.m7d = (ushort)((data << 8) | regs.mode7_latchdata);
            regs.mode7_latchdata = data;
        }  //M7D

        private void mmio_w211f(byte data)
        {
            regs.m7x = (ushort)((data << 8) | regs.mode7_latchdata);
            regs.mode7_latchdata = data;
        }  //M7X

        private void mmio_w2120(byte data)
        {
            regs.m7y = (ushort)((data << 8) | regs.mode7_latchdata);
            regs.mode7_latchdata = data;
        }  //M7Y

        private void mmio_w2121(byte data)
        {
            regs.cgram_addr = (ushort)(data << 1);
        }  //CGADD

        private void mmio_w2122(byte data)
        {
            if ((regs.cgram_addr & 1) == 0)
            {
                regs.cgram_latchdata = data;
            }
            else
            {
                cgram_write((uint)((regs.cgram_addr & ~1) + 0), regs.cgram_latchdata);
                cgram_write((uint)((regs.cgram_addr & ~1) + 1), (byte)(data & 0x7f));
            }
            regs.cgram_addr = (ushort)((regs.cgram_addr + 1) & 0x01ff);
        }  //CGDATA

        private void mmio_w2123(byte data)
        {
            window.regs.bg2_two_enable = Convert.ToBoolean(data & 0x80);
            window.regs.bg2_two_invert = Convert.ToBoolean(data & 0x40);
            window.regs.bg2_one_enable = Convert.ToBoolean(data & 0x20);
            window.regs.bg2_one_invert = Convert.ToBoolean(data & 0x10);
            window.regs.bg1_two_enable = Convert.ToBoolean(data & 0x08);
            window.regs.bg1_two_invert = Convert.ToBoolean(data & 0x04);
            window.regs.bg1_one_enable = Convert.ToBoolean(data & 0x02);
            window.regs.bg1_one_invert = Convert.ToBoolean(data & 0x01);
        }  //W12SEL

        private void mmio_w2124(byte data)
        {
            window.regs.bg4_two_enable = Convert.ToBoolean(data & 0x80);
            window.regs.bg4_two_invert = Convert.ToBoolean(data & 0x40);
            window.regs.bg4_one_enable = Convert.ToBoolean(data & 0x20);
            window.regs.bg4_one_invert = Convert.ToBoolean(data & 0x10);
            window.regs.bg3_two_enable = Convert.ToBoolean(data & 0x08);
            window.regs.bg3_two_invert = Convert.ToBoolean(data & 0x04);
            window.regs.bg3_one_enable = Convert.ToBoolean(data & 0x02);
            window.regs.bg3_one_invert = Convert.ToBoolean(data & 0x01);
        }  //W34SEL

        private void mmio_w2125(byte data)
        {
            window.regs.col_two_enable = Convert.ToBoolean(data & 0x80);
            window.regs.col_two_invert = Convert.ToBoolean(data & 0x40);
            window.regs.col_one_enable = Convert.ToBoolean(data & 0x20);
            window.regs.col_one_invert = Convert.ToBoolean(data & 0x10);
            window.regs.oam_two_enable = Convert.ToBoolean(data & 0x08);
            window.regs.oam_two_invert = Convert.ToBoolean(data & 0x04);
            window.regs.oam_one_enable = Convert.ToBoolean(data & 0x02);
            window.regs.oam_one_invert = Convert.ToBoolean(data & 0x01);
        }  //WOBJSEL

        private void mmio_w2126(byte data)
        {
            window.regs.one_left = data;
        }  //WH0

        private void mmio_w2127(byte data)
        {
            window.regs.one_right = data;
        }  //WH1

        private void mmio_w2128(byte data)
        {
            window.regs.two_left = data;
        }  //WH2

        private void mmio_w2129(byte data)
        {
            window.regs.two_right = data;
        }  //WH3

        private void mmio_w212a(byte data)
        {
            window.regs.bg4_mask = (byte)((data >> 6) & 3);
            window.regs.bg3_mask = (byte)((data >> 4) & 3);
            window.regs.bg2_mask = (byte)((data >> 2) & 3);
            window.regs.bg1_mask = (byte)((data >> 0) & 3);
        }  //WBGLOG

        private void mmio_w212b(byte data)
        {
            window.regs.col_mask = (byte)((data >> 2) & 3);
            window.regs.oam_mask = (byte)((data >> 0) & 3);
        }  //WOBJLOG

        private void mmio_w212c(byte data)
        {
            oam.regs.main_enabled = Convert.ToBoolean(data & 0x10);
            bg4.regs.main_enabled = Convert.ToBoolean(data & 0x08);
            bg3.regs.main_enabled = Convert.ToBoolean(data & 0x04);
            bg2.regs.main_enabled = Convert.ToBoolean(data & 0x02);
            bg1.regs.main_enabled = Convert.ToBoolean(data & 0x01);
        }  //TM

        private void mmio_w212d(byte data)
        {
            oam.regs.sub_enabled = Convert.ToBoolean(data & 0x10);
            bg4.regs.sub_enabled = Convert.ToBoolean(data & 0x08);
            bg3.regs.sub_enabled = Convert.ToBoolean(data & 0x04);
            bg2.regs.sub_enabled = Convert.ToBoolean(data & 0x02);
            bg1.regs.sub_enabled = Convert.ToBoolean(data & 0x01);
        }  //TS

        private void mmio_w212e(byte data)
        {
            window.regs.oam_main_enable = Convert.ToBoolean(data & 0x10);
            window.regs.bg4_main_enable = Convert.ToBoolean(data & 0x08);
            window.regs.bg3_main_enable = Convert.ToBoolean(data & 0x04);
            window.regs.bg2_main_enable = Convert.ToBoolean(data & 0x02);
            window.regs.bg1_main_enable = Convert.ToBoolean(data & 0x01);
        }  //TMW

        private void mmio_w212f(byte data)
        {
            window.regs.oam_sub_enable = Convert.ToBoolean(data & 0x10);
            window.regs.bg4_sub_enable = Convert.ToBoolean(data & 0x08);
            window.regs.bg3_sub_enable = Convert.ToBoolean(data & 0x04);
            window.regs.bg2_sub_enable = Convert.ToBoolean(data & 0x02);
            window.regs.bg1_sub_enable = Convert.ToBoolean(data & 0x01);
        }  //TSW

        private void mmio_w2130(byte data)
        {
            window.regs.col_main_mask = (byte)((data >> 6) & 3);
            window.regs.col_sub_mask = (byte)((data >> 4) & 3);
            screen.regs.addsub_mode = Convert.ToBoolean(data & 0x02);
            screen.regs.direct_color = Convert.ToBoolean(data & 0x01);
        }  //CGWSEL

        private void mmio_w2131(byte data)
        {
            screen.regs.color_mode = Convert.ToBoolean(data & 0x80);
            screen.regs.color_halve = Convert.ToBoolean(data & 0x40);
            screen.regs.back_color_enable = Convert.ToBoolean(data & 0x20);
            screen.regs.oam_color_enable = Convert.ToBoolean(data & 0x10);
            screen.regs.bg4_color_enable = Convert.ToBoolean(data & 0x08);
            screen.regs.bg3_color_enable = Convert.ToBoolean(data & 0x04);
            screen.regs.bg2_color_enable = Convert.ToBoolean(data & 0x02);
            screen.regs.bg1_color_enable = Convert.ToBoolean(data & 0x01);
        }  //CGADDSUB

        private void mmio_w2132(byte data)
        {
            if (Convert.ToBoolean(data & 0x80))
            {
                screen.regs.color_b = (byte)(data & 0x1f);
            }
            if (Convert.ToBoolean(data & 0x40))
            {
                screen.regs.color_g = (byte)(data & 0x1f);
            }
            if (Convert.ToBoolean(data & 0x20))
            {
                screen.regs.color_r = (byte)(data & 0x1f);
            }
        }  //COLDATA

        private void mmio_w2133(byte data)
        {
            regs.mode7_extbg = Convert.ToBoolean(data & 0x40);
            regs.pseudo_hires = Convert.ToBoolean(data & 0x08);
            regs.overscan = Convert.ToBoolean(data & 0x04);
            oam.regs.interlace = Convert.ToBoolean(data & 0x02);
            regs.interlace = Convert.ToBoolean(data & 0x01);
            mmio_update_video_mode();
        }  //SETINI

        private byte mmio_r2134()
        {
            uint result = (uint)((short)regs.m7a * (sbyte)(regs.m7b >> 8));
            regs.ppu1_mdr = (byte)(result >> 0);
            return regs.ppu1_mdr;
        }  //MPYL

        private byte mmio_r2135()
        {
            uint result = (uint)((short)regs.m7a * (sbyte)(regs.m7b >> 8));
            regs.ppu1_mdr = (byte)(result >> 8);
            return regs.ppu1_mdr;
        }  //MPYM

        private byte mmio_r2136()
        {
            uint result = (uint)((short)regs.m7a * (sbyte)(regs.m7b >> 8));
            regs.ppu1_mdr = (byte)(result >> 16);
            return regs.ppu1_mdr;
        }  //MPYH

        private byte mmio_r2137()
        {
            if (Convert.ToBoolean(CPU.cpu.pio() & 0x80))
            {
                latch_counters();
            }
            return CPU.cpu.regs.mdr;
        }  //SLHV

        private byte mmio_r2138()
        {
            regs.ppu1_mdr = oam_read(regs.oam_addr);
            regs.oam_addr = (ushort)((regs.oam_addr + 1) & 0x03ff);
            oam.regs.first_sprite = (byte)(regs.oam_priority == false ? 0 : (regs.oam_addr >> 2) & 127);
            return regs.ppu1_mdr;
        }  //OAMDATAREAD

        private byte mmio_r2139()
        {
            ushort addr = (ushort)(get_vram_address() + 0);
            regs.ppu1_mdr = (byte)(regs.vram_readbuffer >> 0);
            if (regs.vram_incmode == Convert.ToBoolean(0))
            {
                addr &= Bit.ToUint16(~1);
                regs.vram_readbuffer = (ushort)(vram_read(addr + 0U) << 0);
                regs.vram_readbuffer |= (ushort)(vram_read(addr + 1U) << 8);
                regs.vram_addr += regs.vram_incsize;
            }
            return regs.ppu1_mdr;
        }  //VMDATALREAD

        private byte mmio_r213a()
        {
            ushort addr = (ushort)(get_vram_address() + 1);
            regs.ppu1_mdr = (byte)(regs.vram_readbuffer >> 8);
            if (regs.vram_incmode == Convert.ToBoolean(1))
            {
                addr &= Bit.ToUint16(~1);
                regs.vram_readbuffer = (ushort)(vram_read(addr + 0U) << 0);
                regs.vram_readbuffer |= (ushort)(vram_read(addr + 1U) << 8);
                regs.vram_addr += regs.vram_incsize;
            }
            return regs.ppu1_mdr;
        }  //VMDATAHREAD

        private byte mmio_r213b()
        {
            if ((regs.cgram_addr & 1) == 0)
            {
                regs.ppu2_mdr = (byte)(cgram_read(regs.cgram_addr) & 0xff);
            }
            else
            {
                regs.ppu2_mdr &= 0x80;
                regs.ppu2_mdr |= (byte)(cgram_read(regs.cgram_addr) & 0x7f);
            }
            regs.cgram_addr = (ushort)((regs.cgram_addr + 1) & 0x01ff);
            return regs.ppu2_mdr;
        }  //CGDATAREAD

        private byte mmio_r213c()
        {
            if (regs.latch_hcounter == Convert.ToBoolean(0))
            {
                regs.ppu2_mdr = (byte)(regs.hcounter & 0xff);
            }
            else
            {
                regs.ppu2_mdr &= 0xfe;
                regs.ppu2_mdr |= (byte)((regs.hcounter >> 8) & 1);
            }
            regs.latch_hcounter = Convert.ToBoolean(Convert.ToInt32(regs.latch_hcounter) ^ 1);
            return regs.ppu2_mdr;
        }  //OPHCT

        private byte mmio_r213d()
        {
            if (regs.latch_vcounter == Convert.ToBoolean(0))
            {
                regs.ppu2_mdr = (byte)(regs.vcounter & 0xff);
            }
            else
            {
                regs.ppu2_mdr &= 0xfe;
                regs.ppu2_mdr |= (byte)((regs.vcounter >> 8) & 1);
            }
            regs.latch_vcounter = Convert.ToBoolean(Convert.ToInt32(regs.latch_vcounter) ^ 1);
            return regs.ppu2_mdr;
        }  //OPVCT

        private byte mmio_r213e()
        {
            regs.ppu1_mdr &= 0x10;
            regs.ppu1_mdr |= (byte)(Convert.ToInt32(oam.regs.time_over) << 7);
            regs.ppu1_mdr |= (byte)(Convert.ToInt32(oam.regs.range_over) << 6);
            regs.ppu1_mdr |= (byte)(ppu1_version & 0x0f);
            return regs.ppu1_mdr;
        }  //STAT77

        private byte mmio_r213f()
        {
            regs.latch_hcounter = Convert.ToBoolean(0);
            regs.latch_vcounter = Convert.ToBoolean(0);

            regs.ppu2_mdr &= 0x20;
            regs.ppu2_mdr |= (byte)(Convert.ToInt32(PPUCounter.field()) << 7);
            if ((CPU.cpu.pio() & 0x80) == 0)
            {
                regs.ppu2_mdr |= 0x40;
            }
            else if (regs.counters_latched)
            {
                regs.ppu2_mdr |= 0x40;
                regs.counters_latched = false;
            }
            regs.ppu2_mdr |= (byte)((System.system.region == System.Region.NTSC ? 0 : 1) << 4);
            regs.ppu2_mdr |= (byte)(ppu2_version & 0x0f);
            return regs.ppu2_mdr;
        }  //STAT78

        private void mmio_reset()
        {
            regs.ppu1_mdr = 0xff;
            regs.ppu2_mdr = 0xff;

            regs.vram_readbuffer = 0x0000;
            regs.oam_latchdata = 0x00;
            regs.cgram_latchdata = 0x00;
            regs.bgofs_latchdata = 0x00;
            regs.mode7_latchdata = 0x00;
            regs.counters_latched = false;
            regs.latch_hcounter = Convert.ToBoolean(0);
            regs.latch_vcounter = Convert.ToBoolean(0);

            regs.ioamaddr = 0;
            regs.icgramaddr = 0;

            //$2100  INIDISP
            regs.display_disabled = true;
            regs.display_brightness = 0;

            //$2102  OAMADDL
            //$2103  OAMADDH
            regs.oam_baseaddr = 0x0000;
            regs.oam_addr = 0x0000;
            regs.oam_priority = false;

            //$2105  BGMODE
            regs.bg3_priority = false;
            regs.bgmode = 0;

            //$210d  BG1HOFS
            regs.mode7_hoffset = 0x0000;

            //$210e  BG1VOFS
            regs.mode7_voffset = 0x0000;

            //$2115  VMAIN
            regs.vram_incmode = Convert.ToBoolean(1);
            regs.vram_mapping = 0;
            regs.vram_incsize = 1;

            //$2116  VMADDL
            //$2117  VMADDH
            regs.vram_addr = 0x0000;

            //$211a  M7SEL
            regs.mode7_repeat = 0;
            regs.mode7_vflip = false;
            regs.mode7_hflip = false;

            //$211b  M7A
            regs.m7a = 0x0000;

            //$211c  M7B
            regs.m7b = 0x0000;

            //$211d  M7C
            regs.m7c = 0x0000;

            //$211e  M7D
            regs.m7d = 0x0000;

            //$211f  M7X
            regs.m7x = 0x0000;

            //$2120  M7Y
            regs.m7y = 0x0000;

            //$2121  CGADD
            regs.cgram_addr = 0x0000;

            //$2133  SETINI
            regs.mode7_extbg = false;
            regs.pseudo_hires = false;
            regs.overscan = false;
            regs.interlace = false;

            //$213c  OPHCT
            regs.hcounter = 0;

            //$213d  OPVCT
            regs.vcounter = 0;
        }

        public byte mmio_read(uint addr)
        {
            CPU.cpu.synchronize_ppu();

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
                    return regs.ppu1_mdr;
                case 0x2134:
                    return mmio_r2134();  //MPYL
                case 0x2135:
                    return mmio_r2135();  //MPYM
                case 0x2136:
                    return mmio_r2136();  //MYPH
                case 0x2137:
                    return mmio_r2137();  //SLHV
                case 0x2138:
                    return mmio_r2138();  //OAMDATAREAD
                case 0x2139:
                    return mmio_r2139();  //VMDATALREAD
                case 0x213a:
                    return mmio_r213a();  //VMDATAHREAD
                case 0x213b:
                    return mmio_r213b();  //CGDATAREAD
                case 0x213c:
                    return mmio_r213c();  //OPHCT
                case 0x213d:
                    return mmio_r213d();  //OPVCT
                case 0x213e:
                    return mmio_r213e();  //STAT77
                case 0x213f:
                    return mmio_r213f();  //STAT78
            }

            return CPU.cpu.regs.mdr;
        }

        public void mmio_write(uint addr, byte data)
        {
            CPU.cpu.synchronize_ppu();

            switch (addr & 0xffff)
            {
                case 0x2100:
                    mmio_w2100(data);  //INIDISP
                    return;
                case 0x2101:
                    mmio_w2101(data);  //OBSEL
                    return;
                case 0x2102:
                    mmio_w2102(data);  //OAMADDL
                    return;
                case 0x2103:
                    mmio_w2103(data);  //OAMADDH
                    return;
                case 0x2104:
                    mmio_w2104(data);  //OAMDATA
                    return;
                case 0x2105:
                    mmio_w2105(data);  //BGMODE
                    return;
                case 0x2106:
                    mmio_w2106(data);  //MOSAIC
                    return;
                case 0x2107:
                    mmio_w2107(data);  //BG1SC
                    return;
                case 0x2108:
                    mmio_w2108(data);  //BG2SC
                    return;
                case 0x2109:
                    mmio_w2109(data);  //BG3SC
                    return;
                case 0x210a:
                    mmio_w210a(data);  //BG4SC
                    return;
                case 0x210b:
                    mmio_w210b(data);  //BG12NBA
                    return;
                case 0x210c:
                    mmio_w210c(data);  //BG34NBA
                    return;
                case 0x210d:
                    mmio_w210d(data);  //BG1HOFS
                    return;
                case 0x210e:
                    mmio_w210e(data);  //BG1VOFS
                    return;
                case 0x210f:
                    mmio_w210f(data);  //BG2HOFS
                    return;
                case 0x2110:
                    mmio_w2110(data);  //BG2VOFS
                    return;
                case 0x2111:
                    mmio_w2111(data);  //BG3HOFS
                    return;
                case 0x2112:
                    mmio_w2112(data);  //BG3VOFS
                    return;
                case 0x2113:
                    mmio_w2113(data);  //BG4HOFS
                    return;
                case 0x2114:
                    mmio_w2114(data);  //BG4VOFS
                    return;
                case 0x2115:
                    mmio_w2115(data);  //VMAIN
                    return;
                case 0x2116:
                    mmio_w2116(data);  //VMADDL
                    return;
                case 0x2117:
                    mmio_w2117(data);  //VMADDH
                    return;
                case 0x2118:
                    mmio_w2118(data);  //VMDATAL
                    return;
                case 0x2119:
                    mmio_w2119(data);  //VMDATAH
                    return;
                case 0x211a:
                    mmio_w211a(data);  //M7SEL
                    return;
                case 0x211b:
                    mmio_w211b(data);  //M7A
                    return;
                case 0x211c:
                    mmio_w211c(data);  //M7B
                    return;
                case 0x211d:
                    mmio_w211d(data);  //M7C
                    return;
                case 0x211e:
                    mmio_w211e(data);  //M7D
                    return;
                case 0x211f:
                    mmio_w211f(data);  //M7X
                    return;
                case 0x2120:
                    mmio_w2120(data);  //M7Y
                    return;
                case 0x2121:
                    mmio_w2121(data);  //CGADD
                    return;
                case 0x2122:
                    mmio_w2122(data);  //CGDATA
                    return;
                case 0x2123:
                    mmio_w2123(data);  //W12SEL
                    return;
                case 0x2124:
                    mmio_w2124(data);  //W34SEL
                    return;
                case 0x2125:
                    mmio_w2125(data);  //WOBJSEL
                    return;
                case 0x2126:
                    mmio_w2126(data);  //WH0
                    return;
                case 0x2127:
                    mmio_w2127(data);  //WH1
                    return;
                case 0x2128:
                    mmio_w2128(data);  //WH2
                    return;
                case 0x2129:
                    mmio_w2129(data);  //WH3
                    return;
                case 0x212a:
                    mmio_w212a(data);  //WBGLOG
                    return;
                case 0x212b:
                    mmio_w212b(data);  //WOBJLOG
                    return;
                case 0x212c:
                    mmio_w212c(data);  //TM
                    return;
                case 0x212d:
                    mmio_w212d(data);  //TS
                    return;
                case 0x212e:
                    mmio_w212e(data);  //TMW
                    return;
                case 0x212f:
                    mmio_w212f(data);  //TSW
                    return;
                case 0x2130:
                    mmio_w2130(data);  //CGWSEL
                    return;
                case 0x2131:
                    mmio_w2131(data);  //CGADDSUB
                    return;
                case 0x2132:
                    mmio_w2132(data);  //COLDATA
                    return;
                case 0x2133:
                    mmio_w2133(data);  //SETINI
                    return;
            }
        }

        private Background bg1;
        private Background bg2;
        private Background bg3;
        private Background bg4;
        private Sprite oam;
        private Window window;
        private Screen screen;

        private ushort[] surface;
        public ArraySegment<ushort> output;

        private byte ppu1_version;
        private byte ppu2_version;

        private Display display = new Display();

        private static void Enter()
        {
            PPU.ppu.enter();
        }

        private void add_clocks(uint clocks)
        {
            clocks >>= 1;
            while (Convert.ToBoolean(clocks--))
            {
                PPUCounter.tick(2);
                step(2);
                synchronize_cpu();
            }
        }

        private void scanline()
        {
            if (PPUCounter.vcounter() == 0)
            {
                frame();
            }
            bg1.scanline();
            bg2.scanline();
            bg3.scanline();
            bg4.scanline();
            oam.scanline();
            window.scanline();
            screen.scanline();
        }

        private void frame()
        {
            System.system.frame();
            oam.frame();

            display.interlace = regs.interlace;
            display.overscan = regs.overscan;
        }

        private Processor _processor = new Processor();
        public Processor Processor
        {
            get
            {
                return _processor;
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
    }
}
#endif