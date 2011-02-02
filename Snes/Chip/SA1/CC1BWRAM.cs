using System;
using System.Collections;

namespace Snes
{
    class CC1BWRAM : Memory
    {
        public static CC1BWRAM cc1bwram = new CC1BWRAM();

        public override uint size() { throw new NotImplementedException(); }
        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }
        public bool dma;
    }
}
