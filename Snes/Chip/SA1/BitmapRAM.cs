using System;

namespace Snes
{
    class BitmapRAM : Memory
    {
        public static BitmapRAM bitmapram = new BitmapRAM();

        public override uint size() { throw new NotImplementedException(); }
        public override byte read(uint addr) { throw new NotImplementedException(); }
        public override void write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
