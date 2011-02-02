using System;

namespace Snes
{
    class SA1IRAM : Memory
    {
        public static SA1IRAM sa1iram = new SA1IRAM();

        public override uint size() { throw new NotImplementedException(); }
        public override byte read(uint addr) { throw new NotImplementedException(); }
        public override void write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
