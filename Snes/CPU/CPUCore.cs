﻿using System;
using System.Collections;
using Nall;

namespace Snes
{
    abstract partial class CPUCore
    {
        public IEnumerable op_readpc(Result result)
        {
            foreach (var e in op_read((uint)((regs.pc.b << 16) + regs.pc.w++), result))
            {
                yield return e;
            };
        }

        public IEnumerable op_readstack(Result result)
        {
            if (regs.e)
            {
                regs.s.l++;
            }
            else
            {
                regs.s.w++;
            }
            foreach (var e in op_read(regs.s.w, result))
            {
                yield return e;
            };
        }

        public IEnumerable op_readstackn(Result result)
        {
            foreach (var e in op_read(++regs.s.w, result))
            {
                yield return e;
            };
        }

        public IEnumerable op_readaddr(uint addr, Result result)
        {
            foreach (var e in op_read(addr & 0xffff, result))
            {
                yield return e;
            };
        }

        public IEnumerable op_readlong(uint addr, Result result)
        {
            foreach (var e in op_read(addr & 0xffffff, result))
            {
                yield return e;
            };
        }

        public IEnumerable op_readdbr(uint addr, Result result)
        {
            foreach (var e in op_read((uint)(((regs.db << 16) + addr) & 0xffffff), result))
            {
                yield return e;
            };
        }

        public IEnumerable op_readpbr(uint addr, Result result)
        {
            foreach (var e in op_read((uint)((regs.pc.b << 16) + (addr & 0xffff)), result))
            {
                yield return e;
            };
        }

        public IEnumerable op_readdp(uint addr, Result result)
        {
            if (regs.e && regs.d.l == 0x00)
            {
                foreach (var e in op_read((regs.d & 0xff00) + ((regs.d + (addr & 0xffff)) & 0xff), result))
                {
                    yield return e;
                };
            }
            else
            {
                foreach (var e in op_read((regs.d + (addr & 0xffff)) & 0xffff, result))
                {
                    yield return e;
                };
            }
        }

        public IEnumerable op_readsp(uint addr, Result result)
        {
            foreach (var e in op_read((regs.s + (addr & 0xffff)) & 0xffff, result))
            {
                yield return e;
            };
        }

        public IEnumerable op_writestack(byte data)
        {
            foreach (var e in op_write(regs.s.w, data))
            {
                yield return e;
            };
            if (regs.e)
            {
                regs.s.l--;
            }
            else
            {
                regs.s.w--;
            }

        }

        public IEnumerable op_writestackn(byte data)
        {
            foreach (var e in op_write(regs.s.w--, data))
            {
                yield return e;
            };
        }

        public IEnumerable op_writeaddr(uint addr, byte data)
        {
            foreach (var e in op_write(addr & 0xffff, data))
            {
                yield return e;
            };
        }

        public IEnumerable op_writelong(uint addr, byte data)
        {
            foreach (var e in op_write(addr & 0xffffff, data))
            {
                yield return e;
            };
        }

        public IEnumerable op_writedbr(uint addr, byte data)
        {
            foreach (var e in op_write((uint)(((regs.db << 16) + addr) & 0xffffff), data))
            {
                yield return e;
            };
        }

        public IEnumerable op_writepbr(uint addr, byte data)
        {
            foreach (var e in op_write((uint)((regs.pc.b << 16) + (addr & 0xffff)), data))
            {
                yield return e;
            };
        }

        public IEnumerable op_writedp(uint addr, byte data)
        {
            if (regs.e && regs.d.l == 0x00)
            {
                foreach (var e in op_write((regs.d & 0xff00) + ((regs.d + (addr & 0xffff)) & 0xff), data))
                {
                    yield return e;
                };
            }
            else
            {
                foreach (var e in op_write((regs.d + (addr & 0xffff)) & 0xffff, data))
                {
                    yield return e;
                };
            }
        }

        public IEnumerable op_writesp(uint addr, byte data)
        {
            foreach (var e in op_write((regs.s + (addr & 0xffff)) & 0xffff, data))
            {
                yield return e;
            };
        }

        public Regs regs = new Regs();
        public Reg24 aa = new Reg24();
        public Reg24 rd = new Reg24();
        public byte sp, dp;

        protected Result _result = new Result();

        public abstract IEnumerable op_io();
        public abstract IEnumerable op_read(uint addr, Result result);
        public abstract IEnumerable op_write(uint addr, byte data);
        public abstract void last_cycle();
        public abstract bool interrupt_pending();

        public IEnumerable op_io_irq()
        {
            if (interrupt_pending())
            {
                //modify I/O cycle to bus read cycle, do not increment PC
                foreach (var e in op_read(regs.pc.d, _result))
                {
                    yield return e;
                };
            }
            else
            {
                foreach (var e in op_io())
                {
                    yield return e;
                };
            }
        }

        public IEnumerable op_io_cond2()
        {
            if (regs.d.l != 0x00)
            {
                foreach (var e in op_io())
                {
                    yield return e;
                };
            }
        }

        public IEnumerable op_io_cond4(ushort x, ushort y)
        {
            if (!regs.p.x || (x & 0xff00) != (y & 0xff00))
            {
                foreach (var e in op_io())
                {
                    yield return e;
                };
            }
        }

        public IEnumerable op_io_cond6(ushort addr)
        {
            if (regs.e && (regs.pc.w & 0xff00) != (addr & 0xff00))
            {
                foreach (var e in op_io())
                {
                    yield return e;
                };
            }
        }

        public IEnumerable op_adc_b(CPUCoreOpArgument args)
        {
            int result;

            if (!regs.p.d)
            {
                result = regs.a.l + rd.l + Convert.ToInt32(regs.p.c);
            }
            else
            {
                result = (regs.a.l & 0x0f) + (rd.l & 0x0f) + (Convert.ToInt32(regs.p.c) << 0);
                if (result > 0x09)
                {
                    result += 0x06;
                }
                regs.p.c = result > 0x0f;
                result = (regs.a.l & 0xf0) + (rd.l & 0xf0) + (Convert.ToInt32(regs.p.c) << 4) + (result & 0x0f);
            }

            regs.p.v = Convert.ToBoolean(~(regs.a.l ^ rd.l) & (regs.a.l ^ result) & 0x80);
            if (regs.p.d && result > 0x9f)
            {
                result += 0x60;
            }
            regs.p.c = result > 0xff;
            regs.p.n = Convert.ToBoolean(result & 0x80);
            regs.p.z = (byte)result == 0;

            regs.a.l = (byte)result;
            yield break;
        }

        public IEnumerable op_adc_w(CPUCoreOpArgument args)
        {
            int result;

            if (!regs.p.d)
            {
                result = regs.a.w + rd.w + Convert.ToInt32(regs.p.c);
            }
            else
            {
                result = (regs.a.w & 0x000f) + (rd.w & 0x000f) + (Convert.ToInt32(regs.p.c) << 0);
                if (result > 0x0009)
                {
                    result += 0x0006;
                }
                regs.p.c = result > 0x000f;
                result = (regs.a.w & 0x00f0) + (rd.w & 0x00f0) + (Convert.ToInt32(regs.p.c) << 4) + (result & 0x000f);
                if (result > 0x009f)
                {
                    result += 0x0060;
                }
                regs.p.c = result > 0x00ff;
                result = (regs.a.w & 0x0f00) + (rd.w & 0x0f00) + (Convert.ToInt32(regs.p.c) << 8) + (result & 0x00ff);
                if (result > 0x09ff)
                {
                    result += 0x0600;
                }
                regs.p.c = result > 0x0fff;
                result = (regs.a.w & 0xf000) + (rd.w & 0xf000) + (Convert.ToInt32(regs.p.c) << 12) + (result & 0x0fff);
            }

            regs.p.v = Convert.ToBoolean(~(regs.a.w ^ rd.w) & (regs.a.w ^ result) & 0x8000);
            if (regs.p.d && result > 0x9fff)
            {
                result += 0x6000;
            }
            regs.p.c = result > 0xffff;
            regs.p.n = Convert.ToBoolean(result & 0x8000);
            regs.p.z = (ushort)result == 0;

            regs.a.w = (ushort)result;
            yield break;
        }

        public IEnumerable op_and_b(CPUCoreOpArgument args)
        {
            regs.a.l &= rd.l;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = regs.a.l == 0;
            yield break;
        }

        public IEnumerable op_and_w(CPUCoreOpArgument args)
        {
            regs.a.w &= rd.w;
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = regs.a.w == 0;
            yield break;
        }

        public IEnumerable op_bit_b(CPUCoreOpArgument args)
        {
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.v = Convert.ToBoolean(rd.l & 0x40);
            regs.p.z = (rd.l & regs.a.l) == 0;
            yield break;
        }

        public IEnumerable op_bit_w(CPUCoreOpArgument args)
        {
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.v = Convert.ToBoolean(rd.w & 0x4000);
            regs.p.z = (rd.w & regs.a.w) == 0;
            yield break;
        }

        public IEnumerable op_cmp_b(CPUCoreOpArgument args)
        {
            int r = regs.a.l - rd.l;
            regs.p.n = Convert.ToBoolean(r & 0x80);
            regs.p.z = (byte)r == 0;
            regs.p.c = r >= 0;
            yield break;
        }

        public IEnumerable op_cmp_w(CPUCoreOpArgument args)
        {
            int r = regs.a.w - rd.w;
            regs.p.n = Convert.ToBoolean(r & 0x8000);
            regs.p.z = (ushort)r == 0;
            regs.p.c = r >= 0;
            yield break;
        }

        public IEnumerable op_cpx_b(CPUCoreOpArgument args)
        {
            int r = regs.x.l - rd.l;
            regs.p.n = Convert.ToBoolean(r & 0x80);
            regs.p.z = (byte)r == 0;
            regs.p.c = r >= 0;
            yield break;
        }

        public IEnumerable op_cpx_w(CPUCoreOpArgument args)
        {
            int r = regs.x.w - rd.w;
            regs.p.n = Convert.ToBoolean(r & 0x8000);
            regs.p.z = (ushort)r == 0;
            regs.p.c = r >= 0;
            yield break;
        }

        public IEnumerable op_cpy_b(CPUCoreOpArgument args)
        {
            int r = regs.y.l - rd.l;
            regs.p.n = Convert.ToBoolean(r & 0x80);
            regs.p.z = (byte)r == 0;
            regs.p.c = r >= 0;
            yield break;
        }

        public IEnumerable op_cpy_w(CPUCoreOpArgument args)
        {
            int r = regs.y.w - rd.w;
            regs.p.n = Convert.ToBoolean(r & 0x8000);
            regs.p.z = (ushort)r == 0;
            regs.p.c = r >= 0;
            yield break;
        }

        public IEnumerable op_eor_b(CPUCoreOpArgument args)
        {
            regs.a.l ^= rd.l;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = regs.a.l == 0;
            yield break;
        }

        public IEnumerable op_eor_w(CPUCoreOpArgument args)
        {
            regs.a.w ^= rd.w;
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = regs.a.w == 0;
            yield break;
        }

        public IEnumerable op_lda_b(CPUCoreOpArgument args)
        {
            regs.a.l = rd.l;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = regs.a.l == 0;
            yield break;
        }

        public IEnumerable op_lda_w(CPUCoreOpArgument args)
        {
            regs.a.w = rd.w;
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = regs.a.w == 0;
            yield break;
        }

        public IEnumerable op_ldx_b(CPUCoreOpArgument args)
        {
            regs.x.l = rd.l;
            regs.p.n = Convert.ToBoolean(regs.x.l & 0x80);
            regs.p.z = regs.x.l == 0;
            yield break;
        }

        public IEnumerable op_ldx_w(CPUCoreOpArgument args)
        {
            regs.x.w = rd.w;
            regs.p.n = Convert.ToBoolean(regs.x.w & 0x8000);
            regs.p.z = regs.x.w == 0;
            yield break;
        }

        public IEnumerable op_ldy_b(CPUCoreOpArgument args)
        {
            regs.y.l = rd.l;
            regs.p.n = Convert.ToBoolean(regs.y.l & 0x80);
            regs.p.z = regs.y.l == 0;
            yield break;
        }

        public IEnumerable op_ldy_w(CPUCoreOpArgument args)
        {
            regs.y.w = rd.w;
            regs.p.n = Convert.ToBoolean(regs.y.w & 0x8000);
            regs.p.z = regs.y.w == 0;
            yield break;
        }

        public IEnumerable op_ora_b(CPUCoreOpArgument args)
        {
            regs.a.l |= rd.l;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = regs.a.l == 0;
            yield break;
        }

        public IEnumerable op_ora_w(CPUCoreOpArgument args)
        {
            regs.a.w |= rd.w;
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = regs.a.w == 0;
            yield break;
        }

        public IEnumerable op_sbc_b(CPUCoreOpArgument args)
        {
            int result;
            rd.l ^= 0xff;

            if (!regs.p.d)
            {
                result = regs.a.l + rd.l + Convert.ToInt32(regs.p.c);
            }
            else
            {
                result = (regs.a.l & 0x0f) + (rd.l & 0x0f) + (Convert.ToInt32(regs.p.c) << 0);
                if (result <= 0x0f)
                {
                    result -= 0x06;
                }
                regs.p.c = result > 0x0f;
                result = (regs.a.l & 0xf0) + (rd.l & 0xf0) + (Convert.ToInt32(regs.p.c) << 4) + (result & 0x0f);
            }

            regs.p.v = Convert.ToBoolean(~(regs.a.l ^ rd.l) & (regs.a.l ^ result) & 0x80);
            if (regs.p.d && result <= 0xff)
            {
                result -= 0x60;
            }
            regs.p.c = result > 0xff;
            regs.p.n = Convert.ToBoolean(result & 0x80);
            regs.p.z = (byte)result == 0;

            regs.a.l = (byte)result;
            yield break;
        }

        public IEnumerable op_sbc_w(CPUCoreOpArgument args)
        {
            int result;
            rd.w ^= 0xffff;

            if (!regs.p.d)
            {
                result = regs.a.w + rd.w + Convert.ToInt32(regs.p.c);
            }
            else
            {
                result = (regs.a.w & 0x000f) + (rd.w & 0x000f) + (Convert.ToInt32(regs.p.c) << 0);
                if (result <= 0x000f)
                {
                    result -= 0x0006;
                }
                regs.p.c = result > 0x000f;
                result = (regs.a.w & 0x00f0) + (rd.w & 0x00f0) + (Convert.ToInt32(regs.p.c) << 4) + (result & 0x000f);
                if (result <= 0x00ff)
                {
                    result -= 0x0060;
                }
                regs.p.c = result > 0x00ff;
                result = (regs.a.w & 0x0f00) + (rd.w & 0x0f00) + (Convert.ToInt32(regs.p.c) << 8) + (result & 0x00ff);
                if (result <= 0x0fff)
                {
                    result -= 0x0600;
                }
                regs.p.c = result > 0x0fff;
                result = (regs.a.w & 0xf000) + (rd.w & 0xf000) + (Convert.ToInt32(regs.p.c) << 12) + (result & 0x0fff);
            }

            regs.p.v = Convert.ToBoolean(~(regs.a.w ^ rd.w) & (regs.a.w ^ result) & 0x8000);
            if (regs.p.d && result <= 0xffff)
            {
                result -= 0x6000;
            }
            regs.p.c = result > 0xffff;
            regs.p.n = Convert.ToBoolean(result & 0x8000);
            regs.p.z = (ushort)result == 0;

            regs.a.w = (ushort)result;
            yield break;
        }

        public IEnumerable op_inc_b(CPUCoreOpArgument args)
        {
            rd.l++;
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.z = rd.l == 0;
            yield break;
        }

        public IEnumerable op_inc_w(CPUCoreOpArgument args)
        {
            rd.w++;
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.z = rd.w == 0;
            yield break;
        }

        public IEnumerable op_dec_b(CPUCoreOpArgument args)
        {
            rd.l--;
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.z = rd.l == 0;
            yield break;
        }

        public IEnumerable op_dec_w(CPUCoreOpArgument args)
        {
            rd.w--;
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.z = rd.w == 0;
            yield break;
        }

        public IEnumerable op_asl_b(CPUCoreOpArgument args)
        {
            regs.p.c = Convert.ToBoolean(rd.l & 0x80);
            rd.l <<= 1;
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.z = rd.l == 0;
            yield break;
        }

        public IEnumerable op_asl_w(CPUCoreOpArgument args)
        {
            regs.p.c = Convert.ToBoolean(rd.w & 0x8000);
            rd.w <<= 1;
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.z = rd.w == 0;
            yield break;
        }

        public IEnumerable op_lsr_b(CPUCoreOpArgument args)
        {
            regs.p.c = Convert.ToBoolean(rd.l & 1);
            rd.l >>= 1;
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.z = rd.l == 0;
            yield break;
        }

        public IEnumerable op_lsr_w(CPUCoreOpArgument args)
        {
            regs.p.c = Convert.ToBoolean(rd.w & 1);
            rd.w >>= 1;
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.z = rd.w == 0;
            yield break;
        }

        public IEnumerable op_rol_b(CPUCoreOpArgument args)
        {
            uint carry = Convert.ToUInt32(regs.p.c);
            regs.p.c = Convert.ToBoolean(rd.l & 0x80);
            rd.l = (byte)((uint)(rd.l << 1) | carry);
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.z = rd.l == 0;
            yield break;
        }

        public IEnumerable op_rol_w(CPUCoreOpArgument args)
        {
            uint carry = Convert.ToUInt32(regs.p.c);
            regs.p.c = Convert.ToBoolean(rd.w & 0x8000);
            rd.w = (ushort)((uint)(rd.w << 1) | carry);
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.z = rd.w == 0;
            yield break;
        }

        public IEnumerable op_ror_b(CPUCoreOpArgument args)
        {
            uint carry = Convert.ToUInt32(regs.p.c) << 7;
            regs.p.c = Convert.ToBoolean(rd.l & 1);
            rd.l = (byte)(carry | (uint)(rd.l >> 1));
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.z = rd.l == 0;
            yield break;
        }

        public IEnumerable op_ror_w(CPUCoreOpArgument args)
        {
            uint carry = Convert.ToUInt32(regs.p.c) << 15;
            regs.p.c = Convert.ToBoolean(rd.w & 1);
            rd.w = (ushort)(carry | (uint)(rd.w >> 1));
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.z = rd.w == 0;
            yield break;
        }

        public IEnumerable op_trb_b(CPUCoreOpArgument args)
        {
            regs.p.z = (rd.l & regs.a.l) == 0;
            rd.l &= (byte)(~regs.a.l);
            yield break;
        }

        public IEnumerable op_trb_w(CPUCoreOpArgument args)
        {
            regs.p.z = (rd.w & regs.a.w) == 0;
            rd.w &= (ushort)(~regs.a.w);
            yield break;
        }

        public IEnumerable op_tsb_b(CPUCoreOpArgument args)
        {
            regs.p.z = (rd.l & regs.a.l) == 0;
            rd.l |= regs.a.l;
            yield break;
        }

        public IEnumerable op_tsb_w(CPUCoreOpArgument args)
        {
            regs.p.z = (rd.w & regs.a.w) == 0;
            rd.w |= regs.a.w;
            yield break;
        }

        public IEnumerable op_read_const_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            last_cycle();
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_const_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_bit_const_b(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            regs.p.z = ((rd.l & regs.a.l) == 0);
        }

        public IEnumerable op_read_bit_const_w(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            regs.p.z = ((rd.w & regs.a.w) == 0);
        }

        public IEnumerable op_read_addr_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            last_cycle();
            foreach (var e in op_readdbr(aa.w, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_addr_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readdbr(aa.w + 0U, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readdbr(aa.w + 1U, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_addrx_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io_cond4(aa.w, (ushort)(aa.w + regs.x.w)))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_readdbr((uint)(aa.w + regs.x.w), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_addrx_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io_cond4(aa.w, (ushort)(aa.w + regs.x.w)))
            {
                yield return e;
            };
            foreach (var e in op_readdbr((uint)(aa.w + regs.x.w + 0), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readdbr((uint)(aa.w + regs.x.w + 1), _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_addry_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io_cond4(aa.w, (ushort)(aa.w + regs.y.w)))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_readdbr((uint)(aa.w + regs.y.w), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_addry_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io_cond4(aa.w, (ushort)(aa.w + regs.y.w)))
            {
                yield return e;
            };
            foreach (var e in op_readdbr((uint)(aa.w + regs.y.w + 0), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readdbr((uint)(aa.w + regs.y.w + 1), _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_long_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            last_cycle();
            foreach (var e in op_readlong(aa.d, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_long_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            foreach (var e in op_readlong(aa.d + 0, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readlong(aa.d + 1, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_longx_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            last_cycle();
            foreach (var e in op_readlong(aa.d + regs.x.w, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_longx_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            foreach (var e in op_readlong(aa.d + regs.x.w + 0, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readlong(aa.d + regs.x.w + 1, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_dp_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_readdp(dp, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_dp_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_dpr_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            int n = args.x;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_readdp((uint)(dp + regs.r[n].w), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_dpr_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            int n = args.x;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((uint)(dp + regs.r[n].w + 0), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readdp((uint)(dp + regs.r[n].w + 1), _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_idp_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            last_cycle();
            foreach (var e in op_readdbr(aa.w, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_idp_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readdbr(aa.w + 0U, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readdbr(aa.w + 1U, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_idpx_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((uint)(dp + regs.x.w + 0), _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp((uint)(dp + regs.x.w + 1), _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            last_cycle();
            foreach (var e in op_readdbr(aa.w, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_idpx_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((uint)(dp + regs.x.w + 0), _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp((uint)(dp + regs.x.w + 1), _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readdbr(aa.w + 0U, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readdbr(aa.w + 1U, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_idpy_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io_cond4(aa.w, (ushort)(aa.w + regs.y.w)))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_readdbr((uint)(aa.w + regs.y.w), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_idpy_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io_cond4(aa.w, (ushort)(aa.w + regs.y.w)))
            {
                yield return e;
            };
            foreach (var e in op_readdbr((uint)(aa.w + regs.y.w + 0), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readdbr((uint)(aa.w + regs.y.w + 1), _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_ildp_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readdp(dp + 2U, _result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            last_cycle();
            foreach (var e in op_readlong(aa.d, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_ildp_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readdp(dp + 2U, _result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            foreach (var e in op_readlong(aa.d + 0, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readlong(aa.d + 1, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_ildpy_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readdp(dp + 2U, _result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            last_cycle();
            foreach (var e in op_readlong(aa.d + regs.y.w, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_ildpy_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readdp(dp + 2U, _result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            foreach (var e in op_readlong(aa.d + regs.y.w + 0, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readlong(aa.d + regs.y.w + 1, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_sr_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_readsp(sp, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_sr_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readsp(sp + 0U, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readsp(sp + 1U, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_isry_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readsp(sp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readsp(sp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_readdbr((uint)(aa.w + regs.y.w), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_read_isry_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readsp(sp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readsp(sp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdbr((uint)(aa.w + regs.y.w + 0), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readdbr((uint)(aa.w + regs.y.w + 1), _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
        }

        public IEnumerable op_write_addr_b(CPUCoreOpArgument args)
        {
            int n = args.x;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            last_cycle();
            foreach (var e in op_writedbr(aa.w, (byte)regs.r[n]))
            {
                yield return e;
            };
        }

        public IEnumerable op_write_addr_w(CPUCoreOpArgument args)
        {
            int n = args.x;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_writedbr(aa.w + 0U, (byte)(regs.r[n] >> 0)))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedbr(aa.w + 1U, (byte)(regs.r[n] >> 8)))
            {
                yield return e;
            };
        }

        public IEnumerable op_write_addrr_b(CPUCoreOpArgument args)
        {
            int n = args.x;
            int i = args.y;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedbr(aa.w + (uint)regs.r[i], (byte)regs.r[n]))
            {
                yield return e;
            };
        }

        public IEnumerable op_write_addrr_w(CPUCoreOpArgument args)
        {
            int n = args.x;
            int i = args.y;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writedbr(aa.w + (uint)regs.r[i] + 0, (byte)(regs.r[n] >> 0)))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedbr(aa.w + (uint)regs.r[i] + 1, (byte)(regs.r[n] >> 8)))
            {
                yield return e;
            };
        }

        public IEnumerable op_write_longr_b(CPUCoreOpArgument args)
        {
            int i = args.x;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            last_cycle();
            foreach (var e in op_writelong(aa.d + (uint)regs.r[i], regs.a.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_write_longr_w(CPUCoreOpArgument args)
        {
            int i = args.x;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            foreach (var e in op_writelong(aa.d + (uint)regs.r[i] + 0, regs.a.l))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writelong(aa.d + (uint)regs.r[i] + 1, regs.a.h))
            {
                yield return e;
            };
        }

        public IEnumerable op_write_dp_b(CPUCoreOpArgument args)
        {
            int n = args.x;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedp(dp, (byte)regs.r[n]))
            {
                yield return e;
            };
        }

        public IEnumerable op_write_dp_w(CPUCoreOpArgument args)
        {
            int n = args.x;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_writedp(dp + 0U, (byte)(regs.r[n] >> 0)))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedp(dp + 1U, (byte)(regs.r[n] >> 8)))
            {
                yield return e;
            };
        }

        public IEnumerable op_write_dpr_b(CPUCoreOpArgument args)
        {
            int n = args.x;
            int i = args.y;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedp(dp + (uint)regs.r[i], (byte)regs.r[n]))
            {
                yield return e;
            };
        }

        public IEnumerable op_write_dpr_w(CPUCoreOpArgument args)
        {
            int n = args.x;
            int i = args.y;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writedp(dp + (uint)regs.r[i] + 0, (byte)(regs.r[n] >> 0)))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedp(dp + (uint)regs.r[i] + 1, (byte)(regs.r[n] >> 8)))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_idp_b(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            last_cycle();
            foreach (var e in op_writedbr(aa.w, regs.a.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_idp_w(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_writedbr(aa.w + 0U, regs.a.l))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedbr(aa.w + 1U, regs.a.h))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_ildp_b(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readdp(dp + 2U, _result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            last_cycle();
            foreach (var e in op_writelong(aa.d, regs.a.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_ildp_w(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readdp(dp + 2U, _result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            foreach (var e in op_writelong(aa.d + 0, regs.a.l))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writelong(aa.d + 1, regs.a.h))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_idpx_b(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((uint)(dp + regs.x.w + 0), _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp((uint)(dp + regs.x.w + 1), _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            last_cycle();
            foreach (var e in op_writedbr(aa.w, regs.a.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_idpx_w(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((uint)(dp + regs.x.w + 0), _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp((uint)(dp + regs.x.w + 1), _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_writedbr(aa.w + 0U, regs.a.l))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedbr(aa.w + 1U, regs.a.h))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_idpy_b(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedbr((uint)(aa.w + regs.y.w), regs.a.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_idpy_w(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writedbr((uint)(aa.w + regs.y.w + 0), regs.a.l))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedbr((uint)(aa.w + regs.y.w + 1), regs.a.h))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_ildpy_b(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readdp(dp + 2U, _result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            last_cycle();
            foreach (var e in op_writelong(aa.d + regs.y.w, regs.a.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_ildpy_w(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readdp(dp + 2U, _result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            foreach (var e in op_writelong(aa.d + regs.y.w + 0, regs.a.l))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writelong(aa.d + regs.y.w + 1, regs.a.h))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_sr_b(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writesp(sp, regs.a.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_sr_w(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writesp(sp + 0U, regs.a.l))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writesp(sp + 1U, regs.a.h))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_isry_b(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readsp(sp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readsp(sp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedbr((uint)(aa.w + regs.y.w), regs.a.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_sta_isry_w(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readsp(sp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readsp(sp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writedbr((uint)(aa.w + regs.y.w + 0), regs.a.l))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedbr((uint)(aa.w + regs.y.w + 1), regs.a.h))
            {
                yield return e;
            };
        }

        public IEnumerable op_adjust_imm_b(CPUCoreOpArgument args)
        {
            int n = args.x;
            int adjust = args.y;
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.r[n].l += (byte)adjust;
            regs.p.n = Convert.ToBoolean(regs.r[n].l & 0x80);
            regs.p.z = (regs.r[n].l == 0);
        }

        public IEnumerable op_adjust_imm_w(CPUCoreOpArgument args)
        {
            int n = args.x;
            int adjust = args.y;
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.r[n].w += (ushort)adjust;
            regs.p.n = Convert.ToBoolean(regs.r[n].w & 0x8000);
            regs.p.z = (regs.r[n].w == 0);
        }

        public IEnumerable op_asl_imm_b(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.p.c = Convert.ToBoolean(regs.a.l & 0x80);
            regs.a.l <<= 1;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = (regs.a.l == 0);
        }

        public IEnumerable op_asl_imm_w(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.p.c = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.a.w <<= 1;
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = (regs.a.w == 0);
        }

        public IEnumerable op_lsr_imm_b(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.p.c = Convert.ToBoolean(regs.a.l & 0x01);
            regs.a.l >>= 1;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = (regs.a.l == 0);
        }

        public IEnumerable op_lsr_imm_w(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.p.c = Convert.ToBoolean(regs.a.w & 0x0001);
            regs.a.w >>= 1;
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = (regs.a.w == 0);
        }

        public IEnumerable op_rol_imm_b(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            bool carry = regs.p.c;
            regs.p.c = Convert.ToBoolean(regs.a.l & 0x80);
            regs.a.l = (byte)((regs.a.l << 1) | Convert.ToInt32(carry));
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = (regs.a.l == 0);
        }

        public IEnumerable op_rol_imm_w(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            bool carry = regs.p.c;
            regs.p.c = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.a.w = (ushort)((regs.a.w << 1) | Convert.ToInt32(carry));
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = (regs.a.w == 0);
        }

        public IEnumerable op_ror_imm_b(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            bool carry = regs.p.c;
            regs.p.c = Convert.ToBoolean(regs.a.l & 0x01);
            regs.a.l = (byte)((Convert.ToInt32(carry) << 7) | (regs.a.l >> 1));
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = (regs.a.l == 0);
        }

        public IEnumerable op_ror_imm_w(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            bool carry = regs.p.c;
            regs.p.c = Convert.ToBoolean(regs.a.w & 0x0001);
            regs.a.w = (ushort)((Convert.ToInt32(carry) << 15) | (regs.a.w >> 1));
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = (regs.a.w == 0);
        }

        public IEnumerable op_adjust_addr_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readdbr(aa.w, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedbr(aa.w, rd.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_adjust_addr_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readdbr(aa.w + 0U, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_readdbr(aa.w + 1U, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
            foreach (var e in op_writedbr(aa.w + 1U, rd.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedbr(aa.w + 0U, rd.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_adjust_addrx_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdbr((uint)(aa.w + regs.x.w), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedbr((uint)(aa.w + regs.x.w), rd.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_adjust_addrx_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdbr((uint)(aa.w + regs.x.w + 0), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_readdbr((uint)(aa.w + regs.x.w + 1), _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
            foreach (var e in op_writedbr((uint)(aa.w + regs.x.w + 1), rd.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedbr((uint)(aa.w + regs.x.w + 0), rd.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_adjust_dp_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedp(dp, rd.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_adjust_dp_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
            foreach (var e in op_writedp(dp + 1U, rd.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedp(dp + 0U, rd.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_adjust_dpx_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((uint)(dp + regs.x.w), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedp((uint)(dp + regs.x.w), rd.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_adjust_dpx_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((uint)(dp + regs.x.w + 0), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_readdp((uint)(dp + regs.x.w + 1), _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op.Invoke(null))
            {
                yield return e;
            };
            foreach (var e in op_writedp((uint)(dp + regs.x.w + 1), rd.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writedp((uint)(dp + regs.x.w + 0), rd.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_branch(CPUCoreOpArgument args)
        {
            int bit = args.x;
            int val = args.y;
            if (Bit.ToBit(regs.p & (uint)bit) != val)
            {
                last_cycle();
                foreach (var e in op_readpc(_result))
                {
                    yield return e;
                };
                rd.l = _result.Value;
            }
            else
            {
                foreach (var e in op_readpc(_result))
                {
                    yield return e;
                };
                rd.l = _result.Value;
                aa.w = (ushort)(regs.pc.d + (sbyte)rd.l);
                foreach (var e in op_io_cond6(aa.w))
                {
                    yield return e;
                };
                last_cycle();
                foreach (var e in op_io())
                {
                    yield return e;
                };
                regs.pc.w = aa.w;
            }
        }

        public IEnumerable op_bra(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            aa.w = (ushort)(regs.pc.d + (sbyte)rd.l);
            foreach (var e in op_io_cond6(aa.w))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.pc.w = aa.w;
        }

        public IEnumerable op_brl(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            last_cycle();
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.pc.w = (ushort)(regs.pc.d + (short)rd.w);
        }

        public IEnumerable op_jmp_addr(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            regs.pc.w = rd.w;
        }

        public IEnumerable op_jmp_long(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            last_cycle();
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.b = _result.Value;
            regs.pc.d = rd.d & 0xffffff;
        }

        public IEnumerable op_jmp_iaddr(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readaddr(aa.w + 0U, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readaddr(aa.w + 1U, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            regs.pc.w = rd.w;
        }

        public IEnumerable op_jmp_iaddrx(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readpbr((uint)(aa.w + regs.x.w + 0), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readpbr((uint)(aa.w + regs.x.w + 1), _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            regs.pc.w = rd.w;
        }

        public IEnumerable op_jmp_iladdr(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_readaddr(aa.w + 0U, _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_readaddr(aa.w + 1U, _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            last_cycle();
            foreach (var e in op_readaddr(aa.w + 2U, _result))
            {
                yield return e;
            };
            rd.b = _result.Value;
            regs.pc.d = rd.d & 0xffffff;
        }

        public IEnumerable op_jsr_addr(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.pc.w--;
            foreach (var e in op_writestack(regs.pc.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestack(regs.pc.l))
            {
                yield return e;
            };
            regs.pc.w = aa.w;
        }

        public IEnumerable op_jsr_long_e(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_writestackn(regs.pc.b))
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            regs.pc.w--;
            foreach (var e in op_writestackn(regs.pc.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestackn(regs.pc.l))
            {
                yield return e;
            };
            regs.pc.d = aa.d & 0xffffff;
            regs.s.h = 0x01;
        }

        public IEnumerable op_jsr_long_n(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_writestackn(regs.pc.b))
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.b = _result.Value;
            regs.pc.w--;
            foreach (var e in op_writestackn(regs.pc.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestackn(regs.pc.l))
            {
                yield return e;
            };
            regs.pc.d = aa.d & 0xffffff;
        }

        public IEnumerable op_jsr_iaddrx_e(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_writestackn(regs.pc.h))
            {
                yield return e;
            };
            foreach (var e in op_writestackn(regs.pc.l))
            {
                yield return e;
            };
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readpbr((uint)(aa.w + regs.x.w + 0), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readpbr((uint)(aa.w + regs.x.w + 1), _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            regs.pc.w = rd.w;
            regs.s.h = 0x01;
        }

        public IEnumerable op_jsr_iaddrx_n(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_writestackn(regs.pc.h))
            {
                yield return e;
            };
            foreach (var e in op_writestackn(regs.pc.l))
            {
                yield return e;
            };
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readpbr((uint)(aa.w + regs.x.w + 0), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readpbr((uint)(aa.w + regs.x.w + 1), _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            regs.pc.w = rd.w;
        }

        public IEnumerable op_rti_e(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            regs.p.Assign((byte)(_result.Value | 0x30));
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            regs.pc.w = rd.w;
        }

        public IEnumerable op_rti_n(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            regs.p.Assign(_result.Value);
            if (regs.p.x)
            {
                regs.x.h = 0x00;
                regs.y.h = 0x00;
            }
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            last_cycle();
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            rd.b = _result.Value;
            regs.pc.d = rd.d & 0xffffff;
            update_table();
        }

        public IEnumerable op_rts(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            last_cycle();
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.pc.w = ++rd.w;
        }

        public IEnumerable op_rtl_e(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readstackn(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_readstackn(_result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            last_cycle();
            foreach (var e in op_readstackn(_result))
            {
                yield return e;
            };
            rd.b = _result.Value;
            regs.pc.b = rd.b;
            regs.pc.w = ++rd.w;
            regs.s.h = 0x01;
        }

        public IEnumerable op_rtl_n(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readstackn(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_readstackn(_result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            last_cycle();
            foreach (var e in op_readstackn(_result))
            {
                yield return e;
            };
            rd.b = _result.Value;
            regs.pc.b = rd.b;
            regs.pc.w = ++rd.w;
        }

        public IEnumerable op_nop(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
        }

        public IEnumerable op_wdm(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
        }

        public IEnumerable op_xba(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.a.l ^= regs.a.h;
            regs.a.h ^= regs.a.l;
            regs.a.l ^= regs.a.h;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = (regs.a.l == 0);
        }

        public IEnumerable op_move_b(CPUCoreOpArgument args)
        {
            int adjust = args.x;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            regs.db = dp;
            foreach (var e in op_readlong((uint)((sp << 16) | regs.x.w), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_writelong((uint)((dp << 16) | regs.y.w), rd.l))
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.x.l += (byte)adjust;
            regs.y.l += (byte)adjust;
            last_cycle();
            foreach (var e in op_io())
            {
                yield return e;
            };
            if (Convert.ToBoolean(regs.a.w--))
            {
                regs.pc.w -= 3;
            }
        }

        public IEnumerable op_move_w(CPUCoreOpArgument args)
        {
            int adjust = args.x;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            regs.db = dp;
            foreach (var e in op_readlong((uint)((sp << 16) | regs.x.w), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            foreach (var e in op_writelong((uint)((dp << 16) | regs.y.w), rd.l))
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.x.w += (ushort)adjust;
            regs.y.w += (ushort)adjust;
            last_cycle();
            foreach (var e in op_io())
            {
                yield return e;
            };
            if (Convert.ToBoolean(regs.a.w--))
            {
                regs.pc.w -= 3;
            }
        }

        public IEnumerable op_interrupt_e(CPUCoreOpArgument args)
        {
            int vectorE = args.x;
            int vectorN = args.y;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            foreach (var e in op_writestack(regs.pc.h))
            {
                yield return e;
            };
            foreach (var e in op_writestack(regs.pc.l))
            {
                yield return e;
            };
            foreach (var e in op_writestack((byte)regs.p))
            {
                yield return e;
            };
            foreach (var e in op_readlong((uint)(vectorE + 0), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            regs.pc.b = 0;
            regs.p.i = Convert.ToBoolean(1);
            regs.p.d = Convert.ToBoolean(0);
            last_cycle();
            foreach (var e in op_readlong((uint)(vectorE + 1), _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            regs.pc.w = rd.w;
        }

        public IEnumerable op_interrupt_n(CPUCoreOpArgument args)
        {
            int vectorE = args.x;
            int vectorN = args.y;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            foreach (var e in op_writestack(regs.pc.b))
            {
                yield return e;
            };
            foreach (var e in op_writestack(regs.pc.h))
            {
                yield return e;
            };
            foreach (var e in op_writestack(regs.pc.l))
            {
                yield return e;
            };
            foreach (var e in op_writestack((byte)regs.p))
            {
                yield return e;
            };
            foreach (var e in op_readlong((uint)(vectorN + 0), _result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            regs.pc.b = 0x00;
            regs.p.i = Convert.ToBoolean(1);
            regs.p.d = Convert.ToBoolean(0);
            last_cycle();
            foreach (var e in op_readlong((uint)(vectorN + 1), _result))
            {
                yield return e;
            };
            rd.h = _result.Value;
            regs.pc.w = rd.w;
        }

        public IEnumerable op_stp(CPUCoreOpArgument args)
        {
            regs.wai = true;
            while (regs.wai)
            {
                last_cycle();
                foreach (var e in op_io())
                {
                    yield return e;
                };
            }
        }

        public IEnumerable op_wai(CPUCoreOpArgument args)
        {
            regs.wai = true;
            while (regs.wai)
            {
                last_cycle();
                foreach (var e in op_io())
                {
                    yield return e;
                };
            }
            foreach (var e in op_io())
            {
                yield return e;
            };
        }

        public IEnumerable op_xce(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            bool carry = regs.p.c;
            regs.p.c = regs.e;
            regs.e = carry;
            if (regs.e)
            {
                regs.p.Assign((byte)(regs.p | 0x30));
                regs.s.h = 0x01;
            }
            if (regs.p.x)
            {
                regs.x.h = 0x00;
                regs.y.h = 0x00;
            }
            update_table();
        }

        public IEnumerable op_flag(CPUCoreOpArgument args)
        {
            int mask = args.x;
            int value = args.y;
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.p.Assign((byte)(((uint)regs.p & ~mask) | (uint)value));
        }

        public IEnumerable op_pflag_e(CPUCoreOpArgument args)
        {
            int mode = args.x;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.p.Assign((byte)(Convert.ToBoolean(mode) ? regs.p | rd.l : (uint)regs.p & ~rd.l));
            regs.p.Assign((byte)(regs.p | 0x30));
            if (regs.p.x)
            {
                regs.x.h = 0x00;
                regs.y.h = 0x00;
            }
            update_table();
        }

        public IEnumerable op_pflag_n(CPUCoreOpArgument args)
        {
            int mode = args.x;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd.l = _result.Value;
            last_cycle();
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.p.Assign((byte)(Convert.ToBoolean(mode) ? regs.p | rd.l : (uint)regs.p & ~rd.l));
            if (regs.p.x)
            {
                regs.x.h = 0x00;
                regs.y.h = 0x00;
            }
            update_table();
        }

        public IEnumerable op_transfer_b(CPUCoreOpArgument args)
        {
            int from = args.x;
            int to = args.y;
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.r[to].l = regs.r[from].l;
            regs.p.n = Convert.ToBoolean(regs.r[to].l & 0x80);
            regs.p.z = (regs.r[to].l == 0);
        }

        public IEnumerable op_transfer_w(CPUCoreOpArgument args)
        {
            int from = args.x;
            int to = args.y;
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.r[to].w = regs.r[from].w;
            regs.p.n = Convert.ToBoolean(regs.r[to].w & 0x8000);
            regs.p.z = (regs.r[to].w == 0);
        }

        public IEnumerable op_tcs_e(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.s.l = regs.a.l;
        }

        public IEnumerable op_tcs_n(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.s.w = regs.a.w;
        }

        public IEnumerable op_tsx_b(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.x.l = regs.s.l;
            regs.p.n = Convert.ToBoolean(regs.x.l & 0x80);
            regs.p.z = (regs.x.l == 0);
        }

        public IEnumerable op_tsx_w(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.x.w = regs.s.w;
            regs.p.n = Convert.ToBoolean(regs.x.w & 0x8000);
            regs.p.z = (regs.x.w == 0);
        }

        public IEnumerable op_txs_e(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.s.l = regs.x.l;
        }

        public IEnumerable op_txs_n(CPUCoreOpArgument args)
        {
            last_cycle();
            foreach (var e in op_io_irq())
            {
                yield return e;
            };
            regs.s.w = regs.x.w;
        }

        public IEnumerable op_push_b(CPUCoreOpArgument args)
        {
            int n = args.x;
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestack(regs.r[n].l))
            {
                yield return e;
            };
        }

        public IEnumerable op_push_w(CPUCoreOpArgument args)
        {
            int n = args.x;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writestack(regs.r[n].h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestack(regs.r[n].l))
            {
                yield return e;
            };
        }

        public IEnumerable op_phd_e(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writestackn(regs.d.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestackn(regs.d.l))
            {
                yield return e;
            };
            regs.s.h = 0x01;
        }

        public IEnumerable op_phd_n(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writestackn(regs.d.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestackn(regs.d.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_phb(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestack(regs.db))
            {
                yield return e;
            };
        }

        public IEnumerable op_phk(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestack(regs.pc.b))
            {
                yield return e;
            };
        }

        public IEnumerable op_php(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestack((byte)regs.p))
            {
                yield return e;
            };
        }

        public IEnumerable op_pull_b(CPUCoreOpArgument args)
        {
            int n = args.x;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            regs.r[n].l = _result.Value;
            regs.p.n = Convert.ToBoolean(regs.r[n].l & 0x80);
            regs.p.z = (regs.r[n].l == 0);
        }

        public IEnumerable op_pull_w(CPUCoreOpArgument args)
        {
            int n = args.x;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            regs.r[n].l = _result.Value;
            last_cycle();
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            regs.r[n].h = _result.Value;
            regs.p.n = Convert.ToBoolean(regs.r[n].w & 0x8000);
            regs.p.z = (regs.r[n].w == 0);
        }

        public IEnumerable op_pld_e(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readstackn(_result))
            {
                yield return e;
            };
            regs.d.l = _result.Value;
            last_cycle();
            foreach (var e in op_readstackn(_result))
            {
                yield return e;
            };
            regs.d.h = _result.Value;
            regs.p.n = Convert.ToBoolean(regs.d.w & 0x8000);
            regs.p.z = (regs.d.w == 0);
            regs.s.h = 0x01;
        }

        public IEnumerable op_pld_n(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readstackn(_result))
            {
                yield return e;
            };
            regs.d.l = _result.Value;
            last_cycle();
            foreach (var e in op_readstackn(_result))
            {
                yield return e;
            };
            regs.d.h = _result.Value;
            regs.p.n = Convert.ToBoolean(regs.d.w & 0x8000);
            regs.p.z = (regs.d.w == 0);
        }

        public IEnumerable op_plb(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            regs.db = _result.Value;
            regs.p.n = Convert.ToBoolean(regs.db & 0x80);
            regs.p.z = (regs.db == 0);
        }

        public IEnumerable op_plp_e(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            regs.p.Assign((byte)(_result.Value | 0x30));
            if (regs.p.x)
            {
                regs.x.h = 0x00;
                regs.y.h = 0x00;
            }
            update_table();
        }

        public IEnumerable op_plp_n(CPUCoreOpArgument args)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            regs.p.Assign(_result.Value);
            if (regs.p.x)
            {
                regs.x.h = 0x00;
                regs.y.h = 0x00;
            }
            update_table();
        }

        public IEnumerable op_pea_e(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_writestackn(aa.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestackn(aa.l))
            {
                yield return e;
            };
            regs.s.h = 0x01;
        }

        public IEnumerable op_pea_n(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_writestackn(aa.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestackn(aa.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_pei_e(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_writestackn(aa.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestackn(aa.l))
            {
                yield return e;
            };
            regs.s.h = 0x01;
        }

        public IEnumerable op_pei_n(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io_cond2())
            {
                yield return e;
            };
            foreach (var e in op_readdp(dp + 0U, _result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readdp(dp + 1U, _result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_writestackn(aa.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestackn(aa.l))
            {
                yield return e;
            };
        }

        public IEnumerable op_per_e(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            rd.w = (ushort)(regs.pc.d + (short)aa.w);
            foreach (var e in op_writestackn(rd.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestackn(rd.l))
            {
                yield return e;
            };
            regs.s.h = 0x01;
        }

        public IEnumerable op_per_n(CPUCoreOpArgument args)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.l = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            aa.h = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            rd.w = (ushort)(regs.pc.d + (short)aa.w);
            foreach (var e in op_writestackn(rd.h))
            {
                yield return e;
            };
            last_cycle();
            foreach (var e in op_writestackn(rd.l))
            {
                yield return e;
            };
        }

        public ArraySegment<CPUCoreOperation> opcode_table;
        public CPUCoreOperation[] op_table = new CPUCoreOperation[256 * 5];

        private CPUCoreOp GetCoreOp(string name, string modifier = "")
        {
            return (CPUCoreOp)Delegate.CreateDelegate(typeof(CPUCoreOp), this, "op_" + name + modifier);
        }

        private void opA(byte id, string name)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name), null);
        }

        private void opAII(byte id, string name, int x, int y)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name), new CPUCoreOpArgument() { x = x, y = y });
        }

        private void opE(byte id, string name)
        {
            op_table[(int)Table.EM + id] = new CPUCoreOperation(GetCoreOp(name, "_e"), null);
            op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_n"), null);
        }

        private void opEI(byte id, string name, int x)
        {
            op_table[(int)Table.EM + id] = new CPUCoreOperation(GetCoreOp(name, "_e"), new CPUCoreOpArgument() { x = x });
            op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_n"), new CPUCoreOpArgument() { x = x });
        }

        private void opEII(byte id, string name, int x, int y)
        {
            op_table[(int)Table.EM + id] = new CPUCoreOperation(GetCoreOp(name, "_e"), new CPUCoreOpArgument() { x = x, y = y });
            op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_n"), new CPUCoreOpArgument() { x = x, y = y });
        }

        private void opM(byte id, string name)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), null);
            op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), null);
        }

        private void opMI(byte id, string name, int x)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { x = x });
            op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { x = x });
        }

        private void opMII(byte id, string name, int x, int y)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { x = x, y = y });
            op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { x = x, y = y });
        }

        private void opMF(byte id, string name, string fn)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_b") });
            op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_w") });
        }

        private void opMFI(byte id, string name, string fn, int x)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_b"), x = x });
            op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_w"), x = x });
        }

        private void opX(byte id, string name)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.mX + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), null);
            op_table[(int)Table.Mx + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), null);
        }

        private void opXI(byte id, string name, int x)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.mX + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { x = x });
            op_table[(int)Table.Mx + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { x = x });
        }

        private void opXII(byte id, string name, int x, int y)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.mX + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { x = x, y = y });
            op_table[(int)Table.Mx + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { x = x, y = y });
        }

        private void opXF(byte id, string name, string fn)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.mX + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_b") });
            op_table[(int)Table.Mx + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_w") });
        }

        private void opXFI(byte id, string name, string fn, int x)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.mX + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_b"), x = x });
            op_table[(int)Table.Mx + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_w"), x = x });
        }

        public void initialize_opcode_table()
        {
            opEII(0x00, "interrupt", 0xfffe, 0xffe6);
            opMF(0x01, "read_idpx", "ora");
            opEII(0x02, "interrupt", 0xfff4, 0xffe4);
            opMF(0x03, "read_sr", "ora");
            opMF(0x04, "adjust_dp", "tsb");
            opMF(0x05, "read_dp", "ora");
            opMF(0x06, "adjust_dp", "asl");
            opMF(0x07, "read_ildp", "ora");
            opA(0x08, "php");
            opMF(0x09, "read_const", "ora");
            opM(0x0a, "asl_imm");
            opE(0x0b, "phd");
            opMF(0x0c, "adjust_addr", "tsb");
            opMF(0x0d, "read_addr", "ora");
            opMF(0x0e, "adjust_addr", "asl");
            opMF(0x0f, "read_long", "ora");
            opAII(0x10, "branch", 0x80, Convert.ToInt32(false));
            opMF(0x11, "read_idpy", "ora");
            opMF(0x12, "read_idp", "ora");
            opMF(0x13, "read_isry", "ora");
            opMF(0x14, "adjust_dp", "trb");
            opMFI(0x15, "read_dpr", "ora", (int)OpCode.X);
            opMF(0x16, "adjust_dpx", "asl");
            opMF(0x17, "read_ildpy", "ora");
            opAII(0x18, "flag", 0x01, 0x00);
            opMF(0x19, "read_addry", "ora");
            opMII(0x1a, "adjust_imm", (int)OpCode.A, +1);
            opE(0x1b, "tcs");
            opMF(0x1c, "adjust_addr", "trb");
            opMF(0x1d, "read_addrx", "ora");
            opMF(0x1e, "adjust_addrx", "asl");
            opMF(0x1f, "read_longx", "ora");
            opA(0x20, "jsr_addr");
            opMF(0x21, "read_idpx", "and");
            opE(0x22, "jsr_long");
            opMF(0x23, "read_sr", "and");
            opMF(0x24, "read_dp", "bit");
            opMF(0x25, "read_dp", "and");
            opMF(0x26, "adjust_dp", "rol");
            opMF(0x27, "read_ildp", "and");
            opE(0x28, "plp");
            opMF(0x29, "read_const", "and");
            opM(0x2a, "rol_imm");
            opE(0x2b, "pld");
            opMF(0x2c, "read_addr", "bit");
            opMF(0x2d, "read_addr", "and");
            opMF(0x2e, "adjust_addr", "rol");
            opMF(0x2f, "read_long", "and");
            opAII(0x30, "branch", 0x80, Convert.ToInt32(true));
            opMF(0x31, "read_idpy", "and");
            opMF(0x32, "read_idp", "and");
            opMF(0x33, "read_isry", "and");
            opMFI(0x34, "read_dpr", "bit", (int)OpCode.X);
            opMFI(0x35, "read_dpr", "and", (int)OpCode.X);
            opMF(0x36, "adjust_dpx", "rol");
            opMF(0x37, "read_ildpy", "and");
            opAII(0x38, "flag", 0x01, 0x01);
            opMF(0x39, "read_addry", "and");
            opMII(0x3a, "adjust_imm", (int)OpCode.A, -1);
            opAII(0x3b, "transfer_w", (int)OpCode.S, (int)OpCode.A);
            opMF(0x3c, "read_addrx", "bit");
            opMF(0x3d, "read_addrx", "and");
            opMF(0x3e, "adjust_addrx", "rol");
            opMF(0x3f, "read_longx", "and");
            opE(0x40, "rti");
            opMF(0x41, "read_idpx", "eor");
            opA(0x42, "wdm");
            opMF(0x43, "read_sr", "eor");
            opXI(0x44, "move", -1);
            opMF(0x45, "read_dp", "eor");
            opMF(0x46, "adjust_dp", "lsr");
            opMF(0x47, "read_ildp", "eor");
            opMI(0x48, "push", (int)OpCode.A);
            opMF(0x49, "read_const", "eor");
            opM(0x4a, "lsr_imm");
            opA(0x4b, "phk");
            opA(0x4c, "jmp_addr");
            opMF(0x4d, "read_addr", "eor");
            opMF(0x4e, "adjust_addr", "lsr");
            opMF(0x4f, "read_long", "eor");
            opAII(0x50, "branch", 0x40, Convert.ToInt32(false));
            opMF(0x51, "read_idpy", "eor");
            opMF(0x52, "read_idp", "eor");
            opMF(0x53, "read_isry", "eor");
            opXI(0x54, "move", +1);
            opMFI(0x55, "read_dpr", "eor", (int)OpCode.X);
            opMF(0x56, "adjust_dpx", "lsr");
            opMF(0x57, "read_ildpy", "eor");
            opAII(0x58, "flag", 0x04, 0x00);
            opMF(0x59, "read_addry", "eor");
            opXI(0x5a, "push", (int)OpCode.Y);
            opAII(0x5b, "transfer_w", (int)OpCode.A, (int)OpCode.D);
            opA(0x5c, "jmp_long");
            opMF(0x5d, "read_addrx", "eor");
            opMF(0x5e, "adjust_addrx", "lsr");
            opMF(0x5f, "read_longx", "eor");
            opA(0x60, "rts");
            opMF(0x61, "read_idpx", "adc");
            opE(0x62, "per");
            opMF(0x63, "read_sr", "adc");
            opMI(0x64, "write_dp", (int)OpCode.Z);
            opMF(0x65, "read_dp", "adc");
            opMF(0x66, "adjust_dp", "ror");
            opMF(0x67, "read_ildp", "adc");
            opMI(0x68, "pull", (int)OpCode.A);
            opMF(0x69, "read_const", "adc");
            opM(0x6a, "ror_imm");
            opE(0x6b, "rtl");
            opA(0x6c, "jmp_iaddr");
            opMF(0x6d, "read_addr", "adc");
            opMF(0x6e, "adjust_addr", "ror");
            opMF(0x6f, "read_long", "adc");
            opAII(0x70, "branch", 0x40, Convert.ToInt32(true));
            opMF(0x71, "read_idpy", "adc");
            opMF(0x72, "read_idp", "adc");
            opMF(0x73, "read_isry", "adc");
            opMII(0x74, "write_dpr", (int)OpCode.Z, (int)OpCode.X);
            opMFI(0x75, "read_dpr", "adc", (int)OpCode.X);
            opMF(0x76, "adjust_dpx", "ror");
            opMF(0x77, "read_ildpy", "adc");
            opAII(0x78, "flag", 0x04, 0x04);
            opMF(0x79, "read_addry", "adc");
            opXI(0x7a, "pull", (int)OpCode.Y);
            opAII(0x7b, "transfer_w", (int)OpCode.D, (int)OpCode.A);
            opA(0x7c, "jmp_iaddrx");
            opMF(0x7d, "read_addrx", "adc");
            opMF(0x7e, "adjust_addrx", "ror");
            opMF(0x7f, "read_longx", "adc");
            opA(0x80, "bra");
            opM(0x81, "sta_idpx");
            opA(0x82, "brl");
            opM(0x83, "sta_sr");
            opXI(0x84, "write_dp", (int)OpCode.Y);
            opMI(0x85, "write_dp", (int)OpCode.A);
            opXI(0x86, "write_dp", (int)OpCode.X);
            opM(0x87, "sta_ildp");
            opXII(0x88, "adjust_imm", (int)OpCode.Y, -1);
            opM(0x89, "read_bit_const");
            opMII(0x8a, "transfer", (int)OpCode.X, (int)OpCode.A);
            opA(0x8b, "phb");
            opXI(0x8c, "write_addr", (int)OpCode.Y);
            opMI(0x8d, "write_addr", (int)OpCode.A);
            opXI(0x8e, "write_addr", (int)OpCode.X);
            opMI(0x8f, "write_longr", (int)OpCode.Z);
            opAII(0x90, "branch", 0x01, Convert.ToInt32(false));
            opM(0x91, "sta_idpy");
            opM(0x92, "sta_idp");
            opM(0x93, "sta_isry");
            opXII(0x94, "write_dpr", (int)OpCode.Y, (int)OpCode.X);
            opMII(0x95, "write_dpr", (int)OpCode.A, (int)OpCode.X);
            opXII(0x96, "write_dpr", (int)OpCode.X, (int)OpCode.Y);
            opM(0x97, "sta_ildpy");
            opMII(0x98, "transfer", (int)OpCode.Y, (int)OpCode.A);
            opMII(0x99, "write_addrr", (int)OpCode.A, (int)OpCode.Y);
            opE(0x9a, "txs");
            opXII(0x9b, "transfer", (int)OpCode.X, (int)OpCode.Y);
            opMI(0x9c, "write_addr", (int)OpCode.Z);
            opMII(0x9d, "write_addrr", (int)OpCode.A, (int)OpCode.X);
            opMII(0x9e, "write_addrr", (int)OpCode.Z, (int)OpCode.X);
            opMI(0x9f, "write_longr", (int)OpCode.X);
            opXF(0xa0, "read_const", "ldy");
            opMF(0xa1, "read_idpx", "lda");
            opXF(0xa2, "read_const", "ldx");
            opMF(0xa3, "read_sr", "lda");
            opXF(0xa4, "read_dp", "ldy");
            opMF(0xa5, "read_dp", "lda");
            opXF(0xa6, "read_dp", "ldx");
            opMF(0xa7, "read_ildp", "lda");
            opXII(0xa8, "transfer", (int)OpCode.A, (int)OpCode.Y);
            opMF(0xa9, "read_const", "lda");
            opXII(0xaa, "transfer", (int)OpCode.A, (int)OpCode.X);
            opA(0xab, "plb");
            opXF(0xac, "read_addr", "ldy");
            opMF(0xad, "read_addr", "lda");
            opXF(0xae, "read_addr", "ldx");
            opMF(0xaf, "read_long", "lda");
            opAII(0xb0, "branch", 0x01, Convert.ToInt32(true));
            opMF(0xb1, "read_idpy", "lda");
            opMF(0xb2, "read_idp", "lda");
            opMF(0xb3, "read_isry", "lda");
            opXFI(0xb4, "read_dpr", "ldy", (int)OpCode.X);
            opMFI(0xb5, "read_dpr", "lda", (int)OpCode.X);
            opXFI(0xb6, "read_dpr", "ldx", (int)OpCode.Y);
            opMF(0xb7, "read_ildpy", "lda");
            opAII(0xb8, "flag", 0x40, 0x00);
            opMF(0xb9, "read_addry", "lda");
            opX(0xba, "tsx");
            opXII(0xbb, "transfer", (int)OpCode.Y, (int)OpCode.X);
            opXF(0xbc, "read_addrx", "ldy");
            opMF(0xbd, "read_addrx", "lda");
            opXF(0xbe, "read_addry", "ldx");
            opMF(0xbf, "read_longx", "lda");
            opXF(0xc0, "read_const", "cpy");
            opMF(0xc1, "read_idpx", "cmp");
            opEI(0xc2, "pflag", 0);
            opMF(0xc3, "read_sr", "cmp");
            opXF(0xc4, "read_dp", "cpy");
            opMF(0xc5, "read_dp", "cmp");
            opMF(0xc6, "adjust_dp", "dec");
            opMF(0xc7, "read_ildp", "cmp");
            opXII(0xc8, "adjust_imm", (int)OpCode.Y, +1);
            opMF(0xc9, "read_const", "cmp");
            opXII(0xca, "adjust_imm", (int)OpCode.X, -1);
            opA(0xcb, "wai");
            opXF(0xcc, "read_addr", "cpy");
            opMF(0xcd, "read_addr", "cmp");
            opMF(0xce, "adjust_addr", "dec");
            opMF(0xcf, "read_long", "cmp");
            opAII(0xd0, "branch", 0x02, Convert.ToInt32(false));
            opMF(0xd1, "read_idpy", "cmp");
            opMF(0xd2, "read_idp", "cmp");
            opMF(0xd3, "read_isry", "cmp");
            opE(0xd4, "pei");
            opMFI(0xd5, "read_dpr", "cmp", (int)OpCode.X);
            opMF(0xd6, "adjust_dpx", "dec");
            opMF(0xd7, "read_ildpy", "cmp");
            opAII(0xd8, "flag", 0x08, 0x00);
            opMF(0xd9, "read_addry", "cmp");
            opXI(0xda, "push", (int)OpCode.X);
            opA(0xdb, "stp");
            opA(0xdc, "jmp_iladdr");
            opMF(0xdd, "read_addrx", "cmp");
            opMF(0xde, "adjust_addrx", "dec");
            opMF(0xdf, "read_longx", "cmp");
            opXF(0xe0, "read_const", "cpx");
            opMF(0xe1, "read_idpx", "sbc");
            opEI(0xe2, "pflag", 1);
            opMF(0xe3, "read_sr", "sbc");
            opXF(0xe4, "read_dp", "cpx");
            opMF(0xe5, "read_dp", "sbc");
            opMF(0xe6, "adjust_dp", "inc");
            opMF(0xe7, "read_ildp", "sbc");
            opXII(0xe8, "adjust_imm", (int)OpCode.X, +1);
            opMF(0xe9, "read_const", "sbc");
            opA(0xea, "nop");
            opA(0xeb, "xba");
            opXF(0xec, "read_addr", "cpx");
            opMF(0xed, "read_addr", "sbc");
            opMF(0xee, "adjust_addr", "inc");
            opMF(0xef, "read_long", "sbc");
            opAII(0xf0, "branch", 0x02, Convert.ToInt32(true));
            opMF(0xf1, "read_idpy", "sbc");
            opMF(0xf2, "read_idp", "sbc");
            opMF(0xf3, "read_isry", "sbc");
            opE(0xf4, "pea");
            opMFI(0xf5, "read_dpr", "sbc", (int)OpCode.X);
            opMF(0xf6, "adjust_dpx", "inc");
            opMF(0xf7, "read_ildpy", "sbc");
            opAII(0xf8, "flag", 0x08, 0x08);
            opMF(0xf9, "read_addry", "sbc");
            opXI(0xfa, "pull", (int)OpCode.X);
            opA(0xfb, "xce");
            opE(0xfc, "jsr_iaddrx");
            opMF(0xfd, "read_addrx", "sbc");
            opMF(0xfe, "adjust_addrx", "inc");
            opMF(0xff, "read_longx", "sbc");
        }

        public void update_table()
        {
            if (regs.e)
            {
                opcode_table = new ArraySegment<CPUCoreOperation>(op_table, (int)Table.EM, op_table.Length - (int)Table.EM);
            }
            else if (regs.p.m)
            {
                if (regs.p.x)
                {
                    opcode_table = new ArraySegment<CPUCoreOperation>(op_table, (int)Table.MX, op_table.Length - (int)Table.MX);
                }
                else
                {
                    opcode_table = new ArraySegment<CPUCoreOperation>(op_table, (int)Table.Mx, op_table.Length - (int)Table.Mx);
                }
            }
            else
            {
                if (regs.p.x)
                {
                    opcode_table = new ArraySegment<CPUCoreOperation>(op_table, (int)Table.mX, op_table.Length - (int)Table.mX);
                }
                else
                {
                    opcode_table = new ArraySegment<CPUCoreOperation>(op_table, (int)Table.mx, op_table.Length - (int)Table.mx);
                }
            }
        }

        public enum Table { EM = 0, MX = 256, Mx = 512, mX = 768, mx = 1024 }
        private enum OpCode { A = 0, X = 1, Y = 2, Z = 3, S = 4, D = 5 };

        public void core_serialize(Serializer s)
        {
            s.integer(regs.pc.d, "regs.pc.d");

            s.integer(regs.a.w, "regs.a.w");
            s.integer(regs.x.w, "regs.x.w");
            s.integer(regs.y.w, "regs.y.w");
            s.integer(regs.z.w, "regs.z.w");
            s.integer(regs.s.w, "regs.s.w");
            s.integer(regs.d.w, "regs.d.w");

            s.integer(regs.p.n, "regs.p.n");
            s.integer(regs.p.v, "regs.p.v");
            s.integer(regs.p.m, "regs.p.m");
            s.integer(regs.p.x, "regs.p.x");
            s.integer(regs.p.d, "regs.p.d");
            s.integer(regs.p.i, "regs.p.i");
            s.integer(regs.p.z, "regs.p.z");
            s.integer(regs.p.c, "regs.p.c");

            s.integer(regs.db, "regs.db");
            s.integer(regs.e, "regs.e");
            s.integer(regs.irq, "regs.irq");
            s.integer(regs.wai, "regs.wai");
            s.integer(regs.mdr, "regs.mdr");

            s.integer(aa.d, "aa.d");
            s.integer(rd.d, "rd.d");
            s.integer(sp, "sp");
            s.integer(dp, "dp");

            update_table();
        }

        public CPUCore()
        {
            initialize_opcode_table();
        }
    }
}
