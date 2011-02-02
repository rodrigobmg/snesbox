using System;

namespace Snes
{
    partial class SMP
    {
        class sSMPTimer
        {
            public byte stage0_ticks;
            public byte stage1_ticks;
            public byte stage2_ticks;
            public byte stage3_ticks;
            public bool current_line;
            public bool enabled;
            public byte target;

            public void tick()
            {   //stage 0 increment
                stage0_ticks += (byte)SMP.smp.status.timer_step;
                if (stage0_ticks < timer_frequency)
                {
                    return;
                }
                stage0_ticks -= (byte)timer_frequency;

                //stage 1 increment
                stage1_ticks ^= 1;
                sync_stage1();
            }

            public void sync_stage1()
            {
                bool new_line = Convert.ToBoolean(stage1_ticks);
                if (smp.status.timers_enabled == false)
                {
                    new_line = false;
                }
                if (smp.status.timers_disabled == true)
                {
                    new_line = false;
                }

                bool old_line = current_line;
                current_line = new_line;
                if (old_line != Convert.ToBoolean(1) || new_line != Convert.ToBoolean(0))
                {
                    return;  //only pulse on 1->0 transition
                }
                //stage 2 increment
                if (enabled == false)
                {
                    return;
                }
                stage2_ticks++;
                if (stage2_ticks != target)
                {
                    return;
                }

                //stage 3 increment
                stage2_ticks = 0;
                stage3_ticks++;
                stage3_ticks &= 15;
            }

            private uint timer_frequency;
            public sSMPTimer(uint timer_frequency_)
            {
                timer_frequency = timer_frequency_;
            }
        }
    }
}
