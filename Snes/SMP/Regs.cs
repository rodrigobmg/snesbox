using System;

namespace Snes
{
    partial class SMPCore
    {
        public class Regs
        {
            public ushort pc;
            public byte[] r = new byte[4];
            public ArraySegment<byte> a, x, y, sp;
            public RegYA ya;
            public Flag p = new Flag();

            public Regs()
            {
                a = new ArraySegment<byte>(r, 0, 1);
                x = new ArraySegment<byte>(r, 1, 1);
                y = new ArraySegment<byte>(r, 2, 1);
                sp = new ArraySegment<byte>(r, 3, 1);
                ya = new RegYA(new ArraySegment<byte>(r, 2, 1), new ArraySegment<byte>(r, 0, 1));
            }
        }
    }
}
