﻿using System;
using System.Collections;

namespace Snes
{
    class DSP1SR : Memory
    {
        public static DSP1SR dsp1sr = new DSP1SR();

        public override uint size() { throw new NotImplementedException(); }
        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
