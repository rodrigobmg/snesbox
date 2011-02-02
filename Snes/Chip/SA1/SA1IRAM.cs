using System;
using System.Collections;

namespace Snes
{
    class SA1IRAM : Memory
    {
        public static SA1IRAM sa1iram = new SA1IRAM();

        public override uint size() { throw new NotImplementedException(); }
        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
