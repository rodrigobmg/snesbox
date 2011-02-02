using System;

namespace Snes
{
    partial class CPUCore
    {
        public class Flag
        {
            public bool n, v, m, x, d, i, z, c;

            public static explicit operator uint(Flag flag)
            {
                return (uint)((Convert.ToInt32(flag.n) << 7) + (Convert.ToInt32(flag.v) << 6) + (Convert.ToInt32(flag.m) << 5) + (Convert.ToInt32(flag.x) << 4)
                     + (Convert.ToInt32(flag.d) << 3) + (Convert.ToInt32(flag.i) << 2) + (Convert.ToInt32(flag.z) << 1) + (Convert.ToInt32(flag.c) << 0));
            }

            public uint Assign(byte data)
            {
                n = Convert.ToBoolean(data & 0x80);
                v = Convert.ToBoolean(data & 0x40);
                m = Convert.ToBoolean(data & 0x20);
                x = Convert.ToBoolean(data & 0x10);
                d = Convert.ToBoolean(data & 0x08);
                i = Convert.ToBoolean(data & 0x04);
                z = Convert.ToBoolean(data & 0x02);
                c = Convert.ToBoolean(data & 0x01);
                return data;
            }

            public static uint operator |(Flag flag, uint data)
            {
                return (uint)flag | data;
            }

            public static uint operator ^(Flag flag, uint data)
            {
                return (uint)flag ^ data;
            }

            public static uint operator &(Flag flag, uint data)
            {
                return (uint)flag & data;
            }

            public Flag()
            {
                n = v = m = x = d = i = z = c = false;
            }
        }
    }
}
