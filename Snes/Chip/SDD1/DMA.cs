
namespace Snes
{
    partial class SDD1
    {
        private class DMA
        {
            uint addr;       //$43x2-$43x4 -- DMA transfer address
            ushort size;         //$43x5-$43x6 -- DMA transfer size
        }
    }
}
