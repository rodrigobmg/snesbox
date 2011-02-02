using System;
using Nall;

namespace Snes
{
    partial class DSP2
    {
        public static DSP2 dsp2 = new DSP2();

        public Status status;

        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public byte read(uint addr) { throw new NotImplementedException(); }
        public void write(uint addr, byte data) { throw new NotImplementedException(); }

        public void serialize(Serializer s)
        {
            throw new NotImplementedException();
        }

        public DSP2() { /*throw new NotImplementedException();*/ }

        protected void op01() { throw new NotImplementedException(); }
        protected void op03() { throw new NotImplementedException(); }
        protected void op05() { throw new NotImplementedException(); }
        protected void op06() { throw new NotImplementedException(); }
        protected void op09() { throw new NotImplementedException(); }
        protected void op0d() { throw new NotImplementedException(); }
    }
}
