using System.Collections;

namespace Snes
{
    interface IMMIO
    {
        IEnumerable mmio_read(uint addr, Result result);
        IEnumerable mmio_write(uint addr, byte data);
    }
}
