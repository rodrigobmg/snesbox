using System.Collections;

namespace Snes
{
    class UnmappedMemory : Memory
    {
        public static UnmappedMemory memory_unmapped = new UnmappedMemory();

        public override uint size()
        {
            return 16 * 1024 * 1024;
        }

        public override IEnumerable read(uint addr, Result result)
        {
            result.Value = CPU.cpu.regs.mdr;
            yield break;
        }

        public override IEnumerable write(uint addr, byte data)
        {
            yield break;
        }
    }
}
