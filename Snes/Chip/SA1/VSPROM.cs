using System;
using System.Collections;

namespace Snes
{
    class VSPROM : Memory
    {
        public static VSPROM vsprom = new VSPROM();

        public override uint size() { throw new NotImplementedException(); }
        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
