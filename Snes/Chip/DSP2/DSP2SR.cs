using System;
using System.Collections;

namespace Snes
{
    class DSP2SR : Memory
    {
        public static DSP2SR dsp2sr = new DSP2SR();

        public override uint size() { throw new NotImplementedException(); }
        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
