using System;
using System.Collections;

namespace Snes
{
    class SA1BWRAM : Memory
    {
        public static SA1BWRAM sa1bwram = new SA1BWRAM();

        public override uint size() { throw new NotImplementedException(); }
        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
