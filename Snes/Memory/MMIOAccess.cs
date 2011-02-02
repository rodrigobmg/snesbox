using System.Collections;

namespace Snes
{
    class MMIOAccess : Memory
    {
        public static MMIOAccess mmio = new MMIOAccess();

        public IMMIO handle(uint addr)
        {
            return _mmio[addr & 0x7fff];
        }

        public void map(uint addr, IMMIO access)
        {
            _mmio[addr & 0x7fff] = access;
        }

        public override IEnumerable read(uint addr, Result result)
        {
            foreach (var e in _mmio[addr & 0x7fff].mmio_read(addr, result))
            {
                yield return e;
            };
        }

        public override IEnumerable write(uint addr, byte data)
        {
            foreach (var e in _mmio[addr & 0x7fff].mmio_write(addr, data))
            {
                yield return e;
            };
        }

        public MMIOAccess()
        {
            for (uint i = 0; i < 0x8000; i++)
            {
                _mmio[i] = UnmappedMMIO.mmio_unmapped;
            }
        }

        private IMMIO[] _mmio = new IMMIO[0x8000];
    }
}
