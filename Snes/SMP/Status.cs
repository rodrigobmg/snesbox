
namespace Snes
{
    partial class SMP
    {
        class Status
        {
            //timing
            public uint clock_counter;
            public uint dsp_counter;
            public uint timer_step;

            //$00f0
            public byte clock_speed;
            public byte timer_speed;
            public bool timers_enabled;
            public bool ram_disabled;
            public bool ram_writable;
            public bool timers_disabled;

            //$00f1
            public bool iplrom_enabled;

            //$00f2
            public byte dsp_addr;

            //$00f8,$00f9
            public byte ram0;
            public byte ram1;
        }
    }
}
