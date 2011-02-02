
namespace Snes
{
    class UnmappedMMIO : IMMIO
    {
        public static UnmappedMMIO mmio_unmapped = new UnmappedMMIO();

        public byte mmio_read(uint addr)
        {
            return CPU.cpu.regs.mdr;
        }

        public void mmio_write(uint addr, byte data) { }
    }
}
