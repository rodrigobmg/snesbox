using System;
using System.Collections;

namespace Snes
{
    class CPUIRAM : Memory
    {
        public static CPUIRAM cpuiram = new CPUIRAM();

        public override uint size() { throw new NotImplementedException(); }
        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
