
namespace Snes
{
    class UnmappedMemory : Memory
    {
        public static UnmappedMemory memory_unmapped = new UnmappedMemory();

        public override uint size()
        {
            return 16 * 1024 * 1024;
        }

        public override byte read(uint addr)
        {
            return CPU.cpu.regs.mdr;
        }

        public override void write(uint addr, byte data) { }
    }
}
