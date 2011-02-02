using System;

namespace Snes
{
    class DSP2SR : Memory
    {
        public static DSP2SR dsp2sr = new DSP2SR();

        public override uint size() { throw new NotImplementedException(); }
        public override byte read(uint addr) { throw new NotImplementedException(); }
        public override void write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
