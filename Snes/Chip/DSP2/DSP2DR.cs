using System;
using System.Collections;

namespace Snes
{
    class DSP2DR : Memory
    {
        public static DSP2DR dsp2dr = new DSP2DR();

        public override uint size() { throw new NotImplementedException(); }
        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
