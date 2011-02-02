using System;
using System.Collections;
using Nall;

namespace Snes
{
    abstract partial class SMPCore
    {
        public IEnumerable op_readpc(Result result)
        {
            foreach (var e in op_read(regs.pc++, result))
            {
                yield return e;
            };
        }

        public IEnumerable op_readstack(Result result)
        {
            foreach (var e in op_read((ushort)(0x0100 | ++regs.sp.Array[regs.sp.Offset]), result))
            {
                yield return e;
            };
        }

        public IEnumerable op_writestack(byte data)
        {
            foreach (var e in op_write((ushort)(0x0100 | regs.sp.Array[regs.sp.Offset]--), data))
            {
                yield return e;
            };
        }

        public IEnumerable op_readaddr(ushort addr, Result result)
        {
            foreach (var e in op_read(addr, result))
            {
                yield return e;
            };
        }

        public IEnumerable op_writeaddr(ushort addr, byte data)
        {
            foreach (var e in op_write(addr, data))
            {
                yield return e;
            };
        }

        public IEnumerable op_readdp(byte addr, Result result)
        {
            foreach (var e in op_read((ushort)((Convert.ToUInt32(regs.p.p) << 8) + addr), result))
            {
                yield return e;
            };
        }

        public IEnumerable op_writedp(byte addr, byte data)
        {
            foreach (var e in op_write((ushort)((Convert.ToUInt32(regs.p.p) << 8) + addr), data))
            {
                yield return e;
            };
        }

        public Regs regs = new Regs();
        public ushort dp, sp, rd, wr, bit, ya;
        public enum OpCode { A = 0, X = 1, Y = 2, SP = 3 }

        protected Result _result = new Result();
        protected SMPCoreOpArgument _argument = new SMPCoreOpArgument();

        public abstract IEnumerable op_io();
        public abstract IEnumerable op_read(ushort addr, Result result);
        public abstract IEnumerable op_write(ushort addr, byte data);

        public IEnumerable op_adc(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            int r = args.x_byte + args.y_byte + Convert.ToInt32(regs.p.c);
            regs.p.n = Convert.ToBoolean(r & 0x80);
            regs.p.v = Convert.ToBoolean(~(args.x_byte ^ args.y_byte) & (args.x_byte ^ r) & 0x80);
            regs.p.h = Convert.ToBoolean((args.x_byte ^ args.y_byte ^ r) & 0x10);
            regs.p.z = (byte)r == 0;
            regs.p.c = r > 0xff;
            result.ByteValue = (byte)r;
            yield break;
        }

        public IEnumerable op_addw(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            ushort r;
            regs.p.c = Convert.ToBoolean(0);
            _argument.x_byte = (byte)args.x_ushort;
            _argument.y_byte = (byte)args.y_ushort;
            foreach (var e in op_adc(_argument, result))
            {
                yield return e;
            };
            r = result.ByteValue;
            _argument.x_byte = (byte)(args.x_ushort >> 8);
            _argument.y_byte = (byte)(args.y_ushort >> 8);
            foreach (var e in op_adc(_argument, result))
            {
                yield return e;
            };
            r |= (ushort)(result.ByteValue << 8);
            regs.p.z = r == 0;
            result.UShortValue = r;
        }

        public IEnumerable op_and(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            args.x_byte &= args.y_byte;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            result.ByteValue = args.x_byte;
            yield break;
        }

        public IEnumerable op_cmp(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            int r = args.x_byte - args.y_byte;
            regs.p.n = Convert.ToBoolean(r & 0x80);
            regs.p.z = (byte)r == 0;
            regs.p.c = r >= 0;
            result.ByteValue = args.x_byte;
            yield break;
        }

        public IEnumerable op_cmpw(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            int r = args.x_ushort - args.y_ushort;
            regs.p.n = Convert.ToBoolean(r & 0x8000);
            regs.p.z = (ushort)r == 0;
            regs.p.c = r >= 0;
            result.UShortValue = args.x_ushort;
            yield break;
        }

        public IEnumerable op_eor(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            args.x_byte ^= args.y_byte;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            result.ByteValue = args.x_byte;
            yield break;
        }

        public IEnumerable op_inc(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            args.x_byte++;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            result.ByteValue = args.x_byte;
            yield break;
        }

        public IEnumerable op_dec(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            args.x_byte--;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            result.ByteValue = args.x_byte;
            yield break;
        }

        public IEnumerable op_or(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            args.x_byte |= args.y_byte;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            result.ByteValue = args.x_byte;
            yield break;
        }

        public IEnumerable op_sbc(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            int r = args.x_byte - args.y_byte - Convert.ToInt32(!regs.p.c);
            regs.p.n = Convert.ToBoolean(r & 0x80);
            regs.p.v = Convert.ToBoolean((args.x_byte ^ args.y_byte) & (args.x_byte ^ r) & 0x80);
            regs.p.h = !Convert.ToBoolean(((args.x_byte ^ args.y_byte ^ r) & 0x10));
            regs.p.z = (byte)r == 0;
            regs.p.c = r >= 0;
            result.ByteValue = (byte)r;
            yield break;
        }

        public IEnumerable op_subw(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            ushort r;
            regs.p.c = Convert.ToBoolean(1);
            _argument.x_byte = (byte)args.x_ushort;
            _argument.y_byte = (byte)args.y_ushort;
            foreach (var e in op_sbc(_argument, result))
            {
                yield return e;
            };
            r = result.ByteValue;
            _argument.x_byte = (byte)(args.x_ushort >> 8);
            _argument.y_byte = (byte)(args.y_ushort >> 8);
            foreach (var e in op_sbc(_argument, result))
            {
                yield return e;
            };
            r |= (ushort)(result.ByteValue << 8);
            regs.p.z = r == 0;
            result.UShortValue = r;
        }

        public IEnumerable op_asl(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            regs.p.c = Convert.ToBoolean(args.x_byte & 0x80);
            args.x_byte <<= 1;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            result.ByteValue = args.x_byte;
            yield break;
        }

        public IEnumerable op_lsr(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            regs.p.c = Convert.ToBoolean(args.x_byte & 0x01);
            args.x_byte >>= 1;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            result.ByteValue = args.x_byte;
            yield break;
        }

        public IEnumerable op_rol(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            uint carry = Convert.ToUInt32(regs.p.c);
            regs.p.c = Convert.ToBoolean(args.x_byte & 0x80);
            args.x_byte = (byte)((uint)(args.x_byte << 1) | carry);
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            result.ByteValue = args.x_byte;
            yield break;
        }

        public IEnumerable op_ror(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            uint carry = Convert.ToUInt32(regs.p.c) << 7;
            regs.p.c = Convert.ToBoolean(args.x_byte & 0x01);
            args.x_byte = (byte)(carry | (uint)(args.x_byte >> 1));
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            result.ByteValue = args.x_byte;
            yield break;
        }

        public IEnumerable op_mov_reg_reg(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.r[args.to] = regs.r[args.from];
            regs.p.n = Convert.ToBoolean(regs.r[args.to] & 0x80);
            regs.p.z = (regs.r[args.to] == 0);
        }

        public IEnumerable op_mov_sp_x(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.sp.Array[regs.sp.Offset] = regs.x.Array[regs.x.Offset];
        }

        public IEnumerable op_mov_reg_const(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            regs.r[args.n] = _result.Value;
            regs.p.n = Convert.ToBoolean(regs.r[args.n] & 0x80);
            regs.p.z = (regs.r[args.n] == 0);
        }

        public IEnumerable op_mov_a_ix(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp(regs.x.Array[regs.x.Offset], _result))
            {
                yield return e;
            };
            regs.a.Array[regs.a.Offset] = _result.Value;
            regs.p.n = Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80);
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
        }

        public IEnumerable op_mov_a_ixinc(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp(regs.x.Array[regs.x.Offset]++, _result))
            {
                yield return e;
            };
            regs.a.Array[regs.a.Offset] = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.p.n = Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80);
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
        }

        public IEnumerable op_mov_reg_dp(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_readdp((byte)sp, _result))
            {
                yield return e;
            };
            regs.r[args.n] = _result.Value;
            regs.p.n = Convert.ToBoolean(regs.r[args.n] & 0x80);
            regs.p.z = (regs.r[args.n] == 0);
        }

        public IEnumerable op_mov_reg_dpr(SMPCoreOpArgument args, SMPCoreOpResult result)
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
            foreach (var e in op_readdp((byte)(sp + regs.r[args.i]), _result))
            {
                yield return e;
            };
            regs.r[args.n] = _result.Value;
            regs.p.n = Convert.ToBoolean(regs.r[args.n] & 0x80);
            regs.p.z = (regs.r[args.n] == 0);
        }

        public IEnumerable op_mov_reg_addr(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp |= (ushort)(_result.Value << 8);
            foreach (var e in op_readaddr(sp, _result))
            {
                yield return e;
            };
            regs.r[args.n] = _result.Value;
            regs.p.n = Convert.ToBoolean(regs.r[args.n] & 0x80);
            regs.p.z = (regs.r[args.n] == 0);
        }

        public IEnumerable op_mov_a_addrr(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp |= (ushort)(_result.Value << 8);
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readaddr((ushort)(sp + regs.r[args.i]), _result))
            {
                yield return e;
            };
            regs.a.Array[regs.a.Offset] = _result.Value;
            regs.p.n = Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80);
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
        }

        public IEnumerable op_mov_a_idpx(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value + regs.x.Array[regs.x.Offset]);
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((byte)(dp + 0), _result))
            {
                yield return e;
            };
            sp = (ushort)(_result.Value << 0);
            foreach (var e in op_readdp((byte)(dp + 1), _result))
            {
                yield return e;
            };
            sp |= (ushort)(_result.Value << 8);
            foreach (var e in op_readaddr(sp, _result))
            {
                yield return e;
            };
            regs.a.Array[regs.a.Offset] = _result.Value;
            regs.p.n = Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80);
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
        }

        public IEnumerable op_mov_a_idpy(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((byte)(dp + 0), _result))
            {
                yield return e;
            };
            sp = (ushort)(_result.Value << 0);
            foreach (var e in op_readdp((byte)(dp + 1), _result))
            {
                yield return e;
            };
            sp |= (ushort)(_result.Value << 8);
            foreach (var e in op_readaddr((ushort)(sp + regs.y.Array[regs.y.Offset]), _result))
            {
                yield return e;
            };
            regs.a.Array[regs.a.Offset] = _result.Value;
            regs.p.n = Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80);
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
        }

        public IEnumerable op_mov_dp_dp(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_readdp((byte)sp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_writedp((byte)dp, (byte)rd))
            {
                yield return e;
            };
        }

        public IEnumerable op_mov_dp_const(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            foreach (var e in op_writedp((byte)dp, (byte)rd))
            {
                yield return e;
            };
        }

        public IEnumerable op_mov_ix_a(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp(regs.x.Array[regs.x.Offset], _result))
            {
                yield return e;
            };
            foreach (var e in op_writedp(regs.x.Array[regs.x.Offset], regs.a.Array[regs.a.Offset]))
            {
                yield return e;
            };
        }

        public IEnumerable op_mov_ixinc_a(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writedp(regs.x.Array[regs.x.Offset]++, regs.a.Array[regs.a.Offset]))
            {
                yield return e;
            };
        }

        public IEnumerable op_mov_dp_reg(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            foreach (var e in op_writedp((byte)dp, regs.r[args.n]))
            {
                yield return e;
            };
        }

        public IEnumerable op_mov_dpr_reg(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            dp += regs.r[args.i];
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            foreach (var e in op_writedp((byte)dp, regs.r[args.n]))
            {
                yield return e;
            };
        }

        public IEnumerable op_mov_addr_reg(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            foreach (var e in op_readaddr(dp, _result))
            {
                yield return e;
            };
            op_writeaddr(dp, regs.r[args.n]);
        }

        public IEnumerable op_mov_addrr_a(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            foreach (var e in op_io())
            {
                yield return e;
            };
            dp += regs.r[args.i];
            foreach (var e in op_readaddr(dp, _result))
            {
                yield return e;
            };
            op_writeaddr(dp, regs.a.Array[regs.a.Offset]);
        }

        public IEnumerable op_mov_idpx_a(SMPCoreOpArgument args, SMPCoreOpResult result)
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
            sp += regs.x.Array[regs.x.Offset];
            foreach (var e in op_readdp((byte)(sp + 0), _result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readdp((byte)(sp + 1), _result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            foreach (var e in op_readaddr(dp, _result))
            {
                yield return e;
            };
            op_writeaddr(dp, regs.a.Array[regs.a.Offset]);
        }

        public IEnumerable op_mov_idpy_a(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_readdp((byte)(sp + 0), _result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readdp((byte)(sp + 1), _result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            foreach (var e in op_io())
            {
                yield return e;
            };
            dp += regs.y.Array[regs.y.Offset];
            foreach (var e in op_readaddr(dp, _result))
            {
                yield return e;
            };
            op_writeaddr(dp, regs.a.Array[regs.a.Offset]);
        }

        public IEnumerable op_movw_ya_dp(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_readdp((byte)(sp + 0), _result))
            {
                yield return e;
            };
            regs.a.Array[regs.a.Offset] = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((byte)(sp + 1), _result))
            {
                yield return e;
            };
            regs.y.Array[regs.y.Offset] = _result.Value;
            regs.p.n = Convert.ToBoolean((ushort)regs.ya & 0x8000);
            regs.p.z = ((ushort)regs.ya == 0);
        }

        public IEnumerable op_movw_dp_ya(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            foreach (var e in op_writedp((byte)(dp + 0), regs.a.Array[regs.a.Offset]))
            {
                yield return e;
            };
            foreach (var e in op_writedp((byte)(dp + 1), regs.y.Array[regs.y.Offset]))
            {
                yield return e;
            };
        }

        public IEnumerable op_mov1_c_bit(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp |= (ushort)(_result.Value << 8);
            bit = (ushort)(sp >> 13);
            sp &= 0x1fff;
            foreach (var e in op_readaddr(sp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            regs.p.c = Convert.ToBoolean(rd & (1 << bit));
        }

        public IEnumerable op_mov1_bit_c(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            bit = (ushort)(dp >> 13);
            dp &= 0x1fff;
            foreach (var e in op_readaddr(dp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            if (regs.p.c)
            {
                rd |= (ushort)(1 << bit);
            }
            else
            {
                rd &= (ushort)(~(1 << bit));
            }
            foreach (var e in op_io())
            {
                yield return e;
            };
            op_writeaddr((byte)dp, (byte)rd);
        }

        public IEnumerable op_bra(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.pc += (ushort)((sbyte)rd);
        }

        public IEnumerable op_branch(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd = _result.Value;
            if (Convert.ToInt32(Convert.ToBoolean((uint)regs.p & args.flag)) != args.value)
            {
                yield break;
            }
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.pc += (ushort)((sbyte)rd);
        }

        public IEnumerable op_bitbranch(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            if (Convert.ToInt32(Convert.ToBoolean(sp & args.mask)) != args.value)
            {
                yield break;
            }
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.pc += (ushort)((sbyte)rd);
        }

        public IEnumerable op_cbne_dp(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            if (regs.a.Array[regs.a.Offset] == sp)
            {
                yield break;
            }
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.pc += (ushort)((sbyte)rd);
        }

        public IEnumerable op_cbne_dpx(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((byte)(dp + regs.x.Array[regs.x.Offset]), _result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            if (regs.a.Array[regs.a.Offset] == sp)
            {
                yield break;
            }
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.pc += (ushort)((sbyte)rd);
        }

        public IEnumerable op_dbnz_dp(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            wr = _result.Value;
            foreach (var e in op_writedp((byte)dp, (byte)--wr))
            {
                yield return e;
            };
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd = _result.Value;
            if (wr == 0)
            {
                yield break;
            }
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.pc += (ushort)((sbyte)rd);
        }

        public IEnumerable op_dbnz_y(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.y.Array[regs.y.Offset]--;
            foreach (var e in op_io())
            {
                yield return e;
            };
            if (regs.y.Array[regs.y.Offset] == 0)
            {
                yield break;
            }
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.pc += (ushort)((sbyte)rd);
        }

        public IEnumerable op_jmp_addr(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd |= (ushort)(_result.Value << 8);
            regs.pc = rd;
        }

        public IEnumerable op_jmp_iaddrx(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            foreach (var e in op_io())
            {
                yield return e;
            };
            dp += regs.x.Array[regs.x.Offset];
            foreach (var e in op_readaddr((ushort)(dp + 0), _result))
            {
                yield return e;
            };
            rd = (ushort)(_result.Value << 0);
            foreach (var e in op_readaddr((ushort)(dp + 1), _result))
            {
                yield return e;
            };
            rd |= (ushort)(_result.Value << 8);
            regs.pc = rd;
        }

        public IEnumerable op_call(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd |= (ushort)(_result.Value << 8);
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writestack((byte)(regs.pc >> 8)))
            {
                yield return e;
            };
            foreach (var e in op_writestack((byte)(regs.pc >> 0)))
            {
                yield return e;
            };
            regs.pc = rd;
        }

        public IEnumerable op_pcall(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writestack((byte)(regs.pc >> 8)))
            {
                yield return e;
            };
            foreach (var e in op_writestack((byte)(regs.pc >> 0)))
            {
                yield return e;
            };
            regs.pc = (ushort)(0xff00 | rd);
        }

        public IEnumerable op_tcall(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            dp = (ushort)(0xffde - (args.n << 1));
            foreach (var e in op_readaddr((ushort)(dp + 0), _result))
            {
                yield return e;
            };
            rd = (ushort)(_result.Value << 0);
            foreach (var e in op_readaddr((ushort)(dp + 1), _result))
            {
                yield return e;
            };
            rd |= (ushort)(_result.Value << 8);
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writestack((byte)(regs.pc >> 8)))
            {
                yield return e;
            };
            foreach (var e in op_writestack((byte)(regs.pc >> 0)))
            {
                yield return e;
            };
            regs.pc = rd;
        }

        public IEnumerable op_brk(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readaddr(0xffde, _result))
            {
                yield return e;
            };
            rd = (ushort)(_result.Value << 0);
            foreach (var e in op_readaddr(0xffdf, _result))
            {
                yield return e;
            };
            rd |= (ushort)(_result.Value << 8);
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writestack((byte)(regs.pc >> 8)))
            {
                yield return e;
            };
            foreach (var e in op_writestack((byte)(regs.pc >> 0)))
            {
                yield return e;
            };
            foreach (var e in op_writestack((byte)(regs.p)))
            {
                yield return e;
            };
            regs.pc = rd;
            regs.p.b = Convert.ToBoolean(1);
            regs.p.i = Convert.ToBoolean(0);
        }

        public IEnumerable op_ret(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            rd = (ushort)(_result.Value << 0);
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            rd |= (ushort)(_result.Value << 8);
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.pc = rd;
        }

        public IEnumerable op_reti(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            regs.p.Assign(_result.Value);
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            rd = (ushort)(_result.Value << 0);
            foreach (var e in op_readstack(_result))
            {
                yield return e;
            };
            rd |= (ushort)(_result.Value << 8);
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.pc = rd;
        }

        public IEnumerable op_read_reg_const(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd = _result.Value;
            _argument.x_byte = regs.r[args.n];
            _argument.y_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            regs.r[args.n] = result.ByteValue;
        }

        public IEnumerable op_read_a_ix(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp(regs.x.Array[regs.x.Offset], _result))
            {
                yield return e;
            };
            rd = _result.Value;
            _argument.x_byte = regs.a.Array[regs.a.Offset];
            _argument.y_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            regs.a.Array[regs.a.Offset] = result.ByteValue;
        }

        public IEnumerable op_read_reg_dp(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            _argument.x_byte = regs.r[args.n];
            _argument.y_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            regs.r[args.n] = result.ByteValue;
        }

        public IEnumerable op_read_a_dpx(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((byte)(dp + regs.x.Array[regs.x.Offset]), _result))
            {
                yield return e;
            };
            rd = _result.Value;
            _argument.x_byte = regs.a.Array[regs.a.Offset];
            _argument.y_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            regs.a.Array[regs.a.Offset] = result.ByteValue;
        }

        public IEnumerable op_read_reg_addr(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            foreach (var e in op_readaddr(dp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            _argument.x_byte = regs.r[args.n];
            _argument.y_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            regs.r[args.n] = result.ByteValue;
        }

        public IEnumerable op_read_a_addrr(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readaddr((ushort)(dp + regs.r[args.i]), _result))
            {
                yield return e;
            };
            rd = _result.Value;
            _argument.x_byte = regs.a.Array[regs.a.Offset];
            _argument.y_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            regs.a.Array[regs.a.Offset] = result.ByteValue;
        }

        public IEnumerable op_read_a_idpx(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value + regs.x.Array[regs.x.Offset]);
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((byte)(dp + 0), _result))
            {
                yield return e;
            };
            sp = (ushort)(_result.Value << 0);
            foreach (var e in op_readdp((byte)(dp + 1), _result))
            {
                yield return e;
            };
            sp |= (ushort)(_result.Value << 8);
            foreach (var e in op_readaddr(sp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            _argument.x_byte = regs.a.Array[regs.a.Offset];
            _argument.y_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            regs.a.Array[regs.a.Offset] = result.ByteValue;
        }

        public IEnumerable op_read_a_idpy(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((byte)(dp + 0), _result))
            {
                yield return e;
            };
            sp = (ushort)(_result.Value << 0);
            foreach (var e in op_readdp((byte)(dp + 1), _result))
            {
                yield return e;
            };
            sp |= (ushort)(_result.Value << 8);
            foreach (var e in op_readaddr((ushort)(sp + regs.y.Array[regs.y.Offset]), _result))
            {
                yield return e;
            };
            rd = _result.Value;
            _argument.x_byte = regs.a.Array[regs.a.Offset];
            _argument.y_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            regs.a.Array[regs.a.Offset] = result.ByteValue;
        }

        public IEnumerable op_read_ix_iy(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp(regs.y.Array[regs.y.Offset], _result))
            {
                yield return e;
            };
            rd = _result.Value;
            foreach (var e in op_readdp(regs.x.Array[regs.x.Offset], _result))
            {
                yield return e;
            };
            wr = _result.Value;
            _argument.x_byte = (byte)wr;
            _argument.y_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            wr = result.ByteValue;
            SMPCoreOp cmp = op_cmp;
            if (args.op_func != cmp)
            {
                foreach (var e in op_writedp(regs.x.Array[regs.x.Offset], (byte)wr))
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

        public IEnumerable op_read_dp_dp(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            sp = _result.Value;
            foreach (var e in op_readdp((byte)sp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            wr = _result.Value;
            _argument.x_byte = (byte)wr;
            _argument.y_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            wr = result.ByteValue;
            SMPCoreOp cmp = op_cmp;
            if (args.op_func != cmp)
            {
                foreach (var e in op_writedp((byte)dp, (byte)wr))
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

        public IEnumerable op_read_dp_const(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            rd = _result.Value;
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            wr = _result.Value;
            _argument.x_byte = (byte)wr;
            _argument.y_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            wr = result.ByteValue;
            SMPCoreOp cmp = op_cmp;
            if (args.op_func != cmp)
            {
                foreach (var e in op_writedp((byte)dp, (byte)wr))
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

        public IEnumerable op_read_ya_dp(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)(dp + 0), _result))
            {
                yield return e;
            };
            rd = (ushort)(_result.Value << 0);
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((byte)(dp + 1), _result))
            {
                yield return e;
            };
            rd |= (ushort)(_result.Value << 8);
            _argument.x_ushort = (ushort)regs.ya;
            _argument.y_ushort = rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            regs.ya.Assign(result.UShortValue);
        }

        public IEnumerable op_cmpw_ya_dp(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)(dp + 0), _result))
            {
                yield return e;
            };
            rd = (ushort)(_result.Value << 0);
            foreach (var e in op_readdp((byte)(dp + 1), _result))
            {
                yield return e;
            };
            rd |= (ushort)(_result.Value << 8);
            _argument.x_ushort = (ushort)regs.ya;
            _argument.y_ushort = rd;
            foreach (var e in op_cmpw(_argument, result))
            {
                yield return e;
            };
        }

        public IEnumerable op_and1_bit(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            bit = (ushort)(dp >> 13);
            dp &= 0x1fff;
            foreach (var e in op_readaddr(dp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            regs.p.c = Convert.ToBoolean(Convert.ToInt32(regs.p.c) & (Convert.ToInt32(Convert.ToBoolean(rd & (1 << bit))) ^ args.op));
        }

        public IEnumerable op_eor1_bit(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            bit = (ushort)(dp >> 13);
            dp &= 0x1fff;
            foreach (var e in op_readaddr(dp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.p.c = Convert.ToBoolean(Convert.ToInt32(regs.p.c) ^ Convert.ToInt32(Convert.ToBoolean(rd & (1 << bit))));
        }

        public IEnumerable op_not1_bit(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            bit = (ushort)(dp >> 13);
            dp &= 0x1fff;
            foreach (var e in op_readaddr(dp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            rd ^= (ushort)(1 << bit);
            op_writeaddr(dp, (byte)rd);
        }

        public IEnumerable op_or1_bit(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            bit = (ushort)(dp >> 13);
            dp &= 0x1fff;
            foreach (var e in op_readaddr(dp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.p.c = Convert.ToBoolean(Convert.ToInt32(regs.p.c) | (Convert.ToInt32(Convert.ToBoolean(rd & (1 << bit))) ^ args.op));
        }

        public IEnumerable op_adjust_reg(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            _argument.x_byte = regs.r[args.n];
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            regs.r[args.n] = result.ByteValue;
        }

        public IEnumerable op_adjust_dp(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            _argument.x_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            rd = result.ByteValue;
            foreach (var e in op_writedp((byte)dp, (byte)rd))
            {
                yield return e;
            };
        }

        public IEnumerable op_adjust_dpx(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_readdp((byte)(dp + regs.x.Array[regs.x.Offset]), _result))
            {
                yield return e;
            };
            rd = _result.Value;
            _argument.x_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            rd = result.ByteValue;
            foreach (var e in op_writedp((byte)(dp + regs.x.Array[regs.x.Offset]), (byte)rd))
            {
                yield return e;
            };
        }

        public IEnumerable op_adjust_addr(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            foreach (var e in op_readaddr(dp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            _argument.x_byte = (byte)rd;
            foreach (var e in args.op_func.Invoke(_argument, result))
            {
                yield return e;
            };
            rd = result.ByteValue;
            op_writeaddr(dp, (byte)rd);
        }

        public IEnumerable op_adjust_addr_a(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = (ushort)(_result.Value << 0);
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp |= (ushort)(_result.Value << 8);
            foreach (var e in op_readaddr(dp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            regs.p.n = Convert.ToBoolean((regs.a.Array[regs.a.Offset] - rd) & 0x80);
            regs.p.z = ((regs.a.Array[regs.a.Offset] - rd) == 0);
            foreach (var e in op_readaddr(dp, _result))
            {
                yield return e;
            };
            op_writeaddr(dp, (byte)(Convert.ToBoolean(args.op) ? rd | regs.a.Array[regs.a.Offset] : rd & ~regs.a.Array[regs.a.Offset]));
        }

        public IEnumerable op_adjustw_dp(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            rd = (ushort)(_result.Value << 0);
            rd += (ushort)args.adjust;
            foreach (var e in op_writedp((byte)(dp++), (byte)rd))
            {
                yield return e;
            };
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            rd += (ushort)(_result.Value << 8);
            foreach (var e in op_writedp((byte)dp, (byte)(rd >> 8)))
            {
                yield return e;
            };
            regs.p.n = Convert.ToBoolean(rd & 0x8000);
            regs.p.z = (rd == 0);
        }

        public IEnumerable op_nop(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
        }

        public IEnumerable op_wait(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            while (true)
            {
                foreach (var e in op_io())
                {
                    yield return e;
                };
                foreach (var e in op_io())
                {
                    yield return e;
                };
            }
        }

        public IEnumerable op_xcn(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.a.Array[regs.a.Offset] = (byte)((regs.a.Array[regs.a.Offset] >> 4) | (regs.a.Array[regs.a.Offset] << 4));
            regs.p.n = Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80);
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
        }

        public IEnumerable op_daa(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            if (regs.p.c || (regs.a.Array[regs.a.Offset]) > 0x99)
            {
                regs.a.Array[regs.a.Offset] += 0x60;
                regs.p.c = Convert.ToBoolean(1);
            }
            if (regs.p.h || (regs.a.Array[regs.a.Offset] & 15) > 0x09)
            {
                regs.a.Array[regs.a.Offset] += 0x06;
            }
            regs.p.n = Convert.ToBoolean(Convert.ToInt32(Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80)));
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
        }

        public IEnumerable op_das(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            if (!regs.p.c || (regs.a.Array[regs.a.Offset]) > 0x99)
            {
                regs.a.Array[regs.a.Offset] -= 0x60;
                regs.p.c = Convert.ToBoolean(0);
            }
            if (!regs.p.h || (regs.a.Array[regs.a.Offset] & 15) > 0x09)
            {
                regs.a.Array[regs.a.Offset] -= 0x06;
            }
            regs.p.n = Convert.ToBoolean(Convert.ToInt32(Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80)));
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
        }

        public IEnumerable op_setbit(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.p.Assign((byte)(((uint)regs.p & ~args.mask) | (uint)args.value));
        }

        public IEnumerable op_notc(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.p.c = !regs.p.c;
        }

        public IEnumerable op_seti(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            regs.p.i = Convert.ToBoolean(args.value);
        }

        public IEnumerable op_setbit_dp(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_readpc(_result))
            {
                yield return e;
            };
            dp = _result.Value;
            foreach (var e in op_readdp((byte)dp, _result))
            {
                yield return e;
            };
            rd = _result.Value;
            rd = (ushort)(Convert.ToBoolean(args.op) ? rd | args.value : rd & ~args.value);
            foreach (var e in op_writedp((byte)dp, (byte)rd))
            {
                yield return e;
            };
        }

        public IEnumerable op_push_reg(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writestack(regs.r[args.n]))
            {
                yield return e;
            };
        }

        public IEnumerable op_push_p(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_writestack((byte)regs.p))
            {
                yield return e;
            };
        }

        public IEnumerable op_pop_reg(SMPCoreOpArgument args, SMPCoreOpResult result)
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
            regs.r[args.n] = _result.Value;
        }

        public IEnumerable op_pop_p(SMPCoreOpArgument args, SMPCoreOpResult result)
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
        }

        public IEnumerable op_mul_ya(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            ya = (ushort)(regs.y.Array[regs.y.Offset] * regs.a.Array[regs.a.Offset]);
            regs.a.Array[regs.a.Offset] = (byte)ya;
            regs.y.Array[regs.y.Offset] = (byte)(ya >> 8);
            //result is set based on y (high-byte) only
            regs.p.n = Convert.ToBoolean(Convert.ToInt32(Convert.ToBoolean(regs.y.Array[regs.y.Offset] & 0x80)));
            regs.p.z = (regs.y.Array[regs.y.Offset] == 0);
        }

        public IEnumerable op_div_ya_x(SMPCoreOpArgument args, SMPCoreOpResult result)
        {
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            foreach (var e in op_io())
            {
                yield return e;
            };
            ya = (ushort)regs.ya;
            //overflow set if quotient >= 256
            regs.p.v = !!(regs.y.Array[regs.y.Offset] >= regs.x.Array[regs.x.Offset]);
            regs.p.h = !!((regs.y.Array[regs.y.Offset] & 15) >= (regs.x.Array[regs.x.Offset] & 15));
            if (regs.y.Array[regs.y.Offset] < (regs.x.Array[regs.x.Offset] << 1))
            {
                //if quotient is <= 511 (will fit into 9-bit result)
                regs.a.Array[regs.a.Offset] = (byte)(ya / regs.x.Array[regs.x.Offset]);
                regs.y.Array[regs.y.Offset] = (byte)(ya % regs.x.Array[regs.x.Offset]);
            }
            else
            {
                //otherwise, the quotient won't fit into regs.p.v + regs.a
                //this emulates the odd behavior of the S-SMP in this case
                regs.a.Array[regs.a.Offset] = (byte)(255 - (ya - (regs.x.Array[regs.x.Offset] << 9)) / (256 - regs.x.Array[regs.x.Offset]));
                regs.y.Array[regs.y.Offset] = (byte)(regs.x.Array[regs.x.Offset] + (ya - (regs.x.Array[regs.x.Offset] << 9)) % (256 - regs.x.Array[regs.x.Offset]));
            }
            //result is set based on a (quotient) only
            regs.p.n = Convert.ToBoolean(Convert.ToInt32(Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80)));
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
        }

        public SMPCoreOperation[] opcode_table = new SMPCoreOperation[256];

        public void initialize_opcode_table()
        {
            opcode_table[0x00] = new SMPCoreOperation(op_nop, null);
            opcode_table[0x01] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 0 });
            opcode_table[0x02] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x01 });
            opcode_table[0x03] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x01, value = Convert.ToInt32(true) });
            opcode_table[0x04] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_or, n = (int)OpCode.A });
            opcode_table[0x05] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_or, n = (int)OpCode.A });
            opcode_table[0x06] = new SMPCoreOperation(op_read_a_ix, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x07] = new SMPCoreOperation(op_read_a_idpx, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x08] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_or, n = (int)OpCode.A });
            opcode_table[0x09] = new SMPCoreOperation(op_read_dp_dp, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x0a] = new SMPCoreOperation(op_or1_bit, new SMPCoreOpArgument() { op = 0 });
            opcode_table[0x0b] = new SMPCoreOperation(op_adjust_dp, new SMPCoreOpArgument() { op_func = op_asl });
            opcode_table[0x0c] = new SMPCoreOperation(op_adjust_addr, new SMPCoreOpArgument() { op_func = op_asl });
            opcode_table[0x0d] = new SMPCoreOperation(op_push_p, null);
            opcode_table[0x0e] = new SMPCoreOperation(op_adjust_addr_a, new SMPCoreOpArgument() { op = 1 });
            opcode_table[0x0f] = new SMPCoreOperation(op_brk, null);
            opcode_table[0x10] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x80, value = Convert.ToInt32(false) });
            opcode_table[0x11] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 1 });
            opcode_table[0x12] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x01 });
            opcode_table[0x13] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x01, value = Convert.ToInt32(false) });
            opcode_table[0x14] = new SMPCoreOperation(op_read_a_dpx, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x15] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_or, i = (int)OpCode.X });
            opcode_table[0x16] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_or, i = (int)OpCode.Y });
            opcode_table[0x17] = new SMPCoreOperation(op_read_a_idpy, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x18] = new SMPCoreOperation(op_read_dp_const, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x19] = new SMPCoreOperation(op_read_ix_iy, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x1a] = new SMPCoreOperation(op_adjustw_dp, new SMPCoreOpArgument() { adjust = -1 });
            opcode_table[0x1b] = new SMPCoreOperation(op_adjust_dpx, new SMPCoreOpArgument() { op_func = op_asl });
            opcode_table[0x1c] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_asl, n = (int)OpCode.A });
            opcode_table[0x1d] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_dec, n = (int)OpCode.X });
            opcode_table[0x1e] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.X });
            opcode_table[0x1f] = new SMPCoreOperation(op_jmp_iaddrx, null);
            opcode_table[0x20] = new SMPCoreOperation(op_setbit, new SMPCoreOpArgument() { mask = 0x20, value = 0x00 });
            opcode_table[0x21] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 2 });
            opcode_table[0x22] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x02 });
            opcode_table[0x23] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x02, value = Convert.ToInt32(true) });
            opcode_table[0x24] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_and, n = (int)OpCode.A });
            opcode_table[0x25] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_and, n = (int)OpCode.A });
            opcode_table[0x26] = new SMPCoreOperation(op_read_a_ix, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x27] = new SMPCoreOperation(op_read_a_idpx, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x28] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_and, n = (int)OpCode.A });
            opcode_table[0x29] = new SMPCoreOperation(op_read_dp_dp, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x2a] = new SMPCoreOperation(op_or1_bit, new SMPCoreOpArgument() { op = 1 });
            opcode_table[0x2b] = new SMPCoreOperation(op_adjust_dp, new SMPCoreOpArgument() { op_func = op_rol });
            opcode_table[0x2c] = new SMPCoreOperation(op_adjust_addr, new SMPCoreOpArgument() { op_func = op_rol });
            opcode_table[0x2d] = new SMPCoreOperation(op_push_reg, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0x2e] = new SMPCoreOperation(op_cbne_dp, null);
            opcode_table[0x2f] = new SMPCoreOperation(op_bra, null);
            opcode_table[0x30] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x80, value = Convert.ToInt32(true) });
            opcode_table[0x31] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 3 });
            opcode_table[0x32] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x02 });
            opcode_table[0x33] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x02, value = Convert.ToInt32(false) });
            opcode_table[0x34] = new SMPCoreOperation(op_read_a_dpx, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x35] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_and, i = (int)OpCode.X });
            opcode_table[0x36] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_and, i = (int)OpCode.Y });
            opcode_table[0x37] = new SMPCoreOperation(op_read_a_idpy, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x38] = new SMPCoreOperation(op_read_dp_const, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x39] = new SMPCoreOperation(op_read_ix_iy, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x3a] = new SMPCoreOperation(op_adjustw_dp, new SMPCoreOpArgument() { adjust = +1 });
            opcode_table[0x3b] = new SMPCoreOperation(op_adjust_dpx, new SMPCoreOpArgument() { op_func = op_rol });
            opcode_table[0x3c] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_rol, n = (int)OpCode.A });
            opcode_table[0x3d] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_inc, n = (int)OpCode.X });
            opcode_table[0x3e] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.X });
            opcode_table[0x3f] = new SMPCoreOperation(op_call, null);
            opcode_table[0x40] = new SMPCoreOperation(op_setbit, new SMPCoreOpArgument() { mask = 0x20, value = 0x20 });
            opcode_table[0x41] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 4 });
            opcode_table[0x42] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x04 });
            opcode_table[0x43] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x04, value = Convert.ToInt32(true) });
            opcode_table[0x44] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_eor, n = (int)OpCode.A });
            opcode_table[0x45] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_eor, n = (int)OpCode.A });
            opcode_table[0x46] = new SMPCoreOperation(op_read_a_ix, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x47] = new SMPCoreOperation(op_read_a_idpx, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x48] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_eor, n = (int)OpCode.A });
            opcode_table[0x49] = new SMPCoreOperation(op_read_dp_dp, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x4a] = new SMPCoreOperation(op_and1_bit, new SMPCoreOpArgument() { op = 0 });
            opcode_table[0x4b] = new SMPCoreOperation(op_adjust_dp, new SMPCoreOpArgument() { op_func = op_lsr });
            opcode_table[0x4c] = new SMPCoreOperation(op_adjust_addr, new SMPCoreOpArgument() { op_func = op_lsr });
            opcode_table[0x4d] = new SMPCoreOperation(op_push_reg, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0x4e] = new SMPCoreOperation(op_adjust_addr_a, new SMPCoreOpArgument() { op = 0 });
            opcode_table[0x4f] = new SMPCoreOperation(op_pcall, null);
            opcode_table[0x50] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x40, value = Convert.ToInt32(false) });
            opcode_table[0x51] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 5 });
            opcode_table[0x52] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x04 });
            opcode_table[0x53] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x04, value = Convert.ToInt32(false) });
            opcode_table[0x54] = new SMPCoreOperation(op_read_a_dpx, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x55] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_eor, i = (int)OpCode.X });
            opcode_table[0x56] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_eor, i = (int)OpCode.Y });
            opcode_table[0x57] = new SMPCoreOperation(op_read_a_idpy, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x58] = new SMPCoreOperation(op_read_dp_const, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x59] = new SMPCoreOperation(op_read_ix_iy, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x5a] = new SMPCoreOperation(op_cmpw_ya_dp, null);
            opcode_table[0x5b] = new SMPCoreOperation(op_adjust_dpx, new SMPCoreOpArgument() { op_func = op_lsr });
            opcode_table[0x5c] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_lsr, n = (int)OpCode.A });
            opcode_table[0x5d] = new SMPCoreOperation(op_mov_reg_reg, new SMPCoreOpArgument() { to = (int)OpCode.X, from = (int)OpCode.A });
            opcode_table[0x5e] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.Y });
            opcode_table[0x5f] = new SMPCoreOperation(op_jmp_addr, null);
            opcode_table[0x60] = new SMPCoreOperation(op_setbit, new SMPCoreOpArgument() { mask = 0x01, value = 0x00 });
            opcode_table[0x61] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 6 });
            opcode_table[0x62] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x08 });
            opcode_table[0x63] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x08, value = Convert.ToInt32(true) });
            opcode_table[0x64] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.A });
            opcode_table[0x65] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.A });
            opcode_table[0x66] = new SMPCoreOperation(op_read_a_ix, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x67] = new SMPCoreOperation(op_read_a_idpx, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x68] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.A });
            opcode_table[0x69] = new SMPCoreOperation(op_read_dp_dp, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x6a] = new SMPCoreOperation(op_and1_bit, new SMPCoreOpArgument() { op = 1 });
            opcode_table[0x6b] = new SMPCoreOperation(op_adjust_dp, new SMPCoreOpArgument() { op_func = op_ror });
            opcode_table[0x6c] = new SMPCoreOperation(op_adjust_addr, new SMPCoreOpArgument() { op_func = op_ror });
            opcode_table[0x6d] = new SMPCoreOperation(op_push_reg, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0x6e] = new SMPCoreOperation(op_dbnz_dp, null);
            opcode_table[0x6f] = new SMPCoreOperation(op_ret, null);
            opcode_table[0x70] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x40, value = Convert.ToInt32(true) });
            opcode_table[0x71] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 7 });
            opcode_table[0x72] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x08 });
            opcode_table[0x73] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x08, value = Convert.ToInt32(false) });
            opcode_table[0x74] = new SMPCoreOperation(op_read_a_dpx, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x75] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_cmp, i = (int)OpCode.X });
            opcode_table[0x76] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_cmp, i = (int)OpCode.Y });
            opcode_table[0x77] = new SMPCoreOperation(op_read_a_idpy, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x78] = new SMPCoreOperation(op_read_dp_const, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x79] = new SMPCoreOperation(op_read_ix_iy, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x7a] = new SMPCoreOperation(op_read_ya_dp, new SMPCoreOpArgument() { op_func = op_addw });
            opcode_table[0x7b] = new SMPCoreOperation(op_adjust_dpx, new SMPCoreOpArgument() { op_func = op_ror });
            opcode_table[0x7c] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_ror, n = (int)OpCode.A });
            opcode_table[0x7d] = new SMPCoreOperation(op_mov_reg_reg, new SMPCoreOpArgument() { to = (int)OpCode.A, from = (int)OpCode.X });
            opcode_table[0x7e] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.Y });
            opcode_table[0x7f] = new SMPCoreOperation(op_reti, null);
            opcode_table[0x80] = new SMPCoreOperation(op_setbit, new SMPCoreOpArgument() { mask = 0x01, value = 0x01 });
            opcode_table[0x81] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 8 });
            opcode_table[0x82] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x10 });
            opcode_table[0x83] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x10, value = Convert.ToInt32(true) });
            opcode_table[0x84] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_adc, n = (int)OpCode.A });
            opcode_table[0x85] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_adc, n = (int)OpCode.A });
            opcode_table[0x86] = new SMPCoreOperation(op_read_a_ix, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x87] = new SMPCoreOperation(op_read_a_idpx, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x88] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_adc, n = (int)OpCode.A });
            opcode_table[0x89] = new SMPCoreOperation(op_read_dp_dp, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x8a] = new SMPCoreOperation(op_eor1_bit, null);
            opcode_table[0x8b] = new SMPCoreOperation(op_adjust_dp, new SMPCoreOpArgument() { op_func = op_dec });
            opcode_table[0x8c] = new SMPCoreOperation(op_adjust_addr, new SMPCoreOpArgument() { op_func = op_dec });
            opcode_table[0x8d] = new SMPCoreOperation(op_mov_reg_const, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0x8e] = new SMPCoreOperation(op_pop_p, null);
            opcode_table[0x8f] = new SMPCoreOperation(op_mov_dp_const, null);
            opcode_table[0x90] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x01, value = Convert.ToInt32(false) });
            opcode_table[0x91] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 9 });
            opcode_table[0x92] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x10 });
            opcode_table[0x93] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x10, value = Convert.ToInt32(false) });
            opcode_table[0x94] = new SMPCoreOperation(op_read_a_dpx, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x95] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_adc, i = (int)OpCode.X });
            opcode_table[0x96] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_adc, i = (int)OpCode.Y });
            opcode_table[0x97] = new SMPCoreOperation(op_read_a_idpy, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x98] = new SMPCoreOperation(op_read_dp_const, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x99] = new SMPCoreOperation(op_read_ix_iy, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x9a] = new SMPCoreOperation(op_read_ya_dp, new SMPCoreOpArgument() { op_func = op_subw });
            opcode_table[0x9b] = new SMPCoreOperation(op_adjust_dpx, new SMPCoreOpArgument() { op_func = op_dec });
            opcode_table[0x9c] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_dec, n = (int)OpCode.A });
            opcode_table[0x9d] = new SMPCoreOperation(op_mov_reg_reg, new SMPCoreOpArgument() { to = (int)OpCode.X, from = (int)OpCode.SP });
            opcode_table[0x9e] = new SMPCoreOperation(op_div_ya_x, null);
            opcode_table[0x9f] = new SMPCoreOperation(op_xcn, null);
            opcode_table[0xa0] = new SMPCoreOperation(op_seti, new SMPCoreOpArgument() { value = 1 });
            opcode_table[0xa1] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 10 });
            opcode_table[0xa2] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x20 });
            opcode_table[0xa3] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x20, value = Convert.ToInt32(true) });
            opcode_table[0xa4] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_sbc, n = (int)OpCode.A });
            opcode_table[0xa5] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_sbc, n = (int)OpCode.A });
            opcode_table[0xa6] = new SMPCoreOperation(op_read_a_ix, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xa7] = new SMPCoreOperation(op_read_a_idpx, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xa8] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_sbc, n = (int)OpCode.A });
            opcode_table[0xa9] = new SMPCoreOperation(op_read_dp_dp, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xaa] = new SMPCoreOperation(op_mov1_c_bit, null);
            opcode_table[0xab] = new SMPCoreOperation(op_adjust_dp, new SMPCoreOpArgument() { op_func = op_inc });
            opcode_table[0xac] = new SMPCoreOperation(op_adjust_addr, new SMPCoreOpArgument() { op_func = op_inc });
            opcode_table[0xad] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.Y });
            opcode_table[0xae] = new SMPCoreOperation(op_pop_reg, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0xaf] = new SMPCoreOperation(op_mov_ixinc_a, null);
            opcode_table[0xb0] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x01, value = Convert.ToInt32(true) });
            opcode_table[0xb1] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 11 });
            opcode_table[0xb2] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x20 });
            opcode_table[0xb3] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x20, value = Convert.ToInt32(false) });
            opcode_table[0xb4] = new SMPCoreOperation(op_read_a_dpx, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xb5] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_sbc, i = (int)OpCode.X });
            opcode_table[0xb6] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_sbc, i = (int)OpCode.Y });
            opcode_table[0xb7] = new SMPCoreOperation(op_read_a_idpy, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xb8] = new SMPCoreOperation(op_read_dp_const, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xb9] = new SMPCoreOperation(op_read_ix_iy, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xba] = new SMPCoreOperation(op_movw_ya_dp, null);
            opcode_table[0xbb] = new SMPCoreOperation(op_adjust_dpx, new SMPCoreOpArgument() { op_func = op_inc });
            opcode_table[0xbc] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_inc, n = (int)OpCode.A });
            opcode_table[0xbd] = new SMPCoreOperation(op_mov_sp_x, null);
            opcode_table[0xbe] = new SMPCoreOperation(op_das, null);
            opcode_table[0xbf] = new SMPCoreOperation(op_mov_a_ixinc, null);
            opcode_table[0xc0] = new SMPCoreOperation(op_seti, new SMPCoreOpArgument() { value = 0 });
            opcode_table[0xc1] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 12 });
            opcode_table[0xc2] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x40 });
            opcode_table[0xc3] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x40, value = Convert.ToInt32(true) });
            opcode_table[0xc4] = new SMPCoreOperation(op_mov_dp_reg, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0xc5] = new SMPCoreOperation(op_mov_addr_reg, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0xc6] = new SMPCoreOperation(op_mov_ix_a, null);
            opcode_table[0xc7] = new SMPCoreOperation(op_mov_idpx_a, null);
            opcode_table[0xc8] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.X });
            opcode_table[0xc9] = new SMPCoreOperation(op_mov_addr_reg, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0xca] = new SMPCoreOperation(op_mov1_bit_c, null);
            opcode_table[0xcb] = new SMPCoreOperation(op_mov_dp_reg, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0xcc] = new SMPCoreOperation(op_mov_addr_reg, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0xcd] = new SMPCoreOperation(op_mov_reg_const, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0xce] = new SMPCoreOperation(op_pop_reg, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0xcf] = new SMPCoreOperation(op_mul_ya, null);
            opcode_table[0xd0] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x02, value = Convert.ToInt32(false) });
            opcode_table[0xd1] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 13 });
            opcode_table[0xd2] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x40 });
            opcode_table[0xd3] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x40, value = Convert.ToInt32(false) });
            opcode_table[0xd4] = new SMPCoreOperation(op_mov_dpr_reg, new SMPCoreOpArgument() { n = (int)OpCode.A, i = (int)OpCode.X });
            opcode_table[0xd5] = new SMPCoreOperation(op_mov_addrr_a, new SMPCoreOpArgument() { i = (int)OpCode.X });
            opcode_table[0xd6] = new SMPCoreOperation(op_mov_addrr_a, new SMPCoreOpArgument() { i = (int)OpCode.Y });
            opcode_table[0xd7] = new SMPCoreOperation(op_mov_idpy_a, null);
            opcode_table[0xd8] = new SMPCoreOperation(op_mov_dp_reg, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0xd9] = new SMPCoreOperation(op_mov_dpr_reg, new SMPCoreOpArgument() { n = (int)OpCode.X, i = (int)OpCode.Y });
            opcode_table[0xda] = new SMPCoreOperation(op_movw_dp_ya, null);
            opcode_table[0xdb] = new SMPCoreOperation(op_mov_dpr_reg, new SMPCoreOpArgument() { n = (int)OpCode.Y, i = (int)OpCode.X });
            opcode_table[0xdc] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_dec, n = (int)OpCode.Y });
            opcode_table[0xdd] = new SMPCoreOperation(op_mov_reg_reg, new SMPCoreOpArgument() { to = (int)OpCode.A, from = (int)OpCode.Y });
            opcode_table[0xde] = new SMPCoreOperation(op_cbne_dpx, null);
            opcode_table[0xdf] = new SMPCoreOperation(op_daa, null);
            opcode_table[0xe0] = new SMPCoreOperation(op_setbit, new SMPCoreOpArgument() { mask = 0x48, value = 0x00 });
            opcode_table[0xe1] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 14 });
            opcode_table[0xe2] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x80 });
            opcode_table[0xe3] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x80, value = Convert.ToInt32(true) });
            opcode_table[0xe4] = new SMPCoreOperation(op_mov_reg_dp, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0xe5] = new SMPCoreOperation(op_mov_reg_addr, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0xe6] = new SMPCoreOperation(op_mov_a_ix, null);
            opcode_table[0xe7] = new SMPCoreOperation(op_mov_a_idpx, null);
            opcode_table[0xe8] = new SMPCoreOperation(op_mov_reg_const, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0xe9] = new SMPCoreOperation(op_mov_reg_addr, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0xea] = new SMPCoreOperation(op_not1_bit, null);
            opcode_table[0xeb] = new SMPCoreOperation(op_mov_reg_dp, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0xec] = new SMPCoreOperation(op_mov_reg_addr, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0xed] = new SMPCoreOperation(op_notc, null);
            opcode_table[0xee] = new SMPCoreOperation(op_pop_reg, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0xef] = new SMPCoreOperation(op_wait, null);
            opcode_table[0xf0] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x02, value = Convert.ToInt32(true) });
            opcode_table[0xf1] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 15 });
            opcode_table[0xf2] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x80 });
            opcode_table[0xf3] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x80, value = Convert.ToInt32(false) });
            opcode_table[0xf4] = new SMPCoreOperation(op_mov_reg_dpr, new SMPCoreOpArgument() { n = (int)OpCode.A, i = (int)OpCode.X });
            opcode_table[0xf5] = new SMPCoreOperation(op_mov_a_addrr, new SMPCoreOpArgument() { i = (int)OpCode.X });
            opcode_table[0xf6] = new SMPCoreOperation(op_mov_a_addrr, new SMPCoreOpArgument() { i = (int)OpCode.Y });
            opcode_table[0xf7] = new SMPCoreOperation(op_mov_a_idpy, null);
            opcode_table[0xf8] = new SMPCoreOperation(op_mov_reg_dp, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0xf9] = new SMPCoreOperation(op_mov_reg_dpr, new SMPCoreOpArgument() { n = (int)OpCode.X, i = (int)OpCode.Y });
            opcode_table[0xfa] = new SMPCoreOperation(op_mov_dp_dp, null);
            opcode_table[0xfb] = new SMPCoreOperation(op_mov_reg_dpr, new SMPCoreOpArgument() { n = (int)OpCode.Y, i = (int)OpCode.X });
            opcode_table[0xfc] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_inc, n = (int)OpCode.Y });
            opcode_table[0xfd] = new SMPCoreOperation(op_mov_reg_reg, new SMPCoreOpArgument() { to = (int)OpCode.Y, from = (int)OpCode.A });
            opcode_table[0xfe] = new SMPCoreOperation(op_dbnz_y, null);
            opcode_table[0xff] = new SMPCoreOperation(op_wait, null);
        }

        public void core_serialize(Serializer s)
        {
            s.integer(regs.pc, "regs.pc");
            s.integer(regs.a.Array[regs.a.Offset], "regs.a");
            s.integer(regs.x.Array[regs.x.Offset], "regs.x");
            s.integer(regs.y.Array[regs.y.Offset], "regs.y");
            s.integer(regs.sp.Array[regs.sp.Offset], "regs.sp");
            s.integer(regs.p.n, "regs.p.n");
            s.integer(regs.p.v, "regs.p.v");
            s.integer(regs.p.p, "regs.p.p");
            s.integer(regs.p.b, "regs.p.b");
            s.integer(regs.p.h, "regs.p.h");
            s.integer(regs.p.i, "regs.p.i");
            s.integer(regs.p.z, "regs.p.z");
            s.integer(regs.p.c, "regs.p.c");

            s.integer(dp, "dp");
            s.integer(sp, "sp");
            s.integer(rd, "rd");
            s.integer(wr, "wr");
            s.integer(bit, "bit");
            s.integer(ya, "ya");
        }

        public SMPCore()
        {
            initialize_opcode_table();
        }
    }
}
