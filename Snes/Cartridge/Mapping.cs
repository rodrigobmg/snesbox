
namespace Snes
{
    partial class Cartridge
    {
        public class Mapping
        {
            public Memory memory;
            public IMMIO mmio;
            public Bus.MapMode mode;
            public uint banklo;
            public uint bankhi;
            public uint addrlo;
            public uint addrhi;
            public uint offset;
            public uint size;

            public Mapping()
            {
                memory = null;
                mmio = null;
                mode = Bus.MapMode.Direct;
                banklo = bankhi = addrlo = addrhi = offset = size = 0;
            }

            public Mapping(Memory memory_)
            {
                memory = memory_;
                mmio = null;
                mode = Bus.MapMode.Direct;
                banklo = bankhi = addrlo = addrhi = offset = size = 0;
            }

            public Mapping(IMMIO mmio_)
            {
                memory = null;
                mmio = mmio_;
                mode = Bus.MapMode.Direct;
                banklo = bankhi = addrlo = addrhi = offset = size = 0;
            }
        }
    }
}
