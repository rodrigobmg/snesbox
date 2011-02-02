#if !FAST_CPU
using System.Runtime.InteropServices;
using Nall;

namespace Snes
{
    partial class CPU
    {
        private class Channel
        {
            //$420b
            public bool dma_enabled;

            //$420c
            public bool hdma_enabled;

            //$43x0
            public bool direction;
            public bool indirect;
            public bool unused;
            public bool reverse_transfer;
            public bool fixed_transfer;
            public uint3 transfer_mode;

            //$43x1
            public byte dest_addr;

            //$43x2-$43x3
            public ushort source_addr;

            //$43x4
            public byte source_bank;

            //$43x5-$43x6
            [StructLayout(LayoutKind.Explicit)]
            public class Union
            {
                [FieldOffset(0)]
                public ushort transfer_size;
                [FieldOffset(0)]
                public ushort indirect_addr;
            }
            public Union union = new Union();

            //$43x7
            public byte indirect_bank;

            //$43x8-$43x9
            public ushort hdma_addr;

            //$43xa
            public byte line_counter;

            //$43xb/$43xf
            public byte unknown;

            //internal state
            public bool hdma_completed;
            public bool hdma_do_transfer;
        }
    }
}
#endif