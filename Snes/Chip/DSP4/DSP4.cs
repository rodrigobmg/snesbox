﻿using System;
using System.Collections;

namespace Snes
{
    class DSP4 : Memory
    {
        public static DSP4 dsp4 = new DSP4();

        public override uint size() { throw new NotImplementedException(); }
        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
