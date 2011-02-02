using System;

namespace Snes
{
    partial class SMPCore
    {
        public class RegYA
        {
            public ArraySegment<byte> hi, lo;

            public static explicit operator ushort(RegYA reg)
            {
                return (ushort)((reg.hi.Array[reg.hi.Offset] << 8) + reg.lo.Array[reg.lo.Offset]);
            }

            public RegYA Assign(ushort data)
            {
                hi.Array[hi.Offset] = (byte)(data >> 8);
                lo.Array[lo.Offset] = (byte)data;
                return this;
            }

            public RegYA(ArraySegment<byte> hi_, ArraySegment<byte> lo_)
            {
                hi = hi_;
                lo = lo_;
            }
        }
    }
}
