using System;

namespace Snes
{
    class SPC7110MCU : Memory
    {
        public static SPC7110MCU spc7110mcu = new SPC7110MCU();

        public override uint size() { throw new NotImplementedException(); }
        public override byte read(uint addr) { throw new NotImplementedException(); }
        public override void write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
