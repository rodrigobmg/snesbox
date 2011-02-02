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

            [FieldOffset(0)]
            public ushort w;
            [FieldOffset(2)]
            public ushort wh;

            [FieldOffset(0)]
            public byte l;
            [FieldOffset(1)]
            public byte h;
            [FieldOffset(2)]
            public byte b;
            [FieldOffset(3)]
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
