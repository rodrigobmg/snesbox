#if ACCURACY || COMPATIBILITY
using Nall;

namespace Snes
{
    partial class CPU
    {
        private class Status
        {
            public bool interrupt_pending;
            public ushort interrupt_vector;

            public uint clock_count;
            public uint line_clocks;

            //timing
            public bool irq_lock;

            public uint dram_refresh_position;
            public bool dram_refreshed;

            public uint hdma_init_position;
            public bool hdma_init_triggered;

            public uint hdma_position;
            public bool hdma_triggered;

            public bool nmi_valid;
            public bool nmi_line;
            public bool nmi_transition;
            public bool nmi_pending;
            public bool nmi_hold;

            public bool irq_valid;
            public bool irq_line;
            public bool irq_transition;
            public bool irq_pending;
            public bool irq_hold;

            public bool reset_pending;

            //DMA
            public bool dma_active;
            public uint dma_counter;
            public uint dma_clocks;
            public bool dma_pending;
            public bool hdma_pending;
            public bool hdma_mode;  //0 = init, 1 = run

            //MMIO
            //$2140-217f
            public byte[] port = new byte[4];

            //$2181-$2183
            public uint17 wram_addr;

            //$4016-$4017
            public bool joypad_strobe_latch;
            public uint joypad1_bits;
            public uint joypad2_bits;

            //$4200
            public bool nmi_enabled;
            public bool hirq_enabled, virq_enabled;
            public bool auto_joypad_poll;

            //$4201
            public byte pio;

            //$4202-$4203
            public byte wrmpya;
            public byte wrmpyb;

            //$4204-$4206
            public ushort wrdiva;
            public byte wrdivb;

            //$4207-$420a
            public uint9 hirq_pos;
            public uint9 virq_pos;

            //$420d
            public uint rom_speed;

            //$4214-$4217
            public ushort rddiv;
            public ushort rdmpy;

            //$4218-$421f
            public byte joy1l, joy1h;
            public byte joy2l, joy2h;
            public byte joy3l, joy3h;
            public byte joy4l, joy4h;
        }
    }
}
#endif