using System;

namespace Snes
{
    partial class SuperFX
    {
        public class Cfgr
        {
            public bool irq;
            public bool ms0;

            public static explicit operator uint(Cfgr cfgr) { throw new NotImplementedException(); }

            public Cfgr Assign(byte data) { throw new NotImplementedException(); }
        }
    }
}
