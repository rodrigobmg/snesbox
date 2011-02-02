using System;

namespace Snes
{
    partial class SuperFX
    {
        public class Scmr
        {
            public uint ht;
            public bool ron;
            public bool ran;
            public uint md;

            public static explicit operator uint(Scmr scmr) { throw new NotImplementedException(); }

            public Scmr Assign(byte data) { throw new NotImplementedException(); }
        }
    }
}
