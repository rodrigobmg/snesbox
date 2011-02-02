using System;
using System.Collections;

namespace Snes
{
    class DSP1DR : Memory
    {
        public static DSP1DR dsp1dr = new DSP1DR();

        public override uint size() { throw new NotImplementedException(); }
        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
