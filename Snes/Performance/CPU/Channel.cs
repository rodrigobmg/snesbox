#if PERFORMANCE
using System.Runtime.InteropServices;

namespace Snes
{
    partial class CPU
    {
        private class Channel
        {
            public bool dma_enabled;
            public bool hdma_enabled;

            public bool direction;
            public bool indirect;
            public bool unused;
            public bool reverse_transfer;
            public bool fixed_transfer;
            public byte transfer_mode;

            public byte dest_addr;
            public ushort source_addr;
            public byte source_bank;

            [StructLayout(LayoutKind.Explicit)]
            public class Union
            {
                [FieldOffset(0)]
                public ushort transfer_size;
                [FieldOffset(0)]
                public ushort indirect_addr;
            }
            public Union union = new Union();

            public byte indirect_bank;
            public ushort hdma_addr;
            public byte line_counter;
            public byte unknown;

            public bool hdma_completed;
            public bool hdma_do_transfer;
        }
    }
}
#endif
