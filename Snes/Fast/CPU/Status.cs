#if FAST_CPU
namespace Snes
{
    partial class CPU
    {
        private class Status
        {
            public bool nmi_valid;
            public bool nmi_line;
            public bool nmi_transition;
            public bool nmi_pending;

            public bool irq_valid;
            public bool irq_line;
            public bool irq_transition;
            public bool irq_pending;

            public bool irq_lock;
            public bool hdma_pending;

            public uint wram_addr;

            public bool joypad_strobe_latch;

            public bool nmi_enabled;
            public bool virq_enabled;
            public bool hirq_enabled;
            public bool auto_joypad_poll_enabled;

            public byte pio;

            public byte wrmpya;
            public byte wrmpyb;
            public ushort wrdiva;
            public byte wrdivb;

            public ushort htime;
            public ushort vtime;

            public uint rom_speed;

            public ushort rddiv;
            public ushort rdmpy;

            public byte joy1l, joy1h;
            public byte joy2l, joy2h;
            public byte joy3l, joy3h;
            public byte joy4l, joy4h;
        }
    }
}
#endif
