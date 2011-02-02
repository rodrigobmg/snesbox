using System.Collections;

namespace Snes
{
    class UnmappedMMIO : IMMIO
    {
        public static UnmappedMMIO mmio_unmapped = new UnmappedMMIO();

        public IEnumerable mmio_read(uint addr, Result result)
        {
            result.Value = CPU.cpu.regs.mdr;
            yield break;
        }

        public IEnumerable mmio_write(uint addr, byte data)
        {
            yield break;
        }
    }
}
