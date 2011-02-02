using System;
using System.Collections;

namespace Snes
{
    class SuperFXGSURAM : Memory
    {
        public static SuperFXGSURAM gsuram = new SuperFXGSURAM();

        public override uint size()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable read(uint addr, Result result)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable write(uint addr, byte data)
        {
            throw new NotImplementedException();
        }
    }
}
