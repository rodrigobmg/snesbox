using System.Runtime.InteropServices;

namespace Snes
{
    partial class CPUCore
    {
        [StructLayout(LayoutKind.Explicit)]
        public class Reg16
        {
            [FieldOffset(0)]
            public ushort w;

#if BIG_ENDIAN
            [FieldOffset(1)]
#else
            [FieldOffset(0)]
#endif
            public byte l;
#if BIG_ENDIAN
            [FieldOffset(0)]
#else
            [FieldOffset(1)]
#endif
            public byte h;

            public static explicit operator uint(Reg16 reg16)
            {
                return reg16.w;
            }

            public uint Assign(uint i)
            {
                return w = (ushort)i;
            }

            public static uint operator |(Reg16 reg16, uint i)
            {
                return (uint)reg16.w | (ushort)i;
            }

            public static uint operator ^(Reg16 reg16, uint i)
            {
                return (uint)reg16.w ^ (ushort)i;
            }

            public static uint operator &(Reg16 reg16, uint i)
            {
                return (uint)reg16.w & (ushort)i;
            }

            public static uint operator <<(Reg16 reg16, int i)
            {
                return (uint)reg16.w << i;
            }

            public static uint operator >>(Reg16 reg16, int i)
            {
                return (uint)reg16.w >> i;
            }

            public static uint operator +(Reg16 reg16, uint i)
            {
                return (uint)reg16.w + (ushort)i;
            }

            public static uint operator -(Reg16 reg16, uint i)
            {
                return (uint)reg16.w - (ushort)i;
            }

            public static uint operator *(Reg16 reg16, uint i)
            {
                return (uint)reg16.w * (ushort)i;
            }

            public static uint operator /(Reg16 reg16, uint i)
            {
                return (uint)reg16.w / (ushort)i;
            }

            public static uint operator %(Reg16 reg16, uint i)
            {
                return (uint)reg16.w % (ushort)i;
            }

            public Reg16()
            {
                w = 0;
            }
        }
    }
}
