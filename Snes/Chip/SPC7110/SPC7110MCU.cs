using System;
using System.Collections;

namespace Snes
{
    class SPC7110MCU : Memory
    {
        public static SPC7110MCU spc7110mcu = new SPC7110MCU();

        public override uint size() { throw new NotImplementedException(); }
        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
