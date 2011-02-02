using System;

namespace Snes
{
    class SuperFXGSURAM : Memory
    {
        public static SuperFXGSURAM gsuram = new SuperFXGSURAM();

        public override uint size()
        {
            throw new NotImplementedException();
        }

        public override byte read(uint addr)
        {
            throw new NotImplementedException();
        }

        public override void write(uint addr, byte data)
        {
            throw new NotImplementedException();
        }
    }
}
