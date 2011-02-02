using System.Runtime.InteropServices;
using Nall;

namespace Snes
{
    partial class CPUCore
    {
        [StructLayout(LayoutKind.Explicit)]
        public class Reg24
        {
            [FieldOffset(0)]
            public uint d;

#if BIG_ENDIAN
            [FieldOffset(2)]
#else
            [FieldOffset(0)]
#endif
            public ushort w;
#if BIG_ENDIAN
            [FieldOffset(0)]
#else
            [FieldOffset(2)]
#endif
            public ushort wh;

#if BIG_ENDIAN
            [FieldOffset(3)]
#else
            [FieldOffset(0)]
#endif
            public byte l;
#if BIG_ENDIAN
            [FieldOffset(2)]
#else
            [FieldOffset(1)]
#endif
            public byte h;
#if BIG_ENDIAN
            [FieldOffset(1)]
#else
            [FieldOffset(2)]
#endif
            public byte b;
#if BIG_ENDIAN
            [FieldOffset(0)]
#else
            [FieldOffset(3)]
#endif
            public byte bh;

            public static explicit operator uint(Reg24 reg24)
            {
                return reg24.d;
            }

            public uint Assign(uint i)
            {
                return d = Bit.uclip(24, i);
            }

            public static uint operator |(Reg24 reg24, uint i)
            {
                return Bit.uclip(24, reg24.d | i);
            }

            public static uint operator ^(Reg24 reg24, uint i)
            {
                return Bit.uclip(24, reg24.d ^ i);
            }

            public static uint operator &(Reg24 reg24, uint i)
            {
                return Bit.uclip(24, reg24.d & i);
            }

            public static uint operator <<(Reg24 reg24, int i)
            {
                return Bit.uclip(24, reg24.d << i);
            }

            public static uint operator >>(Reg24 reg24, int i)
            {
                return Bit.uclip(24, reg24.d >> i);
            }

            public static uint operator +(Reg24 reg24, uint i)
            {
                return Bit.uclip(24, reg24.d + i);
            }

            public static uint operator -(Reg24 reg24, uint i)
            {
                return Bit.uclip(24, reg24.d - i);
            }

            public static uint operator *(Reg24 reg24, uint i)
            {
                return Bit.uclip(24, reg24.d * i);
            }

            public static uint operator /(Reg24 reg24, uint i)
            {
                return Bit.uclip(24, reg24.d / i);
            }

            public static uint operator %(Reg24 reg24, uint i)
            {
                return Bit.uclip(24, reg24.d % i);
            }

            public Reg24()
            {
                d = 0;
            }
        }
    }
}
