
namespace Snes
{
    interface IMMIO
    {
        byte mmio_read(uint addr);
        void mmio_write(uint addr, byte data);
    }
}
