using System;
using System.Collections;

namespace Snes
{
    class SPC7110RAM : Memory
    {
        public static SPC7110RAM spc7110ram = new SPC7110RAM();

        public override uint size() { throw new NotImplementedException(); }
        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
