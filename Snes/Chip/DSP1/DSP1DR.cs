using System;

namespace Snes
{
    class DSP1DR : Memory
    {
        public static DSP1DR dsp1dr = new DSP1DR();

        public override uint size() { throw new NotImplementedException(); }
        public override byte read(uint addr) { throw new NotImplementedException(); }
        public override void write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
