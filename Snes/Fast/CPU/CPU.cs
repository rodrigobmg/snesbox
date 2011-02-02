#if FAST_CPU
using System;
using System.Collections;
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

        public IEnumerable synchronize_smp()
        {
            if (SMP.smp.Processor.clock < 0)
            {
                yield return SMP.smp.Processor.thread;
            }
        }

        public IEnumerable synchronize_ppu()
        {
            if (PPU.ppu.Processor.clock < 0)
            {
                yield return PPU.ppu.Processor.thread;
            }
        }

        public IEnumerable synchronize_coprocessor()
        {
            for (uint i = 0; i < coprocessors.Count; i++)
            {
                IProcessor chip = coprocessors[(int)i];
                if (chip.Processor.clock < 0)
                {
                    yield return chip.Processor.thread;
                }
            }
        }

        public byte pio()
        {
            return status.pio;
        }

        public bool joylatch()
        {
            return Convert.ToBoolean(0);
        }

        public override bool interrupt_pending()
        {
            return false;
        }

        public byte port_read(byte port)
        {
            return port_data[port & 3];
        }

        public void port_write(byte port, byte data)
        {
            port_data[port & 3] = data;
        }

        public IEnumerable mmio_read(uint addr, Result result)
        {
            if ((addr & 0xffc0) == 0x2140)
            {
                foreach (var e in synchronize_smp())
                {
                    yield return e;
                };
                result.Value = SMP.smp.port_read(new uint2(addr & 3));
                yield break;
            }

            switch (addr & 0xffff)
            {
                case 0x2180:
                    {
                        foreach (var e in Bus.bus.read(new uint24(0x7e0000 | status.wram_addr), result))
                        {
                            yield return e;
                        };
                        status.wram_addr = (status.wram_addr + 1) & 0x01ffff;
                        yield break;
                    }
                case 0x4016:
                    {
                        result.Value = (byte)(regs.mdr & 0xfc);
                        result.Value |= (byte)(Input.input.port_read(Convert.ToBoolean(0)) & 3);
                        yield break;
                    }
                case 0x4017:
                    {
                        result.Value = (byte)((regs.mdr & 0xe0) | 0x1c);
                        result.Value |= (byte)(Input.input.port_read(Convert.ToBoolean(1)) & 3);
                        yield break;
                    }
                case 0x4210:
                    {
                        result.Value = (byte)(regs.mdr & 0x70);
                        result.Value |= (byte)(Convert.ToInt32(status.nmi_line) << 7);
                        result.Value |= 0x02;  //CPU revision
                        status.nmi_line = false;
                        yield break;
                    }
                case 0x4211:
                    {
                        result.Value = (byte)(regs.mdr & 0x7f);
                        result.Value |= (byte)(Convert.ToInt32(status.irq_line) << 7);
                        status.irq_line = false;
                        yield break;
                    }
                case 0x4212:
                    {
                        result.Value = (byte)(regs.mdr & 0x3e);
                        uint vbstart = PPU.ppu.overscan() == false ? 225U : 240U;

                        if (PPUCounter.vcounter() >= vbstart && PPUCounter.vcounter() <= vbstart + 2)
                        {
                            result.Value |= 0x01;
                        }
                        if (PPUCounter.hcounter() <= 2 || PPUCounter.hcounter() >= 1096)
                        {
                            result.Value |= 0x40;
                        }
                        if (PPUCounter.vcounter() >= vbstart)
                        {
                            result.Value |= 0x80;
                        }

                        yield break;
                    }
                case 0x4213:
                    result.Value = status.pio;
                    yield break;
                case 0x4214:
                    result.Value = (byte)(status.rddiv >> 0);
                    yield break;
                case 0x4215:
                    result.Value = (byte)(status.rddiv >> 8);
                    yield break;
                case 0x4216:
                    result.Value = (byte)(status.rdmpy >> 0);
                    yield break;
                case 0x4217:
                    result.Value = (byte)(status.rdmpy >> 8);
                    yield break;
                case 0x4218:
                    result.Value = status.joy1l;
                    yield break;
                case 0x4219:
                    result.Value = status.joy1h;
                    yield break;
                case 0x421a:
                    result.Value = status.joy2l;
                    yield break;
                case 0x421b:
                    result.Value = status.joy2h;
                    yield break;
                case 0x421c:
                    result.Value = status.joy3l;
                    yield break;
                case 0x421d:
                    result.Value = status.joy3h;
                    yield break;
                case 0x421e:
                    result.Value = status.joy4l;
                    yield break;
                case 0x421f:
                    result.Value = status.joy4h;
                    yield break;
            }

            if ((addr & 0xff80) == 0x4300)
            {
                uint i = (addr >> 4) & 7;
                switch (addr & 0xff8f)
                {
                    case 0x4300:
                        {
                            result.Value = (byte)((Convert.ToInt32(channel[i].direction) << 7)
                                 | (Convert.ToInt32(channel[i].indirect) << 6)
                                 | (Convert.ToInt32(channel[i].unused) << 5)
                                 | (Convert.ToInt32(channel[i].reverse_transfer) << 4)
                                 | (Convert.ToInt32(channel[i].fixed_transfer) << 3)
                                 | (channel[i].transfer_mode << 0));
                            yield break;
                        }

                    case 0x4301:
                        result.Value = channel[i].dest_addr;
                        yield break;
                    case 0x4302:
                        result.Value = (byte)(channel[i].source_addr >> 0);
                        yield break;
                    case 0x4303:
                        result.Value = (byte)(channel[i].source_addr >> 8);
                        yield break;
                    case 0x4304:
                        result.Value = channel[i].source_bank;
                        yield break;
                    case 0x4305:
                        result.Value = (byte)(channel[i].union.transfer_size >> 0);
                        yield break;
                    case 0x4306:
                        result.Value = (byte)(channel[i].union.transfer_size >> 8);
                        yield break;
                    case 0x4307:
                        result.Value = channel[i].indirect_bank;
                        yield break;
                    case 0x4308:
                        result.Value = (byte)(channel[i].hdma_addr >> 0);
                        yield break;
                    case 0x4309:
                        result.Value = (byte)(channel[i].hdma_addr >> 8);
                        yield break;
                    case 0x430a:
                        result.Value = channel[i].line_counter;
                        yield break;
                    case 0x430b:
                    case 0x430f:
                        result.Value = channel[i].unknown;
                        yield break;
                }
            }

            result.Value = regs.mdr;
        }

        public IEnumerable mmio_write(uint addr, byte data)
        {
            if ((addr & 0xffc0) == 0x2140)
            {
                foreach (var e in synchronize_smp())
                {
                    yield return e;
                };
                port_write((byte)(addr & 3), data);
                yield break;
            }

            switch (addr & 0xffff)
            {
                case 0x2180:
                    {
                        foreach (var e in Bus.bus.write(new uint24(0x7e0000 | status.wram_addr), data))
                        {
                            yield return e;
                        };
                        status.wram_addr = (status.wram_addr + 1) & 0x01ffff;
                        yield break;
                    }
                case 0x2181:
                    {
                        status.wram_addr = (status.wram_addr & 0x01ff00) | (uint)(data << 0);
                        yield break;
                    }
                case 0x2182:
                    {
                        status.wram_addr = (status.wram_addr & 0x0100ff) | (uint)(data << 8);
                        yield break;
                    }
                case 0x2183:
                    {
                        status.wram_addr = (status.wram_addr & 0x00ffff) | (uint)((data & 1) << 16);
                        yield break;
                    }
                case 0x4016:
                    {
                        bool old_latch = status.joypad_strobe_latch;
                        bool new_latch = Convert.ToBoolean(data & 1);
                        status.joypad_strobe_latch = new_latch;
                        if (old_latch != new_latch)
                        {
                            Input.input.poll();
                        }
                        yield break;
                    }
                case 0x4200:
                    {
                        bool nmi_enabled = status.nmi_enabled;
                        bool virq_enabled = status.virq_enabled;
                        bool hirq_enabled = status.hirq_enabled;

                        status.nmi_enabled = Convert.ToBoolean(data & 0x80);
                        status.virq_enabled = Convert.ToBoolean(data & 0x20);
                        status.hirq_enabled = Convert.ToBoolean(data & 0x10);
                        status.auto_joypad_poll_enabled = Convert.ToBoolean(data & 0x01);

                        if (!nmi_enabled && status.nmi_enabled && status.nmi_line)
                        {
                            status.nmi_transition = true;
                        }

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
                        yield break;
                    }
                case 0x4201:
                    {
                        if (Convert.ToBoolean(status.pio & 0x80) && !Convert.ToBoolean(data & 0x80))
                        {
                            PPU.ppu.latch_counters();
                        }
                        status.pio = data;
                        goto case 0x4202;
                    }
                case 0x4202:
                    {
                        status.wrmpya = data;
                        yield break;
                    }
                case 0x4203:
                    {
                        status.wrmpyb = data;
                        status.rdmpy = (ushort)(status.wrmpya * status.wrmpyb);
                        yield break;
                    }
                case 0x4204:
                    {
                        status.wrdiva = (ushort)((status.wrdiva & 0xff00) | (data << 0));
                        yield break;
                    }
                case 0x4205:
                    {
                        status.wrdiva = (ushort)((data << 8) | (status.wrdiva & 0x00ff));
                        yield break;
                    }
                case 0x4206:
                    {
                        status.wrdivb = data;
                        status.rddiv = (ushort)(Convert.ToBoolean(status.wrdivb) ? status.wrdiva / status.wrdivb : 0xffff);
                        status.rdmpy = (ushort)(Convert.ToBoolean(status.wrdivb) ? status.wrdiva % status.wrdivb : status.wrdiva);
                        yield break;
                    }
                case 0x4207:
                    {
                        status.htime = (ushort)((status.htime & 0x0100) | (data << 0));
                        yield break;
                    }
                case 0x4208:
                    {
                        status.htime = (ushort)(((data & 1) << 8) | (status.htime & 0x00ff));
                        yield break;
                    }
                case 0x4209:
                    {
                        status.vtime = (ushort)((status.vtime & 0x0100) | (data << 0));
                        yield break;
                    }
                case 0x420a:
                    {
                        status.vtime = (ushort)(((data & 1) << 8) | (status.vtime & 0x00ff));
                        yield break;
                    }
                case 0x420b:
                    {
                        for (uint i = 0; i < 8; i++)
                        {
                            channel[i].dma_enabled = Convert.ToBoolean(data & (1 << (int)i));
                        }
                        if (Convert.ToBoolean(data))
                        {
                            foreach (var e in dma_run())
                            {
                                yield return e;
                            };
                        }
                        yield break;
                    }
                case 0x420c:
                    {
                        for (uint i = 0; i < 8; i++)
                        {
                            channel[i].hdma_enabled = Convert.ToBoolean(data & (1 << (int)i));
                        }
                        yield break;
                    }
                case 0x420d:
                    {
                        status.rom_speed = Convert.ToBoolean(data & 1) ? 6U : 8U;
                        yield break;
                    }
            }

            if ((addr & 0xff80) == 0x4300)
            {
                uint i = (addr >> 4) & 7;
                switch (addr & 0xff8f)
                {
                    case 0x4300:
                        {
                            channel[i].direction = Convert.ToBoolean(data & 0x80);
                            channel[i].indirect = Convert.ToBoolean(data & 0x40);
                            channel[i].unused = Convert.ToBoolean(data & 0x20);
                            channel[i].reverse_transfer = Convert.ToBoolean(data & 0x10);
                            channel[i].fixed_transfer = Convert.ToBoolean(data & 0x08);
                            channel[i].transfer_mode = (byte)(data & 0x07);
                            yield break;
                        }
                    case 0x4301:
                        {
                            channel[i].dest_addr = data;
                            yield break;
                        }
                    case 0x4302:
                        {
                            channel[i].source_addr = (ushort)((channel[i].source_addr & 0xff00) | (data << 0));
                            yield break;
                        }
                    case 0x4303:
                        {
                            channel[i].source_addr = (ushort)((data << 8) | (channel[i].source_addr & 0x00ff));
                            yield break;
                        }
                    case 0x4304:
                        {
                            channel[i].source_bank = data;
                            yield break;
                        }
                    case 0x4305:
                        {
                            channel[i].union.transfer_size = (ushort)((channel[i].union.transfer_size & 0xff00) | (data << 0));
                            yield break;
                        }
                    case 0x4306:
                        {
                            channel[i].union.transfer_size = (ushort)((data << 8) | (channel[i].union.transfer_size & 0x00ff));
                            yield break;
                        }
                    case 0x4307:
                        {
                            channel[i].indirect_bank = data;
                            yield break;
                        }
                    case 0x4308:
                        {
                            channel[i].hdma_addr = (ushort)((channel[i].hdma_addr & 0xff00) | (data << 0));
                            yield break;
                        }
                    case 0x4309:
                        {
                            channel[i].hdma_addr = (ushort)((data << 8) | (channel[i].hdma_addr & 0x00ff));
                            yield break;
                        }
                    case 0x430a:
                        {
                            channel[i].line_counter = data;
                            yield break;
                        }
                    case 0x430b:
                    case 0x430f:
                        {
                            channel[i].unknown = data;
                            yield break;
                        }
                }
            }
        }

        public override IEnumerable op_io()
        {
            foreach (var e in add_clocks(6))
            {
                yield return e;
            };
        }

        public override IEnumerable op_read(uint addr, Result result)
        {
            foreach (var e in Bus.bus.read(new uint24(addr), result))
            {
                yield return e;
            };
            regs.mdr = result.Value;
            foreach (var e in add_clocks(speed(addr)))
            {
                yield return e;
            };
            result.Value = regs.mdr;
        }

        public override IEnumerable op_write(uint addr, byte data)
        {
            foreach (var e in add_clocks(speed(addr)))
            {
                yield return e;
            };
            foreach (var e in Bus.bus.write(new uint24(addr), regs.mdr = data))
            {
                yield return e;
            };
        }

        public IEnumerable enter()
        {
            while (true)
            {
                if (Scheduler.scheduler.sync == Scheduler.SynchronizeMode.CPU)
                {
                    Scheduler.scheduler.sync = Scheduler.SynchronizeMode.All;
                    yield return Scheduler.ExitReason.SynchronizeEvent;
                }

                if (status.nmi_pending)
                {
                    status.nmi_pending = false;
                    foreach (var e in op_irq((ushort)(regs.e == false ? 0xffea : 0xfffa)))
                    {
                        yield return e;
                    };
                }

                if (status.irq_pending)
                {
                    status.irq_pending = false;
                    foreach (var e in op_irq((ushort)(regs.e == false ? 0xffee : 0xfffe)))
                    {
                        yield return e;
                    };
                }

                foreach (var e in op_step())
                {
                    yield return e;
                };
            }
        }

        public IEnumerable power()
        {
            regs.a.Assign(0x0000);
            regs.x.Assign(0x0000);
            regs.y.Assign(0x0000);
            regs.s.Assign(0x01ff);

            foreach (var e in reset())
            {
                yield return e;
            };
        }

        public IEnumerable reset()
        {
            Processor.create(enter(), System.system.cpu_frequency);
            coprocessors.Clear();
            PPUCounter.reset();

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

            foreach (var e in Bus.bus.read(new uint24(0xfffc), _result))
            {
                yield return e;
            };
            regs.pc.l = _result.Value;
            foreach (var e in Bus.bus.read(new uint24(0xfffd), _result))
            {
                yield return e;
            };
            regs.pc.h = _result.Value;
            regs.pc.b = 0x00;

            status.nmi_valid = false;
            status.nmi_line = false;
            status.nmi_transition = false;
            status.nmi_pending = false;

            status.irq_valid = false;
            status.irq_line = false;
            status.irq_transition = false;
            status.irq_pending = false;

            status.irq_lock = false;
            status.hdma_pending = false;

            status.wram_addr = 0x000000;

            status.joypad_strobe_latch = Convert.ToBoolean(0);

            status.nmi_enabled = false;
            status.virq_enabled = false;
            status.hirq_enabled = false;
            status.auto_joypad_poll_enabled = false;

            status.pio = 0xff;

            status.htime = 0x0000;
            status.vtime = 0x0000;

            status.rom_speed = 8;

            status.joy1l = status.joy1h = 0x00;
            status.joy2l = status.joy2h = 0x00;
            status.joy3l = status.joy3h = 0x00;
            status.joy4l = status.joy4h = 0x00;

            dma_reset();
        }

        public void serialize(Serializer s)
        {
            Processor.serialize(s);
            base.core_serialize(s);
            PPUCounter.serialize(s);

            queue.serialize(s);
            s.array(port_data, "port_data");

            for (uint i = 0; i < 8; i++)
            {
                s.integer(channel[i].dma_enabled, "channel[i].dma_enabled");
                s.integer(channel[i].hdma_enabled, "channel[i].hdma_enabled");

                s.integer(channel[i].direction, "channel[i].direction");
                s.integer(channel[i].indirect, "channel[i].indirect");
                s.integer(channel[i].unused, "channel[i].unused");
                s.integer(channel[i].reverse_transfer, "channel[i].reverse_transfer");
                s.integer(channel[i].fixed_transfer, "channel[i].fixed_transfer");
                s.integer(channel[i].transfer_mode, "channel[i].transfer_mode");

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

            s.integer(status.nmi_valid, "status.nmi_valid");
            s.integer(status.nmi_line, "status.nmi_line");
            s.integer(status.nmi_transition, "status.nmi_transition");
            s.integer(status.nmi_pending, "status.nmi_pending");

            s.integer(status.irq_valid, "status.irq_valid");
            s.integer(status.irq_line, "status.irq_line");
            s.integer(status.irq_transition, "status.irq_transition");
            s.integer(status.irq_pending, "status.irq_pending");

            s.integer(status.irq_lock, "status.irq_lock");
            s.integer(status.hdma_pending, "status.hdma_pending");

            s.integer(status.wram_addr, "status.wram_addr");

            s.integer(status.joypad_strobe_latch, "status.joypad_strobe_latch");

            s.integer(status.nmi_enabled, "status.nmi_enabled");
            s.integer(status.virq_enabled, "status.virq_enabled");
            s.integer(status.hirq_enabled, "status.hirq_enabled");
            s.integer(status.auto_joypad_poll_enabled, "status.auto_joypad_poll_enabled");

            s.integer(status.pio, "status.pio");

            s.integer(status.wrmpya, "status.wrmpya");
            s.integer(status.wrmpyb, "status.wrmpyb");
            s.integer(status.wrdiva, "status.wrdiva");
            s.integer(status.wrdivb, "status.wrdivb");

            s.integer(status.htime, "status.htime");
            s.integer(status.vtime, "status.vtime");

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
        }

        public CPU()
        {
            Utility.InstantiateArrayElements(channel);

            queue = new PriorityQueue(512, this.queue_event);
            PPUCounter.Scanline = this.scanline;
        }

        private IEnumerable op_step()
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            foreach (var e in opcode_table.Array[opcode_table.Offset + _result.Value].Invoke())
            {
                yield return e;
            };
        }

        private IEnumerable op_irq(ushort vector)
        {
            foreach (var e in op_read(regs.pc.d, _result))
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            if (!regs.e)
            {
                foreach (var e in op_writestack(regs.pc.b))
                {
                    yield return e;
                };
            }
            foreach (var e in op_writestack(regs.pc.h))
            {
                yield return e;
            };
            foreach (var e in op_writestack(regs.pc.l))
            {
                yield return e;
            };
            foreach (var e in op_writestack(regs.e ? (byte)((uint)regs.p & ~0x10) : (byte)regs.p))
            {
                yield return e;
            };
            foreach (var e in op_read(vector + 0U, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            regs.pc.b = 0x00;
            regs.p.i = Convert.ToBoolean(1);
            regs.p.d = Convert.ToBoolean(0);
            foreach (var e in op_read(vector + 1U, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            regs.pc.w = rd.w;
        }

        //timing
        private enum QueueEvent : uint { DramRefresh, HdmaRun, ControllerLatch, }

        private PriorityQueue queue;

        private IEnumerable queue_event(uint id)
        {
            switch ((QueueEvent)id)
            {
                case QueueEvent.DramRefresh:
                    {
                        foreach (var e in add_clocks(40))
                        {
                            yield return e;
                        };
                        yield break;
                    }
                case QueueEvent.HdmaRun:
                    {
                        foreach (var e in hdma_run())
                        {
                            yield return e;
                        };
                        yield break;
                    }
                case QueueEvent.ControllerLatch:
                    {
                        PPU.ppu.latch_counters();
                        yield break;
                    }
            }
        }

        public override void last_cycle()
        {
            if (status.irq_lock)
            {
                status.irq_lock = false;
                return;
            }

            if (status.nmi_transition)
            {
                regs.wai = false;
                status.nmi_transition = false;
                status.nmi_pending = true;
            }

            if (status.irq_transition || regs.irq)
            {
                regs.wai = false;
                status.irq_transition = false;
                status.irq_pending = !regs.p.i;
            }
        }

        private IEnumerable add_clocks(uint clocks)
        {
            if (status.hirq_enabled)
            {
                if (status.virq_enabled)
                {
                    uint cpu_time = PPUCounter.vcounter() * 1364U + PPUCounter.hcounter();
                    uint irq_time = status.vtime * 1364U + status.htime * 4U;
                    uint framelines = (System.system.region == System.Region.NTSC ? 262U : 312U) + Convert.ToUInt32(PPUCounter.field());
                    if (cpu_time > irq_time)
                    {
                        irq_time += framelines * 1364;
                    }
                    bool irq_valid = status.irq_valid;
                    status.irq_valid = cpu_time <= irq_time && cpu_time + clocks > irq_time;
                    if (!irq_valid && status.irq_valid)
                    {
                        status.irq_line = true;
                    }
                }
                else
                {
                    uint irq_time = status.htime * 4U;
                    if (PPUCounter.hcounter() > irq_time)
                    {
                        irq_time += 1364;
                    }
                    bool irq_valid = status.irq_valid;
                    status.irq_valid = PPUCounter.hcounter() <= irq_time && PPUCounter.hcounter() + clocks > irq_time;
                    if (!irq_valid && status.irq_valid)
                    {
                        status.irq_line = true;
                    }
                }
                if (status.irq_line)
                {
                    status.irq_transition = true;
                }
            }
            else if (status.virq_enabled)
            {
                bool irq_valid = status.irq_valid;
                status.irq_valid = PPUCounter.vcounter() == status.vtime;
                if (!irq_valid && status.irq_valid)
                {
                    status.irq_line = true;
                }
                if (status.irq_line)
                {
                    status.irq_transition = true;
                }
            }
            else
            {
                status.irq_valid = false;
            }

            foreach (var e in PPUCounter.tick(clocks))
            {
                yield return e;
            };
            foreach (var e in queue.tick(clocks))
            {
                yield return e;
            };
            step(clocks);
        }

        private IEnumerable scanline()
        {
            foreach (var e in synchronize_smp())
            {
                yield return e;
            };
            foreach (var e in synchronize_ppu())
            {
                yield return e;
            };
            foreach (var e in synchronize_coprocessor())
            {
                yield return e;
            };
            foreach (var e in System.system.scanline())
            {
                yield return e;
            };

            if (PPUCounter.vcounter() == 0)
            {
                foreach (var e in hdma_init())
                {
                    yield return e;
                };
            }

            queue.enqueue(534, (uint)QueueEvent.DramRefresh);

            if (PPUCounter.vcounter() <= (PPU.ppu.overscan() == false ? 224 : 239))
            {
                queue.enqueue(1104 + 8, (uint)QueueEvent.HdmaRun);
            }

            if (PPUCounter.vcounter() == Input.input.latchy)
            {
                queue.enqueue((uint)Input.input.latchx, (uint)QueueEvent.ControllerLatch);
            }

            bool nmi_valid = status.nmi_valid;
            status.nmi_valid = PPUCounter.vcounter() >= (PPU.ppu.overscan() == false ? 225 : 240);
            if (!nmi_valid && status.nmi_valid)
            {
                status.nmi_line = true;
                if (status.nmi_enabled)
                {
                    status.nmi_transition = true;
                }
            }
            else if (nmi_valid && !status.nmi_valid)
            {
                status.nmi_line = false;
            }

            if (status.auto_joypad_poll_enabled && PPUCounter.vcounter() == (PPU.ppu.overscan() == false ? 227 : 242))
            {
                Input.input.poll();
                run_auto_joypad_poll();
            }
        }

        private void run_auto_joypad_poll()
        {
            ushort joy1 = 0, joy2 = 0, joy3 = 0, joy4 = 0;
            for (uint i = 0; i < 16; i++)
            {
                byte port0 = Input.input.port_read(Convert.ToBoolean(0));
                byte port1 = Input.input.port_read(Convert.ToBoolean(1));

                joy1 |= (ushort)(Convert.ToBoolean(port0 & 1) ? (0x8000 >> (int)i) : 0);
                joy2 |= (ushort)(Convert.ToBoolean(port1 & 1) ? (0x8000 >> (int)i) : 0);
                joy3 |= (ushort)(Convert.ToBoolean(port0 & 2) ? (0x8000 >> (int)i) : 0);
                joy4 |= (ushort)(Convert.ToBoolean(port1 & 2) ? (0x8000 >> (int)i) : 0);
            }

            status.joy1l = (byte)joy1;
            status.joy1h = (byte)(joy1 >> 8);

            status.joy2l = (byte)joy2;
            status.joy2h = (byte)(joy2 >> 8);

            status.joy3l = (byte)joy3;
            status.joy3h = (byte)(joy3 >> 8);

            status.joy4l = (byte)joy4;
            status.joy4h = (byte)(joy4 >> 8);
        }

        //memory
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

        //dma
        private bool dma_transfer_valid(byte bbus, uint abus)
        {   //transfers from WRAM to WRAM are invalid; chip only has one address bus
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
                return false;  //$[00-3f|80-bf]:[4000-41ff]
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

        private IEnumerable dma_read(uint abus, Result result)
        {
            if (dma_addr_valid(abus) == false)
            {
                result.Value = 0x00;
                yield break;
            }
            foreach (var e in Bus.bus.read(new uint24(abus), result))
            {
                yield return e;
            };
        }

        private IEnumerable dma_write(bool valid, uint addr, byte data)
        {
            if (valid)
            {
                foreach (var e in Bus.bus.write(new uint24(addr), data))
                {
                    yield return e;
                };
            }
        }

        private IEnumerable dma_transfer(bool direction, byte bbus, uint abus)
        {
            if (Convert.ToInt32(direction) == 0)
            {
                foreach (var e in dma_read(abus, _result))
                {
                    yield return e;
                };
                foreach (var e in add_clocks(8))
                {
                    yield return e;
                };
                foreach (var e in dma_write(dma_transfer_valid(bbus, abus), (uint)(0x2100 | bbus), _result.Value))
                {
                    yield return e;
                };
            }
            else
            {
                if (dma_transfer_valid(bbus, abus))
                {
                    foreach (var e in Bus.bus.read(new uint24((uint)(0x2100 | bbus)), _result))
                    {
                        yield return e;
                    };
                }
                else
                {
                    _result.Value = (byte)0x00;
                }
                foreach (var e in add_clocks(8))
                {
                    yield return e;
                };
                foreach (var e in dma_write(dma_addr_valid(abus), abus, _result.Value))
                {
                    yield return e;
                };
            }
        }

        private byte dma_bbus(uint i, uint index)
        {
            switch (channel[i].transfer_mode)
            {
                default:
                case 0:
                    return (channel[i].dest_addr);                       //0
                case 1:
                    return (byte)(channel[i].dest_addr + (index & 1));         //0,1
                case 2:
                    return (channel[i].dest_addr);                       //0,0
                case 3:
                    return (byte)(channel[i].dest_addr + ((index >> 1) & 1));  //0,0,1,1
                case 4:
                    return (byte)(channel[i].dest_addr + (index & 3));         //0,1,2,3
                case 5:
                    return (byte)(channel[i].dest_addr + (index & 1));         //0,1,0,1
                case 6:
                    return (channel[i].dest_addr);                       //0,0     [2]
                case 7:
                    return (byte)(channel[i].dest_addr + ((index >> 1) & 1));  //0,0,1,1 [3]
            }
        }

        private uint dma_addr(uint i)
        {
            uint result = (uint)((channel[i].source_bank << 16) | (channel[i].source_addr));

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

            return result;
        }

        private uint hdma_addr(uint i)
        {
            return (uint)((channel[i].source_bank << 16) | (channel[i].hdma_addr++));
        }

        private uint hdma_iaddr(uint i)
        {
            return (uint)((channel[i].indirect_bank << 16) | (channel[i].union.indirect_addr++));
        }

        private IEnumerable dma_run()
        {
            foreach (var e in add_clocks(16))
            {
                yield return e;
            };

            for (uint i = 0; i < 8; i++)
            {
                if (channel[i].dma_enabled == false)
                {
                    continue;
                }
                foreach (var e in add_clocks(8))
                {
                    yield return e;
                };

                uint index = 0;
                do
                {
                    foreach (var e in dma_transfer(channel[i].direction, dma_bbus(i, index++), dma_addr(i)))
                    {
                        yield return e;
                    };
                }
                while (channel[i].dma_enabled && Convert.ToBoolean(--channel[i].union.transfer_size));

                channel[i].dma_enabled = false;
            }

            status.irq_lock = true;
        }

        private bool hdma_active_after(uint i)
        {
            for (uint n = i + 1; i < 8; i++)
            {
                if (channel[i].hdma_enabled && !channel[i].hdma_completed)
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerable hdma_update(uint i)
        {
            if ((channel[i].line_counter & 0x7f) == 0)
            {
                foreach (var e in dma_read(hdma_addr(i), _result))
                {
                    yield return e;
                };
                channel[i].line_counter = _result.Value;
                channel[i].hdma_completed = (channel[i].line_counter == 0);
                channel[i].hdma_do_transfer = !channel[i].hdma_completed;
                foreach (var e in add_clocks(8))
                {
                    yield return e;
                };

                if (channel[i].indirect)
                {
                    foreach (var e in dma_read(hdma_addr(i), _result))
                    {
                        yield return e;
                    };
                    channel[i].union.indirect_addr = (ushort)(_result.Value << 8);
                    foreach (var e in add_clocks(8))
                    {
                        yield return e;
                    };

                    //emulating this glitch causes a slight slowdown; only enable if needed
                    //if(!channel[i].hdma_completed || hdma_active_after(i)) {
                    channel[i].union.indirect_addr >>= 8;
                    foreach (var e in dma_read(hdma_addr(i), _result))
                    {
                        yield return e;
                    };
                    channel[i].union.indirect_addr |= (ushort)(_result.Value << 8);
                    foreach (var e in add_clocks(8))
                    {
                        yield return e;
                    };
                    //}
                }
            }
        }

        private static readonly uint[] transfer_length = { 1, 2, 2, 4, 4, 4, 2, 4 };
        private IEnumerable hdma_run()
        {
            uint channels = 0;
            for (uint i = 0; i < 8; i++)
            {
                if (channel[i].hdma_enabled)
                {
                    channels++;
                }
            }
            if (channels == 0)
            {
                yield break;
            }

            foreach (var e in add_clocks(16))
            {
                yield return e;
            };
            for (uint i = 0; i < 8; i++)
            {
                if (channel[i].hdma_enabled == false || channel[i].hdma_completed == true)
                {
                    continue;
                }
                channel[i].dma_enabled = false;

                if (channel[i].hdma_do_transfer)
                {
                    uint length = transfer_length[channel[i].transfer_mode];
                    for (uint index = 0; index < length; index++)
                    {
                        uint addr = channel[i].indirect == false ? hdma_addr(i) : hdma_iaddr(i);
                        foreach (var e in dma_transfer(channel[i].direction, dma_bbus(i, index), addr))
                        {
                            yield return e;
                        };
                    }
                }
            }

            for (uint i = 0; i < 8; i++)
            {
                if (channel[i].hdma_enabled == false || channel[i].hdma_completed == true)
                {
                    continue;
                }

                channel[i].line_counter--;
                channel[i].hdma_do_transfer = Convert.ToBoolean(channel[i].line_counter & 0x80);
                foreach (var e in hdma_update(i))
                {
                    yield return e;
                };
            }

            status.irq_lock = true;
        }

        private IEnumerable hdma_init()
        {
            uint channels = 0;
            for (uint i = 0; i < 8; i++)
            {
                channel[i].hdma_completed = false;
                channel[i].hdma_do_transfer = false;
                if (channel[i].hdma_enabled)
                {
                    channels++;
                }
            }
            if (channels == 0)
            {
                yield break;
            }

            foreach (var e in add_clocks(16))
            {
                yield return e;
            };
            for (uint i = 0; i < 8; i++)
            {
                if (!channel[i].hdma_enabled)
                {
                    continue;
                }
                channel[i].dma_enabled = false;

                channel[i].hdma_addr = channel[i].source_addr;
                channel[i].line_counter = 0;
                foreach (var e in hdma_update(i))
                {
                    yield return e;
                };
            }

            status.irq_lock = true;
        }

        private void dma_reset()
        {
            for (uint i = 0; i < 8; i++)
            {
                channel[i].dma_enabled = false;
                channel[i].hdma_enabled = false;

                channel[i].direction = Convert.ToBoolean(1);
                channel[i].indirect = true;
                channel[i].unused = true;
                channel[i].reverse_transfer = true;
                channel[i].fixed_transfer = true;
                channel[i].transfer_mode = 0x07;

                channel[i].dest_addr = 0xff;
                channel[i].source_addr = 0xffff;
                channel[i].source_bank = 0xff;

                channel[i].union.transfer_size = 0xffff;
                channel[i].union.indirect_addr = 0xffff;

                channel[i].indirect_bank = 0xff;
                channel[i].hdma_addr = 0xff;
                channel[i].line_counter = 0xff;
                channel[i].unknown = 0xff;

                channel[i].hdma_completed = false;
                channel[i].hdma_do_transfer = false;
            }
        }

        //registers
        private byte[] port_data = new byte[4];

        private Channel[] channel = new Channel[8];

        private Status status = new Status();

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
