using System;

namespace Snes
{
    class CPUIRAM : Memory
    {
        public static CPUIRAM cpuiram = new CPUIRAM();

        public override uint size() { throw new NotImplementedException(); }
        public override byte read(uint addr) { throw new NotImplementedException(); }
        public override void write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
