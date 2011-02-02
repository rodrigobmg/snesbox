#if ACCURACY
using Nall;

namespace Snes
{
    partial class DSP
    {
        class State
        {
            public byte[] regs = new byte[128];

            public ModuloArray[] echo_hist = new ModuloArray[2];  //echo history keeps most recent 8 samples
            public int echo_hist_pos;

            public bool every_other_sample;  //toggles every sample
            public int kon;                  //KON value when last checked
            public int noise;
            public int counter;
            public int echo_offset;          //offset from ESA in echo buffer
            public int echo_length;          //number of bytes that echo_offset will stop at

            //hidden registers also written to when main register is written to
            public int new_kon;
            public int endx_buf;
            public int envx_buf;
            public int outx_buf;

            //temporary state between clocks

            //read once per sample
            public int t_pmon;
            public int t_non;
            public int t_eon;
            public int t_dir;
            public int t_koff;

            //read a few clocks ahead before used
            public int t_brr_next_addr;
            public int t_adsr0;
            public int t_brr_header;
            public int t_brr_byte;
            public int t_srcn;
            public int t_esa;
            public int t_echo_disabled;

            //internal state that is recalculated every sample
            public int t_dir_addr;
            public int t_pitch;
            public int t_output;
            public int t_looped;
            public int t_echo_ptr;

            //left/right sums
            public int[] t_main_out = new int[2];
            public int[] t_echo_out = new int[2];
            public int[] t_echo_in = new int[2];

            public State()
            {
                for (int i = 0; i < echo_hist.Length; i++)
                {
                    echo_hist[i] = new ModuloArray(echo_hist_size);
                }
            }
        }
    }
}
#endif