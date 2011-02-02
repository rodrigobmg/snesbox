using System;
using System.Collections;

namespace Snes
{
    partial class BSXFlash : Memory
    {
        public static BSXFlash bsxflash = new BSXFlash();

        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public override uint size() { throw new NotImplementedException(); }
        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }

        private Regs regs;
    }
}
