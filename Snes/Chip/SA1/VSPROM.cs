using System;

namespace Snes
{
    class VSPROM : Memory
    {
        public static VSPROM vsprom = new VSPROM();

        public override uint size() { throw new NotImplementedException(); }
        public override byte read(uint addr) { throw new NotImplementedException(); }
        public override void write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
