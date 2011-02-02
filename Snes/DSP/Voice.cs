#if !FAST_DSP
using Nall;

namespace Snes
{
    partial class DSP
    {
        class Voice
        {
            public ModuloArray buffer = new ModuloArray(brr_buf_size);  //decoded samples
            public int buf_pos;     //place in buffer where next samples will be decoded
            public int interp_pos;  //relative fractional position in sample (0x1000 = 1.0)
            public int brr_addr;    //address of current BRR block
            public int brr_offset;  //current decoding offset in BRR block
            public int vbit;        //bitmask for voice: 0x01 for voice 0, 0x02 for voice 1, etc
            public int vidx;        //voice channel register index: 0x00 for voice 0, 0x10 for voice 1, etc
            public int kon_delay;   //KON delay/current setup phase
            public int env_mode;
            public int env;         //current envelope level
            public int t_envx_out;
            public int hidden_env;  //used by GAIN mode 7, very obscure quirk
        }
    }
}
#endif