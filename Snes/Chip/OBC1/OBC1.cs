using System;
using Nall;

namespace Snes
{
    partial class OBC1 : Memory
    {
        public static OBC1 obc1 = new OBC1();

        public override uint size() { throw new NotImplementedException(); }
        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public override byte read(uint addr) { throw new NotImplementedException(); }
        public override void write(uint addr, byte data) { throw new NotImplementedException(); }

        public void serialize(Serializer s)
        {
            throw new NotImplementedException();
        }

        public OBC1() { /*throw new NotImplementedException();*/ }

        private byte ram_read(uint addr) { throw new NotImplementedException(); }
        private void ram_write(uint addr, byte data) { throw new NotImplementedException(); }

        private Status status;
    }
}
