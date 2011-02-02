#if ACCURACY || COMPATIBILITY
using System;
using System.Collections.ObjectModel;
using Nall;

namespace Snes
{
    partial class CPU : CPUCore, IPPUCounter, IProcessor, IMMIO
    {
        public static CPU cpu = new CPU();

        public Collection<IProcessor> coprocessors = new Collection<IProcessor>();

        public void step(uint clocks)
        {
            SMP.smp.Processor.clock -= (long)(clocks * (ulong)SMP.smp.Processor.frequency);
            PPU.ppu.Processor.clock -= clocks;
            for (uint i = 0; i < coprocessors.Count; i++)
            {
                IProcessor chip = coprocessors[(int)i];
                chip.Processor.clock -= (long)(clocks * (ulong)chip.Processor.frequency);
            }
        }

        public void synchronize_smp()
        {
            if (SMP.smp.Processor.clock < 0)
            {
                Libco.Switch(SMP.smp.Processor.thread);
            }
        }

        public void synchronize_ppu()
        {
            if (PPU.ppu.Processor.clock < 0)
            {
                Libco.Switch(PPU.ppu.Processor.thread);
            }
        }

        public void synchronize_coprocessor()
        {
            for (uint i = 0; i < coprocessors.Count; i++)
            {
                IProcessor chip = coprocessors[(int)i];
                if (chip.Processor.clock < 0)
                {
                    Libco.Switch(chip.Processor.thread);
                }
            }
        }

        public byte port_read(uint2 port)
        {
            return status.port[(uint)port];
        }

        public void port_write(uint2 port, byte data)
        {
            status.port[(uint)port] = data;
        }

        public byte pio()
        {
            return status.pio;
        }

        public bool joylatch()
        {
            return status.joypad_strobe_latch;
        }

        public override bool interrupt_pending()
        {
            return status.interrupt_pending;
        }

        public void enter()
        {
            while (true)
            {
                if (Scheduler.scheduler.sync == Scheduler.SynchronizeMode.CPU)
                {
                    Scheduler.scheduler.sync = Scheduler.SynchronizeMode.All;
                    Scheduler.scheduler.exit(Scheduler.ExitReason.SynchronizeEvent);
                }

                if (status.interrupt_pending)
                {
                    status.interrupt_pending = false;
                    if (status.nmi_pending)
                    {
                        status.nmi_pending = false;
                        status.interrupt_vector = (ushort)(regs.e == false ? 0xffea : 0xfffa);
                        op_irq();
                    }
                    else if (status.irq_pending)
                    {
                        status.irq_pending = false;
                        status.interrupt_vector = (ushort)(regs.e == false ? 0xffee : 0xfffe);
                        op_irq();
                    }
                    else if (status.reset_pending)
                    {
                        status.reset_pending = false;
                        add_clocks(186);
                        regs.pc.l = Bus.bus.read(new uint24(0xfffc));
                        regs.pc.h = Bus.bus.read(new uint24(0xfffd));
                    }
                }

                op_step();
            }
        }

        public void power()
        {
            cpu_version = (byte)Configuration.config.cpu.version;

            regs.a.Assign(0x0000);
            regs.x.Assign(0x0000);
            regs.y.Assign(0x0000);
            regs.s.Assign(0x01ff);

            mmio_power();
            dma_power();
            timing_power();

            reset();
        }

        public void reset()
        {
            Processor.create("CPU", Enter, System.system.cpu_frequency);
            coprocessors.Clear();
            PPUCounter.reset();

            //note: some registers are not fully reset by SNES
            regs.pc.Assign(0x000000);
            regs.x.h = 0x00;
            regs.y.h = 0x00;
            regs.s.h = 0x01;
            regs.d.Assign(0x0000);
            regs.db = 0x00;
            regs.p.Assign(0x34);
            regs.e = Convert.ToBoolean(1);
            regs.mdr = 0x00;
            regs.wai = false;
            update_table();

            mmio_reset();
            dma_reset();
            timing_reset();
        }

        public void serialize(Serializer s)
        {
            Processor.serialize(s);
            base.core_serialize(s);
            PPUCounter.serialize(s);
            s.integer(cpu_version, "cpu_version");

            s.integer(status.interrupt_pending, "status.interrupt_pending");
            s.integer(status.interrupt_vector, "status.interrupt_vector");

            s.integer(status.clock_count, "status.clock_count");
            s.integer(status.line_clocks, "status.line_clocks");

            s.integer(status.irq_lock, "status.irq_lock");

            s.integer(status.dram_refresh_position, "status.dram_refresh_position");
            s.integer(status.dram_refreshed, "status.dram_refreshed");

            s.integer(status.hdma_init_position, "status.hdma_init_position");
            s.integer(status.hdma_init_triggered, "status.hdma_init_triggered");

            s.integer(status.hdma_position, "status.hdma_position");
            s.integer(status.hdma_triggered, "status.hdma_triggered");

            s.integer(status.nmi_valid, "status.nmi_valid");
            s.integer(status.nmi_line, "status.nmi_line");
            s.integer(status.nmi_transition, "status.nmi_transition");
            s.integer(status.nmi_pending, "status.nmi_pending");
            s.integer(status.nmi_hold, "status.nmi_hold");

            s.integer(status.irq_valid, "status.irq_valid");
            s.integer(status.irq_line, "status.irq_line");
            s.integer(status.irq_transition, "status.irq_transition");
            s.integer(status.irq_pending, "status.irq_pending");
            s.integer(status.irq_hold, "status.irq_hold");

            s.integer(status.reset_pending, "status.reset_pending");

            s.integer(status.dma_active, "status.dma_active");
            s.integer(status.dma_counter, "status.dma_counter");
            s.integer(status.dma_clocks, "status.dma_clocks");
            s.integer(status.dma_pending, "status.dma_pending");
            s.integer(status.hdma_pending, "status.hdma_pending");
            s.integer(status.hdma_mode, "status.hdma_mode");

            s.array(status.port, "status.port");

            s.integer((uint)status.wram_addr, "status.wram_addr");

            s.integer(status.joypad_strobe_latch, "status.joypad_strobe_latch");
            s.integer(status.joypad1_bits, "status.joypad1_bits");
            s.integer(status.joypad2_bits, "status.joypad2_bits");

            s.integer(status.nmi_enabled, "status.nmi_enabled");
            s.integer(status.hirq_enabled, "status.hirq_enabled");
            s.integer(status.virq_enabled, "status.virq_enabled");
            s.integer(status.auto_joypad_poll, "status.auto_joypad_poll");

            s.integer(status.pio, "status.pio");

            s.integer(status.wrmpya, "status.wrmpya");
            s.integer(status.wrmpyb, "status.wrmpyb");

            s.integer(status.wrdiva, "status.wrdiva");
            s.integer(status.wrdivb, "status.wrdivb");

            s.integer((uint)status.hirq_pos, "status.hirq_pos");
            s.integer((uint)status.virq_pos, "status.virq_pos");

            s.integer(status.rom_speed, "status.rom_speed");

            s.integer(status.rddiv, "status.rddiv");
            s.integer(status.rdmpy, "status.rdmpy");

            s.integer(status.joy1l, "status.joy1l");
            s.integer(status.joy1h, "status.joy1h");
            s.integer(status.joy2l, "status.joy2l");
            s.integer(status.joy2h, "status.joy2h");
            s.integer(status.joy3l, "status.joy3l");
            s.integer(status.joy3h, "status.joy3h");
            s.integer(status.joy4l, "status.joy4l");
            s.integer(status.joy4h, "status.joy4h");

            s.integer(alu.mpyctr, "alu.mpyctr");
            s.integer(alu.divctr, "alu.divctr");
            s.integer(alu.shift, "alu.shift");

            for (uint i = 0; i < 8; i++)
            {
                s.integer(channel[i].dma_enabled, "channel[i].dma_enabled");
                s.integer(channel[i].hdma_enabled, "channel[i].hdma_enabled");
                s.integer(channel[i].direction, "channel[i].direction");
                s.integer(channel[i].indirect, "channel[i].indirect");
                s.integer(channel[i].unused, "channel[i].unused");
                s.integer(channel[i].reverse_transfer, "channel[i].reverse_transfer");
                s.integer(channel[i].fixed_transfer, "channel[i].fixed_transfer");
                s.integer((uint)channel[i].transfer_mode, "channel[i].transfer_mode");
                s.integer(channel[i].dest_addr, "channel[i].dest_addr");
                s.integer(channel[i].source_addr, "channel[i].source_addr");
                s.integer(channel[i].source_bank, "channel[i].source_bank");
                s.integer(channel[i].union.transfer_size, "channel[i].transfer_size");
                s.integer(channel[i].indirect_bank, "channel[i].indirect_bank");
                s.integer(channel[i].hdma_addr, "channel[i].hdma_addr");
                s.integer(channel[i].line_counter, "channel[i].line_counter");
                s.integer(channel[i].unknown, "channel[i].unknown");
                s.integer(channel[i].hdma_completed, "channel[i].hdma_completed");
                s.integer(channel[i].hdma_do_transfer, "channel[i].hdma_do_transfer");
            }

            s.integer(pipe.valid, "pipe.valid");
            s.integer(pipe.addr, "pipe.addr");
            s.integer(pipe.data, "pipe.data");
        }

        public CPU()
        {
            PPUCounter.Scanline = this.scanline;

            for (int i = 0; i < channel.Length; i++)
            {
                channel[i] = new Channel();
            }
        }

        private Channel[] channel = new Channel[8];
        private Pipe pipe = new Pipe();

        private void dma_add_clocks(uint clocks)
        {
            status.dma_clocks += clocks;
            add_clocks(clocks);
        }

        private bool dma_transfer_valid(byte bbus, uint abus)
        {  //transfers from WRAM to WRAM are invalid; chip only has one address bus
            if (bbus == 0x80 && ((abus & 0xfe0000) == 0x7e0000 || (abus & 0x40e000) == 0x0000))
            {
                return false;
            }
            return true;
        }

        private bool dma_addr_valid(uint abus)
        {   //A-bus access to B-bus or S-CPU registers are invalid
            if ((abus & 0x40ff00) == 0x2100)
            {
                return false;  //$[00-3f|80-bf]:[2100-21ff]
            }
            if ((abus & 0x40fe00) == 0x4000)
            {
                return false;  //$[00-3f|80-bf]:[4000-41ff]}
            }
            if ((abus & 0x40ffe0) == 0x4200)
            {
                return false;  //$[00-3f|80-bf]:[4200-421f]
            }
            if ((abus & 0x40ff80) == 0x4300)
            {
                return false;  //$[00-3f|80-bf]:[4300-437f]
            }
            return true;
        }

        private byte dma_read(uint abus)
        {
            if (dma_addr_valid(abus) == false)
            {
                return 0x00;
            }
            return Bus.bus.read(new uint24(abus));
        }

        private void dma_write(bool valid, uint addr = 0, byte data = 0)
        {
            if (pipe.valid)
            {
                Bus.bus.write(new uint24(pipe.addr), pipe.data);
            }
            pipe.valid = valid;
            pipe.addr = addr;
            pipe.data = data;
        }

        private void dma_transfer(bool direction, byte bbus, uint abus)
        {
            if (direction == Convert.ToBoolean(0))
            {
                dma_add_clocks(4);
                regs.mdr = dma_read(abus);
                dma_add_clocks(4);
                dma_write(dma_transfer_valid(bbus, abus), 0x2100U | bbus, regs.mdr);
            }
            else
            {
                dma_add_clocks(4);
                regs.mdr = dma_transfer_valid(bbus, abus) ? Bus.bus.read(new uint24(0x2100U | bbus)) : (byte)0x00;
                dma_add_clocks(4);
                dma_write(dma_addr_valid(abus), abus, regs.mdr);
            }
        }

        private byte dma_bbus(uint i, uint index)
        {
            switch ((uint)channel[i].transfer_mode)
            {
                default:
                case 0:
                    return (channel[i].dest_addr);                              //0
                case 1:
                    return (byte)(channel[i].dest_addr + (index & 1));          //0,1
                case 2:
                    return (channel[i].dest_addr);                              //0,0
                case 3:
                    return (byte)(channel[i].dest_addr + ((index >> 1) & 1));   //0,0,1,1
                case 4:
                    return (byte)(channel[i].dest_addr + (index & 3));          //0,1,2,3
                case 5:
                    return (byte)(channel[i].dest_addr + (index & 1));          //0,1,0,1
                case 6:
                    return (channel[i].dest_addr);                              //0,0     [2]
                case 7:
                    return (byte)(channel[i].dest_addr + ((index >> 1) & 1));   //0,0,1,1 [3]
            }
        }

        private uint dma_addr(uint i)
        {
            uint r = (uint)((channel[i].source_bank << 16) | (channel[i].source_addr));

            if (channel[i].fixed_transfer == false)
            {
                if (channel[i].reverse_transfer == false)
                {
                    channel[i].source_addr++;
                }
                else
                {
                    channel[i].source_addr--;
                }
            }

            return r;
        }

        private uint hdma_addr(uint i)
        {
            return (uint)((channel[i].source_bank << 16) | (channel[i].hdma_addr++));
        }

        private uint hdma_iaddr(uint i)
        {
            return (uint)((channel[i].indirect_bank << 16) | (channel[i].union.indirect_addr++));
        }

        private byte dma_enabled_channels()
        {
            byte r = 0;
            for (uint i = 0; i < 8; i++)
            {
                if (channel[i].dma_enabled)
                {
                    r++;
                }
            }
            return r;
        }

        private bool hdma_active(uint i)
        {
            return (channel[i].hdma_enabled && !channel[i].hdma_completed);
        }

        private bool hdma_active_after(uint i)
        {
            for (uint n = i + 1; n < 8; n++)
            {
                if (hdma_active(n) == true)
                {
                    return true;
                }
            }
            return false;
        }

        private byte hdma_enabled_channels()
        {
            byte r = 0;
            for (uint i = 0; i < 8; i++)
            {
                if (channel[i].hdma_enabled)
                {
                    r++;
                }
            }
            return r;
        }

        private byte hdma_active_channels()
        {
            byte r = 0;
            for (uint i = 0; i < 8; i++)
            {
                if (hdma_active(i) == true)
                {
                    r++;
                }
            }
            return r;
        }

        private void dma_run()
        {
            dma_add_clocks(8);
            dma_write(false);
            dma_edge();

            for (uint i = 0; i < 8; i++)
            {
                if (channel[i].dma_enabled == false)
                {
                    continue;
                }

                uint index = 0;
                do
                {
                    dma_transfer(channel[i].direction, dma_bbus(i, index++), dma_addr(i));
                    dma_edge();
                }
                while (channel[i].dma_enabled && Convert.ToBoolean(--channel[i].union.transfer_size));

                dma_add_clocks(8);
                dma_write(false);
                dma_edge();

                channel[i].dma_enabled = false;
            }

            status.irq_lock = true;
        }

        private void hdma_update(uint i)
        {
            dma_add_clocks(4);
            regs.mdr = dma_read((uint)((channel[i].source_bank << 16) | channel[i].hdma_addr));
            dma_add_clocks(4);
            dma_write(false);

            if ((channel[i].line_counter & 0x7f) == 0)
            {
                channel[i].line_counter = regs.mdr;
                channel[i].hdma_addr++;

                channel[i].hdma_completed = (channel[i].line_counter == 0);
                channel[i].hdma_do_transfer = !channel[i].hdma_completed;

                if (channel[i].indirect)
                {
                    dma_add_clocks(4);
                    regs.mdr = dma_read(hdma_addr(i));
                    channel[i].union.indirect_addr = (ushort)(regs.mdr << 8);
                    dma_add_clocks(4);
                    dma_write(false);

                    if (!channel[i].hdma_completed || hdma_active_after(i))
                    {
                        dma_add_clocks(4);
                        regs.mdr = dma_read(hdma_addr(i));
                        channel[i].union.indirect_addr >>= 8;
                        channel[i].union.indirect_addr |= (ushort)(regs.mdr << 8);
                        dma_add_clocks(4);
                        dma_write(false);
                    }
                }
            }
        }

        private static readonly uint[] transfer_length = new uint[8] { 1, 2, 2, 4, 4, 4, 2, 4 };
        private void hdma_run()
        {
            dma_add_clocks(8);
            dma_write(false);

            for (uint i = 0; i < 8; i++)
            {
                if (hdma_active(i) == false)
                {
                    continue;
                }
                channel[i].dma_enabled = false;  //HDMA run during DMA will stop DMA mid-transfer

                if (channel[i].hdma_do_transfer)
                {
                    uint length = transfer_length[(uint)channel[i].transfer_mode];
                    for (uint index = 0; index < length; index++)
                    {
                        uint addr = channel[i].indirect == false ? hdma_addr(i) : hdma_iaddr(i);
                        dma_transfer(channel[i].direction, dma_bbus(i, index), addr);
                    }
                }
            }

            for (uint i = 0; i < 8; i++)
            {
                if (hdma_active(i) == false)
                {
                    continue;
                }

                channel[i].line_counter--;
                channel[i].hdma_do_transfer = Convert.ToBoolean(channel[i].line_counter & 0x80);
                hdma_update(i);
            }

            status.irq_lock = true;
        }

        private void hdma_init_reset()
        {
            for (uint i = 0; i < 8; i++)
            {
                channel[i].hdma_completed = false;
                channel[i].hdma_do_transfer = false;
            }
        }

        private void hdma_init()
        {
            dma_add_clocks(8);
            dma_write(false);

            for (uint i = 0; i < 8; i++)
            {
                if (!channel[i].hdma_enabled)
                {
                    continue;
                }
                channel[i].dma_enabled = false;  //HDMA init during DMA will stop DMA mid-transfer

                channel[i].hdma_addr = channel[i].source_addr;
                channel[i].line_counter = 0;
                hdma_update(i);
            }

            status.irq_lock = true;
        }

        private void dma_power()
        {
            for (uint i = 0; i < 8; i++)
            {
                channel[i].direction = Convert.ToBoolean(1);
                channel[i].indirect = true;
                channel[i].unused = true;
                channel[i].reverse_transfer = true;
                channel[i].fixed_transfer = true;
                channel[i].transfer_mode.Assign(7);

                channel[i].dest_addr = 0xff;

                channel[i].source_addr = 0xffff;
                channel[i].source_bank = 0xff;

                channel[i].union.transfer_size = 0xffff;
                channel[i].indirect_bank = 0xff;

                channel[i].hdma_addr = 0xffff;
                channel[i].line_counter = 0xff;
                channel[i].unknown = 0xff;
            }
        }

        private void dma_reset()
        {
            for (uint i = 0; i < 8; i++)
            {
                channel[i].dma_enabled = false;
                channel[i].hdma_enabled = false;

                channel[i].hdma_completed = false;
                channel[i].hdma_do_transfer = false;
            }

            pipe.valid = false;
            pipe.addr = 0;
            pipe.data = 0;
        }

        public override void op_io()
        {
            status.clock_count = 6;
            dma_edge();
            add_clocks(6);
            alu_edge();
        }

        public override byte op_read(uint addr)
        {
            status.clock_count = speed(addr);
            dma_edge();
            add_clocks(status.clock_count - 4);
            regs.mdr = Bus.bus.read(new uint24(addr));
            add_clocks(4);
            alu_edge();
            return regs.mdr;
        }

        public override void op_write(uint addr, byte data)
        {
            alu_edge();
            status.clock_count = speed(addr);
            dma_edge();
            add_clocks(status.clock_count);
            Bus.bus.write(new uint24(addr), regs.mdr = data);
        }

        private uint speed(uint addr)
        {
            if (Convert.ToBoolean(addr & 0x408000))
            {
                if (Convert.ToBoolean(addr & 0x800000))
                {
                    return status.rom_speed;
                }
                return 8;
            }
            if (Convert.ToBoolean((addr + 0x6000) & 0x4000))
            {
                return 8;
            }
            if (Convert.ToBoolean((addr - 0x4000) & 0x7e00))
            {
                return 6;
            }
            return 12;
        }

        private void mmio_power() { }

        private void mmio_reset()
        { 	//$2140-217f
            Array.Clear(status.port, 0, status.port.Length);

            //$2181-$2183
            status.wram_addr.Assign(0x000000);

            //$4016-$4017
            status.joypad_strobe_latch = Convert.ToBoolean(0);
            status.joypad1_bits = Bit.ToUint32(~0);
            status.joypad2_bits = Bit.ToUint32(~0);

            //$4200
            status.nmi_enabled = false;
            status.hirq_enabled = false;
            status.virq_enabled = false;
            status.auto_joypad_poll = false;

            //$4201
            status.pio = 0xff;

            //$4202-$4203
            status.wrmpya = 0xff;
            status.wrmpyb = 0xff;

            //$4204-$4206
            status.wrdiva = 0xffff;
            status.wrdivb = 0xff;

            //$4207-$420a
            status.hirq_pos.Assign(0x01ff);
            status.virq_pos.Assign(0x01ff);

            //$420d
            status.rom_speed = 8;

            //$4214-$4217
            status.rddiv = 0x0000;
            status.rdmpy = 0x0000;

            //$4218-$421f
            status.joy1l = 0x00;
            status.joy1h = 0x00;
            status.joy2l = 0x00;
            status.joy2h = 0x00;
            status.joy3l = 0x00;
            status.joy3h = 0x00;
            status.joy4l = 0x00;
            status.joy4h = 0x00;

            //ALU
            alu.mpyctr = 0;
            alu.divctr = 0;
            alu.shift = 0;
        }

        public byte mmio_read(uint addr)
        {
            addr &= 0xffff;

            //APU
            if ((addr & 0xffc0) == 0x2140)
            {  //$2140-$217f
                synchronize_smp();
                return SMP.smp.port_read(new uint2(addr));
            }

            //DMA
            if ((addr & 0xff80) == 0x4300)
            {  //$4300-$437f
                uint i = (addr >> 4) & 7;
                switch (addr & 0xf)
                {
                    case 0x0:
                        return mmio_r43x0((byte)i);
                    case 0x1:
                        return mmio_r43x1((byte)i);
                    case 0x2:
                        return mmio_r43x2((byte)i);
                    case 0x3:
                        return mmio_r43x3((byte)i);
                    case 0x4:
                        return mmio_r43x4((byte)i);
                    case 0x5:
                        return mmio_r43x5((byte)i);
                    case 0x6:
                        return mmio_r43x6((byte)i);
                    case 0x7:
                        return mmio_r43x7((byte)i);
                    case 0x8:
                        return mmio_r43x8((byte)i);
                    case 0x9:
                        return mmio_r43x9((byte)i);
                    case 0xa:
                        return mmio_r43xa((byte)i);
                    case 0xb:
                        return mmio_r43xb((byte)i);
                    case 0xc:
                        return regs.mdr;  //unmapped
                    case 0xd:
                        return regs.mdr;  //unmapped
                    case 0xe:
                        return regs.mdr;  //unmapped
                    case 0xf:
                        return mmio_r43xb((byte)i);  //mirror of $43xb
                }
            }

            switch (addr)
            {
                case 0x2180:
                    return mmio_r2180();
                case 0x4016:
                    return mmio_r4016();
                case 0x4017:
                    return mmio_r4017();
                case 0x4210:
                    return mmio_r4210();
                case 0x4211:
                    return mmio_r4211();
                case 0x4212:
                    return mmio_r4212();
                case 0x4213:
                    return mmio_r4213();
                case 0x4214:
                    return mmio_r4214();
                case 0x4215:
                    return mmio_r4215();
                case 0x4216:
                    return mmio_r4216();
                case 0x4217:
                    return mmio_r4217();
                case 0x4218:
                    return mmio_r4218();
                case 0x4219:
                    return mmio_r4219();
                case 0x421a:
                    return mmio_r421a();
                case 0x421b:
                    return mmio_r421b();
                case 0x421c:
                    return mmio_r421c();
                case 0x421d:
                    return mmio_r421d();
                case 0x421e:
                    return mmio_r421e();
                case 0x421f:
                    return mmio_r421f();
            }

            return regs.mdr;
        }

        public void mmio_write(uint addr, byte data)
        {
            addr &= 0xffff;

            //APU
            if ((addr & 0xffc0) == 0x2140)
            {  //$2140-$217f
                synchronize_smp();
                port_write(new uint2(addr), data);
                return;
            }

            //DMA
            if ((addr & 0xff80) == 0x4300)
            {  //$4300-$437f
                uint i = (addr >> 4) & 7;
                switch (addr & 0xf)
                {
                    case 0x0:
                        mmio_w43x0((byte)i, data);
                        return;
                    case 0x1:
                        mmio_w43x1((byte)i, data);
                        return;
                    case 0x2:
                        mmio_w43x2((byte)i, data);
                        return;
                    case 0x3:
                        mmio_w43x3((byte)i, data);
                        return;
                    case 0x4:
                        mmio_w43x4((byte)i, data);
                        return;
                    case 0x5:
                        mmio_w43x5((byte)i, data);
                        return;
                    case 0x6:
                        mmio_w43x6((byte)i, data);
                        return;
                    case 0x7:
                        mmio_w43x7((byte)i, data);
                        return;
                    case 0x8:
                        mmio_w43x8((byte)i, data);
                        return;
                    case 0x9:
                        mmio_w43x9((byte)i, data);
                        return;
                    case 0xa:
                        mmio_w43xa((byte)i, data);
                        return;
                    case 0xb:
                        mmio_w43xb((byte)i, data);
                        return;
                    case 0xc:
                        return;  //unmapped
                    case 0xd:
                        return;  //unmapped
                    case 0xe:
                        return;  //unmapped
                    case 0xf:
                        mmio_w43xb((byte)i, data);
                        return;  //mirror of $43xb
                }
            }

            switch (addr)
            {
                case 0x2180:
                    mmio_w2180(data);
                    return;
                case 0x2181:
                    mmio_w2181(data);
                    return;
                case 0x2182:
                    mmio_w2182(data);
                    return;
                case 0x2183:
                    mmio_w2183(data);
                    return;
                case 0x4016:
                    mmio_w4016(data);
                    return;
                case 0x4017:
                    return;  //unmapped
                case 0x4200:
                    mmio_w4200(data);
                    return;
                case 0x4201:
                    mmio_w4201(data);
                    return;
                case 0x4202:
                    mmio_w4202(data);
                    return;
                case 0x4203:
                    mmio_w4203(data);
                    return;
                case 0x4204:
                    mmio_w4204(data);
                    return;
                case 0x4205:
                    mmio_w4205(data);
                    return;
                case 0x4206:
                    mmio_w4206(data);
                    return;
                case 0x4207:
                    mmio_w4207(data);
                    return;
                case 0x4208:
                    mmio_w4208(data);
                    return;
                case 0x4209:
                    mmio_w4209(data);
                    return;
                case 0x420a:
                    mmio_w420a(data);
                    return;
                case 0x420b:
                    mmio_w420b(data);
                    return;
                case 0x420c:
                    mmio_w420c(data);
                    return;
                case 0x420d:
                    mmio_w420d(data);
                    return;
            }
        }

        private byte mmio_r2180()
        {
            return Bus.bus.read(new uint24(0x7e0000 | (uint)(status.wram_addr++)));
        }

        private byte mmio_r4016()
        {
            byte r = (byte)(regs.mdr & 0xfc);
            r |= (byte)(Input.input.port_read(Convert.ToBoolean(0)) & 3);
            return r;
        }

        private byte mmio_r4017()
        {
            byte r = (byte)((regs.mdr & 0xe0) | 0x1c);
            r |= (byte)(Input.input.port_read(Convert.ToBoolean(1)) & 3);
            return r;
        }

        private byte mmio_r4210()
        {
            byte r = (byte)(regs.mdr & 0x70);
            r |= (byte)(Convert.ToByte(rdnmi()) << 7);
            r |= (byte)(cpu_version & 0x0f);
            return r;
        }

        private byte mmio_r4211()
        {
            byte r = (byte)(regs.mdr & 0x7f);
            r |= (byte)(Convert.ToByte(timeup()) << 7);
            return r;
        }

        private byte mmio_r4212()
        {
            byte r = (byte)(regs.mdr & 0x3e);
            ushort vs = (ushort)(PPU.ppu.overscan() == false ? 225 : 240);
            if (PPUCounter.vcounter() >= vs && PPUCounter.vcounter() <= (vs + 2))
            {
                r |= 0x01;  //auto joypad polling
            }
            if (PPUCounter.hcounter() <= 2 || PPUCounter.hcounter() >= 1096)
            {
                r |= 0x40;  //hblank
            }
            if (PPUCounter.vcounter() >= vs)
            {
                r |= 0x80;  //vblank
            }
            return r;
        }

        private byte mmio_r4213()
        {
            return status.pio;
        }

        private byte mmio_r4214()
        {
            return (byte)(status.rddiv >> 0);
        }

        private byte mmio_r4215()
        {
            return (byte)(status.rddiv >> 8);
        }

        private byte mmio_r4216()
        {
            return (byte)(status.rdmpy >> 0);
        }

        private byte mmio_r4217()
        {
            return (byte)(status.rdmpy >> 8);
        }

        private byte mmio_r4218()
        {
            return status.joy1l;
        }

        private byte mmio_r4219()
        {
            return status.joy1h;
        }

        private byte mmio_r421a()
        {
            return status.joy2l;
        }

        private byte mmio_r421b()
        {
            return status.joy2h;
        }

        private byte mmio_r421c()
        {
            return status.joy3l;
        }

        private byte mmio_r421d()
        {
            return status.joy3h;
        }

        private byte mmio_r421e()
        {
            return status.joy4l;
        }

        private byte mmio_r421f()
        {
            return status.joy4h;
        }

        private byte mmio_r43x0(byte i)
        {
            return (byte)((Convert.ToUInt32(channel[i].direction) << 7)
                | (Convert.ToUInt32(channel[i].indirect) << 6)
                | (Convert.ToUInt32(channel[i].unused) << 5)
                | (Convert.ToUInt32(channel[i].reverse_transfer) << 4)
                | (Convert.ToUInt32(channel[i].fixed_transfer) << 3)
                | (uint)(channel[i].transfer_mode << 0));
        }

        private byte mmio_r43x1(byte i)
        {
            return channel[i].dest_addr;
        }

        private byte mmio_r43x2(byte i)
        {
            return (byte)(channel[i].source_addr >> 0);
        }

        private byte mmio_r43x3(byte i)
        {
            return (byte)(channel[i].source_addr >> 8);
        }

        private byte mmio_r43x4(byte i)
        {
            return channel[i].source_bank;
        }

        private byte mmio_r43x5(byte i)
        {
            return (byte)(channel[i].union.transfer_size >> 0);
        }

        private byte mmio_r43x6(byte i)
        {
            return (byte)(channel[i].union.transfer_size >> 8);
        }

        private byte mmio_r43x7(byte i)
        {
            return channel[i].indirect_bank;
        }

        private byte mmio_r43x8(byte i)
        {
            return (byte)(channel[i].hdma_addr >> 0);
        }

        private byte mmio_r43x9(byte i)
        {
            return (byte)(channel[i].hdma_addr >> 8);
        }

        private byte mmio_r43xa(byte i)
        {
            return channel[i].line_counter;
        }

        private byte mmio_r43xb(byte i)
        {
            return channel[i].unknown;
        }

        private void mmio_w2180(byte data)
        {
            Bus.bus.write(new uint24(0x7e0000 | (uint)(status.wram_addr++)), data);
        }

        private void mmio_w2181(byte data)
        {
            status.wram_addr.Assign((uint)((status.wram_addr & 0x01ff00) | (uint)(data << 0)));
        }

        private void mmio_w2182(byte data)
        {
            status.wram_addr.Assign((uint)((status.wram_addr & 0x0100ff) | (uint)(data << 8)));
        }

        private void mmio_w2183(byte data)
        {
            status.wram_addr.Assign((uint)((status.wram_addr & 0x00ffff) | (uint)(data << 16)));
        }

        private void mmio_w4016(byte data)
        {
            bool old_latch = status.joypad_strobe_latch;
            bool new_latch = Convert.ToBoolean(data & 1);
            status.joypad_strobe_latch = new_latch;
            if (old_latch != new_latch)
            {
                Input.input.poll();
            }
        }

        private void mmio_w4200(byte data)
        {
            status.auto_joypad_poll = Convert.ToBoolean(data & 1);
            nmitimen_update(data);
        }

        private void mmio_w4201(byte data)
        {
            if (Convert.ToBoolean(status.pio & 0x80) && !Convert.ToBoolean(data & 0x80))
            {
                PPU.ppu.latch_counters();
            }
            status.pio = data;
        }

        private void mmio_w4202(byte data)
        {
            status.wrmpya = data;
        }

        private void mmio_w4203(byte data)
        {
            status.rdmpy = 0;
            if (Convert.ToBoolean(alu.mpyctr) || Convert.ToBoolean(alu.divctr))
            {
                return;
            }

            status.wrmpyb = data;
            status.rddiv = (ushort)((status.wrmpyb << 8) | status.wrmpya);

            alu.mpyctr = 8;  //perform multiplication over the next eight cycles
            alu.shift = status.wrmpyb;
        }

        private void mmio_w4204(byte data)
        {
            status.wrdiva = (ushort)((status.wrdiva & 0xff00) | (data << 0));
        }

        private void mmio_w4205(byte data)
        {
            status.wrdiva = (ushort)((status.wrdiva & 0x00ff) | (data << 8));
        }

        private void mmio_w4206(byte data)
        {
            status.rdmpy = status.wrdiva;
            if (Convert.ToBoolean(alu.mpyctr) || Convert.ToBoolean(alu.divctr))
            {
                return;
            }

            status.wrdivb = data;

            alu.divctr = 16;  //perform division over the next sixteen cycles
            alu.shift = (uint)(status.wrdivb << 16);
        }

        private void mmio_w4207(byte data)
        {
            status.hirq_pos.Assign((uint)((status.hirq_pos & 0x0100) | (uint)(data << 0)));
        }

        private void mmio_w4208(byte data)
        {
            status.hirq_pos.Assign((uint)((status.hirq_pos & 0x00ff) | (uint)(data << 8)));
        }

        private void mmio_w4209(byte data)
        {
            status.virq_pos.Assign((uint)((status.virq_pos & 0x0100) | (uint)(data << 0)));
        }

        private void mmio_w420a(byte data)
        {
            status.virq_pos.Assign((uint)((status.virq_pos & 0x00ff) | (uint)(data << 8)));
        }

        private void mmio_w420b(byte data)
        {
            for (uint i = 0; i < 8; i++)
            {
                channel[i].dma_enabled = Convert.ToBoolean(data & (1 << (int)i));
            }
            if (Convert.ToBoolean(data))
            {
                status.dma_pending = true;
            }
        }

        private void mmio_w420c(byte data)
        {
            for (uint i = 0; i < 8; i++)
            {
                channel[i].hdma_enabled = Convert.ToBoolean(data & (1 << (int)i));
            }
        }

        private void mmio_w420d(byte data)
        {
            status.rom_speed = (Convert.ToBoolean(data & 1) ? 6U : 8U);
        }

        private void mmio_w43x0(byte i, byte data)
        {
            channel[i].direction = Convert.ToBoolean(data & 0x80);
            channel[i].indirect = Convert.ToBoolean(data & 0x40);
            channel[i].unused = Convert.ToBoolean(data & 0x20);
            channel[i].reverse_transfer = Convert.ToBoolean(data & 0x10);
            channel[i].fixed_transfer = Convert.ToBoolean(data & 0x08);
            channel[i].transfer_mode.Assign((uint)(data & 0x07));
        }

        private void mmio_w43x1(byte i, byte data)
        {
            channel[i].dest_addr = data;
        }

        private void mmio_w43x2(byte i, byte data)
        {
            channel[i].source_addr = (ushort)((channel[i].source_addr & 0xff00) | (data << 0));
        }

        private void mmio_w43x3(byte i, byte data)
        {
            channel[i].source_addr = (ushort)((channel[i].source_addr & 0x00ff) | (data << 8));
        }

        private void mmio_w43x4(byte i, byte data)
        {
            channel[i].source_bank = data;
        }

        private void mmio_w43x5(byte i, byte data)
        {
            channel[i].union.transfer_size = (ushort)((channel[i].union.transfer_size & 0xff00) | (data << 0));
        }

        private void mmio_w43x6(byte i, byte data)
        {
            channel[i].union.transfer_size = (ushort)((channel[i].union.transfer_size & 0x00ff) | (data << 8));
        }

        private void mmio_w43x7(byte i, byte data)
        {
            channel[i].indirect_bank = data;
        }

        private void mmio_w43x8(byte i, byte data)
        {
            channel[i].hdma_addr = (ushort)((channel[i].hdma_addr & 0xff00) | (data << 0));
        }

        private void mmio_w43x9(byte i, byte data)
        {
            channel[i].hdma_addr = (ushort)((channel[i].hdma_addr & 0x00ff) | (data << 8));
        }

        private void mmio_w43xa(byte i, byte data)
        {
            channel[i].line_counter = data;
        }

        private void mmio_w43xb(byte i, byte data)
        {
            channel[i].unknown = data;
        }

        private uint dma_counter()
        {
            return (status.dma_counter + PPUCounter.hcounter()) & 7;
        }

        private void add_clocks(uint clocks)
        {
            status.irq_lock = false;
            uint ticks = clocks >> 1;
            while (Convert.ToBoolean(ticks--))
            {
                PPUCounter.tick();
                if (Convert.ToBoolean(PPUCounter.hcounter() & 2))
                {
                    Input.input.tick();
                    poll_interrupts();
                }
            }

            step(clocks);

            if (status.dram_refreshed == false && PPUCounter.hcounter() >= status.dram_refresh_position)
            {
                status.dram_refreshed = true;
                add_clocks(40);
            }
        }

        private void scanline()
        {
            status.dma_counter = (status.dma_counter + status.line_clocks) & 7;
            status.line_clocks = PPUCounter.lineclocks();

            //forcefully sync S-CPU to other processors, in case chips are not communicating
            synchronize_ppu();
            synchronize_smp();
            synchronize_coprocessor();
            System.system.scanline();

            if (PPUCounter.vcounter() == 0)
            {
                //HDMA init triggers once every frame
                status.hdma_init_position = (cpu_version == 1 ? 12 + 8 - dma_counter() : 12 + dma_counter());
                status.hdma_init_triggered = false;
            }

            //DRAM refresh occurs once every scanline
            if (cpu_version == 2)
            {
                status.dram_refresh_position = 530 + 8 - dma_counter();
            }
            status.dram_refreshed = false;

            //HDMA triggers once every visible scanline
            if (PPUCounter.vcounter() <= (PPU.ppu.overscan() == false ? 224 : 239))
            {
                status.hdma_position = 1104;
                status.hdma_triggered = false;
            }

            if (status.auto_joypad_poll == true && PPUCounter.vcounter() == (PPU.ppu.overscan() == false ? 227 : 242))
            {
                Input.input.poll();
                run_auto_joypad_poll();
            }
        }

        private void alu_edge()
        {
            if (Convert.ToBoolean(alu.mpyctr))
            {
                alu.mpyctr--;
                if (Convert.ToBoolean(status.rddiv & 1))
                {
                    status.rdmpy += (ushort)alu.shift;
                }
                status.rddiv >>= 1;
                alu.shift <<= 1;
            }

            if (Convert.ToBoolean(alu.divctr))
            {
                alu.divctr--;
                status.rddiv <<= 1;
                alu.shift >>= 1;
                if (status.rdmpy >= alu.shift)
                {
                    status.rdmpy -= (ushort)alu.shift;
                    status.rddiv |= 1;
                }
            }
        }

        private void dma_edge()
        {	//H/DMA pending && DMA inactive?
            //.. Run one full CPU cycle
            //.. HDMA pending && HDMA enabled ? DMA sync + HDMA run
            //.. DMA pending && DMA enabled ? DMA sync + DMA run
            //.... HDMA during DMA && HDMA enabled ? DMA sync + HDMA run
            //.. Run one bus CPU cycle
            //.. CPU sync

            if (status.dma_active == true)
            {
                if (status.hdma_pending)
                {
                    status.hdma_pending = false;
                    if (Convert.ToBoolean(hdma_enabled_channels()))
                    {
                        if (!Convert.ToBoolean(dma_enabled_channels()))
                        {
                            dma_add_clocks(8 - dma_counter());
                        }
                        if (status.hdma_mode == Convert.ToBoolean(0))
                        {
                            hdma_init();
                        }
                        else
                        {
                            hdma_run();
                        }
                        if (!Convert.ToBoolean(dma_enabled_channels()))
                        {
                            add_clocks(status.clock_count - (status.dma_clocks % status.clock_count));
                            status.dma_active = false;
                        }
                    }
                }

                if (status.dma_pending)
                {
                    status.dma_pending = false;
                    if (Convert.ToBoolean(dma_enabled_channels()))
                    {
                        dma_add_clocks(8 - dma_counter());
                        dma_run();
                        add_clocks(status.clock_count - (status.dma_clocks % status.clock_count));
                        status.dma_active = false;
                    }
                }
            }

            if (status.hdma_init_triggered == false && PPUCounter.hcounter() >= status.hdma_init_position)
            {
                status.hdma_init_triggered = true;
                hdma_init_reset();
                if (Convert.ToBoolean(hdma_enabled_channels()))
                {
                    status.hdma_pending = true;
                    status.hdma_mode = Convert.ToBoolean(0);
                }
            }

            if (status.hdma_triggered == false && PPUCounter.hcounter() >= status.hdma_position)
            {
                status.hdma_triggered = true;
                if (Convert.ToBoolean(hdma_active_channels()))
                {
                    status.hdma_pending = true;
                    status.hdma_mode = Convert.ToBoolean(1);
                }
            }

            if (status.dma_active == false)
            {
                if (status.dma_pending || status.hdma_pending)
                {
                    status.dma_clocks = 0;
                    status.dma_active = true;
                }
            }
        }

        public override void last_cycle()
        {
            if (status.irq_lock == false)
            {
                status.nmi_pending |= nmi_test();
                status.irq_pending |= irq_test();
                status.interrupt_pending = (status.nmi_pending || status.irq_pending);
            }
        }

        private void timing_power() { }

        private void timing_reset()
        {
            status.clock_count = 0;
            status.line_clocks = PPUCounter.lineclocks();

            status.irq_lock = false;
            status.dram_refresh_position = (cpu_version == 1 ? 530U : 538U);
            status.dram_refreshed = false;

            status.hdma_init_position = (cpu_version == 1 ? 12 + 8 - dma_counter() : 12 + dma_counter());
            status.hdma_init_triggered = false;

            status.hdma_position = 1104;
            status.hdma_triggered = false;

            status.nmi_valid = false;
            status.nmi_line = false;
            status.nmi_transition = false;
            status.nmi_pending = false;
            status.nmi_hold = false;

            status.irq_valid = false;
            status.irq_line = false;
            status.irq_transition = false;
            status.irq_pending = false;
            status.irq_hold = false;

            status.reset_pending = true;
            status.interrupt_pending = true;
            status.interrupt_vector = 0xfffc;  //reset vector address

            status.dma_active = false;
            status.dma_counter = 0;
            status.dma_clocks = 0;
            status.dma_pending = false;
            status.hdma_pending = false;
            status.hdma_mode = Convert.ToBoolean(0);
        }

        private void poll_interrupts()
        { 	//NMI hold
            if (status.nmi_hold)
            {
                status.nmi_hold = false;
                if (status.nmi_enabled)
                {
                    status.nmi_transition = true;
                }
            }

            //NMI test
            bool nmi_valid = (PPUCounter.vcounter(2) >= (!PPU.ppu.overscan() ? 225 : 240));
            if (!status.nmi_valid && nmi_valid)
            {
                //0->1 edge sensitive transition
                status.nmi_line = true;
                status.nmi_hold = true;  //hold /NMI for four cycles
            }
            else if (status.nmi_valid && !nmi_valid)
            {
                //1->0 edge sensitive transition
                status.nmi_line = false;
            }
            status.nmi_valid = nmi_valid;

            //IRQ hold
            status.irq_hold = false;
            if (status.irq_line)
            {
                if (status.virq_enabled || status.hirq_enabled)
                {
                    status.irq_transition = true;
                }
            }

            //IRQ test
            bool irq_valid = (status.virq_enabled || status.hirq_enabled);
            if (irq_valid)
            {
                if ((status.virq_enabled && PPUCounter.vcounter(10) != (uint)(status.virq_pos))
                    || (status.hirq_enabled && PPUCounter.hcounter(10) != ((uint)status.hirq_pos + 1) * 4)
                    || (Convert.ToBoolean((uint)status.virq_pos) && PPUCounter.vcounter(6) == 0)  //IRQs cannot trigger on last dot of field
                    )
                {
                    irq_valid = false;
                }
            }
            if (!status.irq_valid && irq_valid)
            {
                //0->1 edge sensitive transition
                status.irq_line = true;
                status.irq_hold = true;  //hold /IRQ for four cycles
            }
            status.irq_valid = irq_valid;
        }

        private void nmitimen_update(byte data)
        {
            bool nmi_enabled = status.nmi_enabled;
            bool virq_enabled = status.virq_enabled;
            bool hirq_enabled = status.hirq_enabled;
            status.nmi_enabled = Convert.ToBoolean(data & 0x80);
            status.virq_enabled = Convert.ToBoolean(data & 0x20);
            status.hirq_enabled = Convert.ToBoolean(data & 0x10);

            //0->1 edge sensitive transition
            if (!nmi_enabled && status.nmi_enabled && status.nmi_line)
            {
                status.nmi_transition = true;
            }

            //?->1 level sensitive transition
            if (status.virq_enabled && !status.hirq_enabled && status.irq_line)
            {
                status.irq_transition = true;
            }

            if (!status.virq_enabled && !status.hirq_enabled)
            {
                status.irq_line = false;
                status.irq_transition = false;
            }

            status.irq_lock = true;
        }

        private bool rdnmi()
        {
            bool result = status.nmi_line;
            if (!status.nmi_hold)
            {
                status.nmi_line = false;
            }
            return result;
        }

        private bool timeup()
        {
            bool result = status.irq_line;
            if (!status.irq_hold)
            {
                status.irq_line = false;
                status.irq_transition = false;
            }
            return result;
        }

        private bool nmi_test()
        {
            if (!status.nmi_transition)
            {
                return false;
            }
            status.nmi_transition = false;
            regs.wai = false;
            return true;
        }

        private bool irq_test()
        {
            if (!status.irq_transition && !regs.irq)
            {
                return false;
            }
            status.irq_transition = false;
            regs.wai = false;
            return !regs.p.i;
        }

        private void run_auto_joypad_poll()
        {
            ushort joy1 = 0, joy2 = 0, joy3 = 0, joy4 = 0;
            for (uint i = 0; i < 16; i++)
            {
                byte port0 = Input.input.port_read(Convert.ToBoolean(0));
                byte port1 = Input.input.port_read(Convert.ToBoolean(1));

                joy1 |= (ushort)((Convert.ToBoolean(port0 & 1)) ? (0x8000 >> (int)i) : 0);
                joy2 |= (ushort)((Convert.ToBoolean(port1 & 1)) ? (0x8000 >> (int)i) : 0);
                joy3 |= (ushort)((Convert.ToBoolean(port0 & 2)) ? (0x8000 >> (int)i) : 0);
                joy4 |= (ushort)((Convert.ToBoolean(port1 & 2)) ? (0x8000 >> (int)i) : 0);
            }

            status.joy1l = (byte)(joy1 >> 0);
            status.joy1h = (byte)(joy1 >> 8);

            status.joy2l = (byte)(joy2 >> 0);
            status.joy2h = (byte)(joy2 >> 8);

            status.joy3l = (byte)(joy3 >> 0);
            status.joy3h = (byte)(joy3 >> 8);

            status.joy4l = (byte)(joy4 >> 0);
            status.joy4h = (byte)(joy4 >> 8);
        }

        private byte cpu_version;

        private Status status = new Status();
        private ALU alu = new ALU();

        private static void Enter()
        {
            cpu.enter();
        }

        private void op_irq()
        {
            op_read(regs.pc.d);
            op_io();
            if (!regs.e)
            {
                op_writestack(regs.pc.b);
            }
            op_writestack(regs.pc.h);
            op_writestack(regs.pc.l);
            op_writestack(regs.e ? (byte)((uint)regs.p & ~0x10) : (byte)regs.p);
            rd.l = op_read(status.interrupt_vector + 0U);
            regs.pc.b = 0x00;
            regs.p.i = Convert.ToBoolean(1);
            regs.p.d = Convert.ToBoolean(0);
            rd.h = op_read(status.interrupt_vector + 1U);
            regs.pc.w = rd.w;
        }

        private void op_step()
        {
            opcode_table.Array[opcode_table.Offset + op_readpc()].Invoke();
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