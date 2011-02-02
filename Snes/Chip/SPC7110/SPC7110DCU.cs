using System;
using System.Collections;

namespace Snes
{
    class SPC7110DCU : Memory
    {
        public static SPC7110DCU spc7110dcu = new SPC7110DCU();

        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
