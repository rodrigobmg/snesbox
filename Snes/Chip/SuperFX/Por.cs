using System;

namespace Snes
{
    partial class SuperFX
    {
        public class Por
        {
            public bool obj;
            public bool freezehigh;
            public bool highnibble;
            public bool dither;
            public bool transparent;

            public static explicit operator uint(Por por) { throw new NotImplementedException(); }

            public Por Assign(byte data) { throw new NotImplementedException(); }
        }
    }
}
