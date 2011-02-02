#if ACCURACY
using System;
using Nall;

namespace Snes
{
    partial class DSP : IProcessor
    {
        public static DSP dsp = new DSP();

        public void step(uint clocks)
        {
            Processor.clock += clocks;
        }

        public void synchronize_smp()
        {
            if (Processor.clock >= 0 && Scheduler.scheduler.sync != Scheduler.SynchronizeMode.All)
            {
                Libco.Switch(SMP.smp.Processor.thread);
            }
        }

        public byte read(byte addr)
        {
            return state.regs[addr];
        }

        public void write(byte addr, byte data)
        {
            state.regs[addr] = data;

            if ((addr & 0x0f) == (int)VoiceReg.envx)
            {
                state.envx_buf = data;
            }
            else if ((addr & 0x0f) == (int)VoiceReg.outx)
            {
                state.outx_buf = data;
            }
            else if (addr == (int)GlobalReg.kon)
            {
                state.new_kon = data;
            }
            else if (addr == (int)GlobalReg.endx)
            {
                //always cleared, regardless of data written
                state.endx_buf = 0;
                state.regs[(int)GlobalReg.endx] = 0;
            }
        }

        public void enter()
        {
            while (true)
            {
                if (Scheduler.scheduler.sync == Scheduler.SynchronizeMode.All)
                {
                    Scheduler.scheduler.exit(Scheduler.ExitReason.SynchronizeEvent);
                }

                voice_5(voice[0]);
                voice_2(voice[1]);
                tick();

                voice_6(voice[0]);
                voice_3(voice[1]);
                tick();

                voice_7(voice[0]);
                voice_4(voice[1]);
                voice_1(voice[3]);
                tick();

                voice_8(voice[0]);
                voice_5(voice[1]);
                voice_2(voice[2]);
                tick();

                voice_9(voice[0]);
                voice_6(voice[1]);
                voice_3(voice[2]);
                tick();

                voice_7(voice[1]);
                voice_4(voice[2]);
                voice_1(voice[4]);
                tick();

                voice_8(voice[1]);
                voice_5(voice[2]);
                voice_2(voice[3]);
                tick();

                voice_9(voice[1]);
                voice_6(voice[2]);
                voice_3(voice[3]);
                tick();

                voice_7(voice[2]);
                voice_4(voice[3]);
                voice_1(voice[5]);
                tick();

                voice_8(voice[2]);
                voice_5(voice[3]);
                voice_2(voice[4]);
                tick();

                voice_9(voice[2]);
                voice_6(voice[3]);
                voice_3(voice[4]);
                tick();

                voice_7(voice[3]);
                voice_4(voice[4]);
                voice_1(voice[6]);
                tick();

                voice_8(voice[3]);
                voice_5(voice[4]);
                voice_2(voice[5]);
                tick();

                voice_9(voice[3]);
                voice_6(voice[4]);
                voice_3(voice[5]);
                tick();

                voice_7(voice[4]);
                voice_4(voice[5]);
                voice_1(voice[7]);
                tick();

                voice_8(voice[4]);
                voice_5(voice[5]);
                voice_2(voice[6]);
                tick();

                voice_9(voice[4]);
                voice_6(voice[5]);
                voice_3(voice[6]);
                tick();

                voice_1(voice[0]);
                voice_7(voice[5]);
                voice_4(voice[6]);
                tick();

                voice_8(voice[5]);
                voice_5(voice[6]);
                voice_2(voice[7]);
                tick();

                voice_9(voice[5]);
                voice_6(voice[6]);
                voice_3(voice[7]);
                tick();

                voice_1(voice[1]);
                voice_7(voice[6]);
                voice_4(voice[7]);
                tick();

                voice_8(voice[6]);
                voice_5(voice[7]);
                voice_2(voice[0]);
                tick();

                voice_3a(voice[0]);
                voice_9(voice[6]);
                voice_6(voice[7]);
                echo_22();
                tick();

                voice_7(voice[7]);
                echo_23();
                tick();

                voice_8(voice[7]);
                echo_24();
                tick();

                voice_3b(voice[0]);
                voice_9(voice[7]);
                echo_25();
                tick();

                echo_26();
                tick();

                misc_27();
                echo_27();
                tick();

                misc_28();
                echo_28();
                tick();

                misc_29();
                echo_29();
                tick();

                misc_30();
                voice_3c(voice[0]);
                echo_30();
                tick();

                voice_4(voice[0]);
                voice_1(voice[2]);
                tick();
            }
        }

        public void power()
        {
            state.regs.Initialize();
            state.echo_hist_pos = 0;
            state.every_other_sample = false;
            state.kon = 0;
            state.noise = 0;
            state.counter = 0;
            state.echo_offset = 0;
            state.echo_length = 0;
            state.new_kon = 0;
            state.endx_buf = 0;
            state.envx_buf = 0;
            state.outx_buf = 0;
            state.t_pmon = 0;
            state.t_non = 0;
            state.t_eon = 0;
            state.t_dir = 0;
            state.t_koff = 0;
            state.t_brr_next_addr = 0;
            state.t_adsr0 = 0;
            state.t_brr_header = 0;
            state.t_brr_byte = 0;
            state.t_srcn = 0;
            state.t_esa = 0;
            state.t_echo_disabled = 0;
            state.t_dir_addr = 0;
            state.t_pitch = 0;
            state.t_output = 0;
            state.t_looped = 0;
            state.t_echo_ptr = 0;
            state.t_main_out[0] = state.t_main_out[1] = 0;
            state.t_echo_out[0] = state.t_echo_out[1] = 0;
            state.t_echo_in[0] = state.t_echo_in[1] = 0;

            for (uint i = 0; i < 8; i++)
            {
                voice[i].buf_pos = 0;
                voice[i].interp_pos = 0;
                voice[i].brr_addr = 0;
                voice[i].brr_offset = 1;
                voice[i].vbit = 1 << (int)i;
                voice[i].vidx = (int)(i * 0x10);
                voice[i].kon_delay = 0;
                voice[i].env_mode = (int)EnvMode.release;
                voice[i].env = 0;
                voice[i].t_envx_out = 0;
                voice[i].hidden_env = 0;
            }

            reset();
        }

        public void reset()
        {
            Processor.create("DSP", Enter, System.system.apu_frequency);

            state.regs[(int)GlobalReg.flg] = 0xe0;

            state.noise = 0x4000;
            state.echo_hist_pos = 0;
            state.every_other_sample = Convert.ToBoolean(1);
            state.echo_offset = 0;
            state.counter = 0;
        }

        public void serialize(Serializer s)
        {
            Processor.serialize(s);

            s.array(state.regs, 128, "state.regs");
            state.echo_hist[0].serialize(s);
            state.echo_hist[1].serialize(s);
            s.integer(state.echo_hist_pos, "state.echo_hist_pos");

            s.integer(state.every_other_sample, "state.every_other_sample");
            s.integer(state.kon, "state.kon");
            s.integer(state.noise, "state.noise");
            s.integer(state.counter, "state.counter");
            s.integer(state.echo_offset, "state.echo_offset");
            s.integer(state.echo_length, "state.echo_length");

            s.integer(state.new_kon, "state.new_kon");
            s.integer(state.endx_buf, "state.endx_buf");
            s.integer(state.envx_buf, "state.envx_buf");
            s.integer(state.outx_buf, "state.outx_buf");

            s.integer(state.t_pmon, "state.t_pmon");
            s.integer(state.t_non, "state.t_non");
            s.integer(state.t_eon, "state.t_eon");
            s.integer(state.t_dir, "state.t_dir");
            s.integer(state.t_koff, "state.t_koff");

            s.integer(state.t_brr_next_addr, "state.t_brr_next_addr");
            s.integer(state.t_adsr0, "state.t_adsr0");
            s.integer(state.t_brr_header, "state.t_brr_header");
            s.integer(state.t_brr_byte, "state.t_brr_byte");
            s.integer(state.t_srcn, "state.t_srcn");
            s.integer(state.t_esa, "state.t_esa");
            s.integer(state.t_echo_disabled, "state.t_echo_disabled");

            s.integer(state.t_dir_addr, "state.t_dir_addr");
            s.integer(state.t_pitch, "state.t_pitch");
            s.integer(state.t_output, "state.t_output");
            s.integer(state.t_looped, "state.t_looped");
            s.integer(state.t_echo_ptr, "state.t_echo_ptr");

            s.integer(state.t_main_out[0], "state.t_main_out[0]");
            s.integer(state.t_main_out[1], "state.t_main_out[1]");
            s.integer(state.t_echo_out[0], "state.t_echo_out[0]");
            s.integer(state.t_echo_out[1], "state.t_echo_out[1]");
            s.integer(state.t_echo_in[0], "state.t_echo_in [0]");
            s.integer(state.t_echo_in[1], "state.t_echo_in [1]");

            for (uint n = 0; n < 8; n++)
            {
                voice[n].buffer.serialize(s);
                s.integer(voice[n].buf_pos, "voice[n].buf_pos");
                s.integer(voice[n].interp_pos, "voice[n].interp_pos");
                s.integer(voice[n].brr_addr, "voice[n].brr_addr");
                s.integer(voice[n].brr_offset, "voice[n].brr_offset");
                s.integer(voice[n].vbit, "voice[n].vbit");
                s.integer(voice[n].vidx, "voice[n].vidx");
                s.integer(voice[n].kon_delay, "voice[n].kon_delay");
                s.integer(voice[n].env_mode, "voice[n].env_mode");
                s.integer(voice[n].env, "voice[n].env");
                s.integer(voice[n].t_envx_out, "voice[n].t_envx_out");
                s.integer(voice[n].hidden_env, "voice[n].hidden_env");
            }
        }

        public DSP()
        {
#if DEBUG
            unchecked
            {
                Debug.Assert(sizeof(int) >= 32 / 8, "int >= 32-bits");
                Debug.Assert((sbyte)0x80 == -0x80, "8-bit sign extension");
                Debug.Assert((short)0x8000 == -0x8000, "16-bit sign extension");
                Debug.Assert((ushort)0xffff0000 == 0, "16-bit uint clip");
                Debug.Assert((-1 >> 1) == -1, "arithmetic shift right");

                //-0x8000 <= n <= +0x7fff
                Debug.Assert(Bit.sclamp(16, +0x8000) == +0x7fff);
                Debug.Assert(Bit.sclamp(16, -0x8001) == -0x8000);
            }
#endif

            for (int i = 0; i < voice.Length; i++)
            {
                voice[i] = new Voice();
            }
        }

        private enum GlobalReg { mvoll = 0x0c, mvolr = 0x1c, evoll = 0x2c, evolr = 0x3c, kon = 0x4c, koff = 0x5c, flg = 0x6c, endx = 0x7c, efb = 0x0d, pmon = 0x2d, non = 0x3d, eon = 0x4d, dir = 0x5d, esa = 0x6d, edl = 0x7d, fir = 0x0f }
        //voice registers
        private enum VoiceReg { voll = 0x00, volr = 0x01, pitchl = 0x02, pitchh = 0x03, srcn = 0x04, adsr0 = 0x05, adsr1 = 0x06, gain = 0x07, envx = 0x08, outx = 0x09 }

        //internal envelope modes
        private enum EnvMode { release, attack, decay, sustain }

        //internal constants
        private const int echo_hist_size = 8;
        private const int brr_buf_size = 12;
        private const int brr_block_size = 9;

        private State state = new State();

        //voice state
        private Voice[] voice = new Voice[8];

        //gaussian
        private static readonly short[] gaussian_table = new short[512]
        {
            0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
            1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    2,    2,    2,    2,    2,
            2,    2,    3,    3,    3,    3,    3,    4,    4,    4,    4,    4,    5,    5,    5,    5,
            6,    6,    6,    6,    7,    7,    7,    8,    8,    8,    9,    9,    9,   10,   10,   10,
            11,   11,   11,   12,   12,   13,   13,   14,   14,   15,   15,   15,   16,   16,   17,   17,
            18,   19,   19,   20,   20,   21,   21,   22,   23,   23,   24,   24,   25,   26,   27,   27,
            28,   29,   29,   30,   31,   32,   32,   33,   34,   35,   36,   36,   37,   38,   39,   40,
            41,   42,   43,   44,   45,   46,   47,   48,   49,   50,   51,   52,   53,   54,   55,   56,
            58,   59,   60,   61,   62,   64,   65,   66,   67,   69,   70,   71,   73,   74,   76,   77,
            78,   80,   81,   83,   84,   86,   87,   89,   90,   92,   94,   95,   97,   99,  100,  102,
            104,  106,  107,  109,  111,  113,  115,  117,  118,  120,  122,  124,  126,  128,  130,  132,
            134,  137,  139,  141,  143,  145,  147,  150,  152,  154,  156,  159,  161,  163,  166,  168,
            171,  173,  175,  178,  180,  183,  186,  188,  191,  193,  196,  199,  201,  204,  207,  210,
            212,  215,  218,  221,  224,  227,  230,  233,  236,  239,  242,  245,  248,  251,  254,  257,
            260,  263,  267,  270,  273,  276,  280,  283,  286,  290,  293,  297,  300,  304,  307,  311,
            314,  318,  321,  325,  328,  332,  336,  339,  343,  347,  351,  354,  358,  362,  366,  370,
            374,  378,  381,  385,  389,  393,  397,  401,  405,  410,  414,  418,  422,  426,  430,  434,
            439,  443,  447,  451,  456,  460,  464,  469,  473,  477,  482,  486,  491,  495,  499,  504,
            508,  513,  517,  522,  527,  531,  536,  540,  545,  550,  554,  559,  563,  568,  573,  577,
            582,  587,  592,  596,  601,  606,  611,  615,  620,  625,  630,  635,  640,  644,  649,  654,
            659,  664,  669,  674,  678,  683,  688,  693,  698,  703,  708,  713,  718,  723,  728,  732,
            737,  742,  747,  752,  757,  762,  767,  772,  777,  782,  787,  792,  797,  802,  806,  811,
            816,  821,  826,  831,  836,  841,  846,  851,  855,  860,  865,  870,  875,  880,  884,  889,
            894,  899,  904,  908,  913,  918,  923,  927,  932,  937,  941,  946,  951,  955,  960,  965,
            969,  974,  978,  983,  988,  992,  997, 1001, 1005, 1010, 1014, 1019, 1023, 1027, 1032, 1036,
            1040, 1045, 1049, 1053, 1057, 1061, 1066, 1070, 1074, 1078, 1082, 1086, 1090, 1094, 1098, 1102,
            1106, 1109, 1113, 1117, 1121, 1125, 1128, 1132, 1136, 1139, 1143, 1146, 1150, 1153, 1157, 1160,
            1164, 1167, 1170, 1174, 1177, 1180, 1183, 1186, 1190, 1193, 1196, 1199, 1202, 1205, 1207, 1210,
            1213, 1216, 1219, 1221, 1224, 1227, 1229, 1232, 1234, 1237, 1239, 1241, 1244, 1246, 1248, 1251,
            1253, 1255, 1257, 1259, 1261, 1263, 1265, 1267, 1269, 1270, 1272, 1274, 1275, 1277, 1279, 1280,
            1282, 1283, 1284, 1286, 1287, 1288, 1290, 1291, 1292, 1293, 1294, 1295, 1296, 1297, 1297, 1298,
            1299, 1300, 1300, 1301, 1302, 1302, 1303, 1303, 1303, 1304, 1304, 1304, 1304, 1304, 1305, 1305,
        };

        private int gaussian_interpolate(Voice v)
        {   //make pointers into gaussian table based on fractional position between samples
            int offset = (v.interp_pos >> 4) & 0xff;
            var fwd = new ArraySegment<short>(gaussian_table, 255 - offset, gaussian_table.Length - (255 - offset));
            var rev = new ArraySegment<short>(gaussian_table, offset, gaussian_table.Length - offset); //mirror left half of gaussian table

            offset = v.buf_pos + (v.interp_pos >> 12);
            int output;
            output = (fwd.Array[fwd.Offset + 0] * v.buffer[offset + 0]) >> 11;
            output += (fwd.Array[fwd.Offset + 256] * v.buffer[offset + 1]) >> 11;
            output += (rev.Array[rev.Offset + 256] * v.buffer[offset + 2]) >> 11;
            output = (short)output;
            output += (rev.Array[rev.Offset + 0] * v.buffer[offset + 3]) >> 11;
            return Bit.sclamp(16, output) & ~1;
        }

        //counter
        private const int counter_range = 2048 * 5 * 3; //30720 (0x7800)

        private static readonly ushort[] counter_rate = new ushort[32]
        {
            0, 2048, 1536,
            1280, 1024,  768,
            640,  512,  384,
            320,  256,  192,
            160,  128,   96,
            80,   64,   48,
            40,   32,   24,
            20,   16,   12,
            10,    8,    6,
            5,    4,    3,
            2,
            1,
        };

        private static readonly ushort[] counter_offset = new ushort[32]
        {
            0, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            0,
            0,
        };

        private void counter_tick()
        {
            state.counter--;
            if (state.counter < 0)
            {
                state.counter = counter_range - 1;
            }
        }

        private bool counter_poll(uint rate)
        {
            if (rate == 0)
            {
                return false;
            }
            return (((uint)state.counter + counter_offset[rate]) % counter_rate[rate]) == 0;
        }

        //envelope
        private void envelope_run(Voice v)
        {
            int env = v.env;

            if (v.env_mode == (int)EnvMode.release)
            { //60%
                env -= 0x8;
                if (env < 0)
                {
                    env = 0;
                }
                v.env = env;
                return;
            }

            int rate;
            int env_data = state.regs[v.vidx + (int)VoiceReg.adsr1];
            if (Convert.ToBoolean(state.t_adsr0 & 0x80))
            { //99% ADSR
                if (v.env_mode >= (int)EnvMode.decay)
                { //99%
                    env--;
                    env -= env >> 8;
                    rate = env_data & 0x1f;
                    if (v.env_mode == (int)EnvMode.decay)
                    { //1%
                        rate = ((state.t_adsr0 >> 3) & 0x0e) + 0x10;
                    }
                }
                else
                { //env_attack
                    rate = ((state.t_adsr0 & 0x0f) << 1) + 1;
                    env += rate < 31 ? 0x20 : 0x400;
                }
            }
            else
            { //GAIN
                env_data = state.regs[v.vidx + (int)VoiceReg.gain];
                int mode = env_data >> 5;
                if (mode < 4)
                { //direct
                    env = env_data << 4;
                    rate = 31;
                }
                else
                {
                    rate = env_data & 0x1f;
                    if (mode == 4)
                    { //4: linear decrease
                        env -= 0x20;
                    }
                    else if (mode < 6)
                    { //5: exponential decrease
                        env--;
                        env -= env >> 8;
                    }
                    else
                    { //6, 7: linear increase
                        env += 0x20;
                        if (mode > 6 && (uint)v.hidden_env >= 0x600)
                        {
                            env += 0x8 - 0x20; //7: two-slope linear increase
                        }
                    }
                }
            }

            //sustain level
            if ((env >> 8) == (env_data >> 5) && v.env_mode == (int)EnvMode.decay)
            {
                v.env_mode = (int)EnvMode.sustain;
            }
            v.hidden_env = env;

            //unsigned cast because linear decrease underflowing also triggers this
            if ((uint)env > 0x7ff)
            {
                env = (env < 0 ? 0 : 0x7ff);
                if (v.env_mode == (int)EnvMode.attack)
                {
                    v.env_mode = (int)EnvMode.decay;
                }
            }

            if (counter_poll((uint)rate) == true)
            {
                v.env = env;
            }
        }

        //brr
        private void brr_decode(Voice v)
        {   //state.t_brr_byte = ram[v.brr_addr + v.brr_offset] cached from previous clock cycle
            int nybbles = (state.t_brr_byte << 8) + StaticRAM.apuram[(ushort)(v.brr_addr + v.brr_offset + 1)];

            int filter = (state.t_brr_header >> 2) & 3;
            int scale = (state.t_brr_header >> 4);

            //decode four samples
            for (uint i = 0; i < 4; i++)
            {
                //bits 12-15 = current nybble; sign extend, then shift right to 4-bit precision
                //result: s = 4-bit sign-extended sample value
                int s = (short)nybbles >> 12;
                nybbles <<= 4; //slide nybble so that on next loop iteration, bits 12-15 = current nybble

                if (scale <= 12)
                {
                    s <<= scale;
                    s >>= 1;
                }
                else
                {
                    s &= ~0x7ff;
                }

                //apply IIR filter (2 is the most commonly used)
                int p1 = v.buffer[v.buf_pos - 1];
                int p2 = v.buffer[v.buf_pos - 2] >> 1;

                switch (filter)
                {
                    case 0:
                        break; //no filter
                    case 1:
                        {
                            //s += p1 * 0.46875
                            s += p1 >> 1;
                            s += (-p1) >> 5;
                        }
                        break;
                    case 2:
                        {
                            //s += p1 * 0.953125 - p2 * 0.46875
                            s += p1;
                            s -= p2;
                            s += p2 >> 4;
                            s += (p1 * -3) >> 6;
                        }
                        break;
                    case 3:
                        {
                            //s += p1 * 0.8984375 - p2 * 0.40625
                            s += p1;
                            s -= p2;
                            s += (p1 * -13) >> 7;
                            s += (p2 * 3) >> 4;
                        }
                        break;
                }

                //adjust and write sample
                s = Bit.sclamp(16, s);
                s = (short)(s << 1);
                v.buffer.write((uint)v.buf_pos++, s);
                if (v.buf_pos >= brr_buf_size)
                {
                    v.buf_pos = 0;
                }
            }
        }

        //misc
        private void misc_27()
        {
            state.t_pmon = state.regs[(int)GlobalReg.pmon] & ~1; //voice 0 doesn't support PMON
        }

        private void misc_28()
        {
            state.t_non = state.regs[(int)GlobalReg.non];
            state.t_eon = state.regs[(int)GlobalReg.eon];
            state.t_dir = state.regs[(int)GlobalReg.dir];
        }

        private void misc_29()
        {
            state.every_other_sample = Convert.ToBoolean(Convert.ToInt32(state.every_other_sample) ^ 1);
            if (state.every_other_sample)
            {
                state.new_kon &= ~state.kon; //clears KON 63 clocks after it was last read
            }
        }

        private void misc_30()
        {
            if (state.every_other_sample)
            {
                state.kon = state.new_kon;
                state.t_koff = state.regs[(int)GlobalReg.koff];
            }

            counter_tick();

            //noise
            if (counter_poll((uint)(state.regs[(int)GlobalReg.flg] & 0x1f)) == true)
            {
                int feedback = (state.noise << 13) ^ (state.noise << 14);
                state.noise = (feedback & 0x4000) ^ (state.noise >> 1);
            }
        }

        //voice
        private void voice_output(Voice v, bool channel)
        {   //apply left/right volume
            int amp = (state.t_output * (sbyte)(state.regs[v.vidx + (int)VoiceReg.voll + Convert.ToInt32(channel)])) >> 7;

            //add to output total
            state.t_main_out[Convert.ToInt32(channel)] += amp;
            state.t_main_out[Convert.ToInt32(channel)] = Bit.sclamp(16, state.t_main_out[Convert.ToInt32(channel)]);

            //optionally add to echo total
            if (Convert.ToBoolean(state.t_eon & v.vbit))
            {
                state.t_echo_out[Convert.ToInt32(channel)] += amp;
                state.t_echo_out[Convert.ToInt32(channel)] = Bit.sclamp(16, state.t_echo_out[Convert.ToInt32(channel)]);
            }
        }

        private void voice_1(Voice v)
        {
            state.t_dir_addr = (state.t_dir << 8) + (state.t_srcn << 2);
            state.t_srcn = state.regs[v.vidx + (int)VoiceReg.srcn];
        }

        private void voice_2(Voice v)
        {   //read sample pointer (ignored if not needed)
            ushort addr = (ushort)state.t_dir_addr;
            if (!Convert.ToBoolean(v.kon_delay))
            {
                addr += 2;
            }
            byte lo = StaticRAM.apuram[(ushort)(addr + 0)];
            byte hi = StaticRAM.apuram[(ushort)(addr + 1)];
            state.t_brr_next_addr = ((hi << 8) + lo);

            state.t_adsr0 = state.regs[v.vidx + (int)VoiceReg.adsr0];

            //read pitch, spread over two clocks
            state.t_pitch = state.regs[v.vidx + (int)VoiceReg.pitchl];
        }

        private void voice_3(Voice v)
        {
            voice_3a(v);
            voice_3b(v);
            voice_3c(v);
        }

        private void voice_3a(Voice v)
        {
            state.t_pitch += (state.regs[v.vidx + (int)VoiceReg.pitchh] & 0x3f) << 8;
        }

        private void voice_3b(Voice v)
        {
            state.t_brr_byte = StaticRAM.apuram[(ushort)(v.brr_addr + v.brr_offset)];
            state.t_brr_header = StaticRAM.apuram[(ushort)(v.brr_addr)];
        }

        private void voice_3c(Voice v)
        {   //pitch modulation using previous voice's output

            if (Convert.ToBoolean(state.t_pmon & v.vbit))
            {
                state.t_pitch += ((state.t_output >> 5) * state.t_pitch) >> 10;
            }

            if (Convert.ToBoolean(v.kon_delay))
            {
                //get ready to start BRR decoding on next sample
                if (v.kon_delay == 5)
                {
                    v.brr_addr = state.t_brr_next_addr;
                    v.brr_offset = 1;
                    v.buf_pos = 0;
                    state.t_brr_header = 0; //header is ignored on this sample
                }

                //envelope is never run during KON
                v.env = 0;
                v.hidden_env = 0;

                //disable BRR decoding until last three samples
                v.interp_pos = 0;
                v.kon_delay--;
                if (Convert.ToBoolean(v.kon_delay & 3))
                {
                    v.interp_pos = 0x4000;
                }

                //pitch is never added during KON
                state.t_pitch = 0;
            }

            //gaussian interpolation
            int output = gaussian_interpolate(v);

            //noise
            if (Convert.ToBoolean(state.t_non & v.vbit))
            {
                output = (short)(state.noise << 1);
            }

            //apply envelope
            state.t_output = ((output * v.env) >> 11) & ~1;
            v.t_envx_out = v.env >> 4;

            //immediate silence due to end of sample or soft reset
            if (Convert.ToBoolean(state.regs[(int)GlobalReg.flg] & 0x80) || (state.t_brr_header & 3) == 1)
            {
                v.env_mode = (int)EnvMode.release;
                v.env = 0;
            }

            if (state.every_other_sample)
            {
                //KOFF
                if (Convert.ToBoolean(state.t_koff & v.vbit))
                {
                    v.env_mode = (int)EnvMode.release;
                }

                //KON
                if (Convert.ToBoolean(state.kon & v.vbit))
                {
                    v.kon_delay = 5;
                    v.env_mode = (int)EnvMode.attack;
                }
            }

            //run envelope for next sample
            if (!Convert.ToBoolean(v.kon_delay))
            {
                envelope_run(v);
            }
        }

        private void voice_4(Voice v)
        {   //decode BRR
            state.t_looped = 0;
            if (v.interp_pos >= 0x4000)
            {
                brr_decode(v);
                v.brr_offset += 2;
                if (v.brr_offset >= 9)
                {
                    //start decoding next BRR block
                    v.brr_addr = (ushort)(v.brr_addr + 9);
                    if (Convert.ToBoolean(state.t_brr_header & 1))
                    {
                        v.brr_addr = state.t_brr_next_addr;
                        state.t_looped = v.vbit;
                    }
                    v.brr_offset = 1;
                }
            }

            //apply pitch
            v.interp_pos = (v.interp_pos & 0x3fff) + state.t_pitch;

            //keep from getting too far ahead (when using pitch modulation)
            if (v.interp_pos > 0x7fff)
            {
                v.interp_pos = 0x7fff;
            }

            //output left
            voice_output(v, Convert.ToBoolean(0));
        }

        private void voice_5(Voice v)
        {   //output right
            voice_output(v, Convert.ToBoolean(1));

            //ENDX, OUTX and ENVX won't update if you wrote to them 1-2 clocks earlier
            state.endx_buf = state.regs[(int)GlobalReg.endx] | state.t_looped;

            //clear bit in ENDX if KON just began
            if (v.kon_delay == 5)
            {
                state.endx_buf &= ~v.vbit;
            }
        }

        private void voice_6(Voice v)
        {
            state.outx_buf = state.t_output >> 8;
        }

        private void voice_7(Voice v)
        {   //update ENDX
            state.regs[(int)GlobalReg.endx] = (byte)state.endx_buf;
            state.envx_buf = v.t_envx_out;
        }

        private void voice_8(Voice v)
        {   //update OUTX
            state.regs[v.vidx + (int)VoiceReg.outx] = (byte)state.outx_buf;
        }

        private void voice_9(Voice v)
        {   //update ENVX
            state.regs[v.vidx + (int)VoiceReg.envx] = (byte)state.envx_buf;
        }

        //echo
        private int calc_fir(int i, bool channel)
        {
            int s = state.echo_hist[Convert.ToInt32(channel)][state.echo_hist_pos + i + 1];
            return (s * (sbyte)(state.regs[(int)GlobalReg.fir + i * 0x10])) >> 6;
        }

        private int echo_output(bool channel)
        {
            int output = (short)((state.t_main_out[Convert.ToInt32(channel)] * (sbyte)(state.regs[(int)GlobalReg.mvoll + Convert.ToInt32(channel) * 0x10])) >> 7)
                + (short)((state.t_echo_in[Convert.ToInt32(channel)] * (sbyte)(state.regs[(int)GlobalReg.evoll + Convert.ToInt32(channel) * 0x10])) >> 7);
            return Bit.sclamp(16, output);
        }

        private void echo_read(bool channel)
        {
            uint addr = (uint)(state.t_echo_ptr + Convert.ToInt32(channel) * 2);
            byte lo = StaticRAM.apuram[(ushort)(addr + 0)];
            byte hi = StaticRAM.apuram[(ushort)(addr + 1)];
            int s = (short)((hi << 8) + lo);
            state.echo_hist[Convert.ToInt32(channel)].write((uint)state.echo_hist_pos, s >> 1);
        }

        private void echo_write(bool channel)
        {
            if (!(Convert.ToBoolean(state.t_echo_disabled & 0x20)))
            {
                uint addr = (uint)(state.t_echo_ptr + Convert.ToInt32(channel) * 2);
                int s = state.t_echo_out[Convert.ToInt32(channel)];
                StaticRAM.apuram[(ushort)(addr + 0)] = (byte)s;
                StaticRAM.apuram[(ushort)(addr + 1)] = (byte)(s >> 8);
            }

            state.t_echo_out[Convert.ToInt32(channel)] = 0;
        }

        private void echo_22()
        {   //history
            state.echo_hist_pos++;
            if (state.echo_hist_pos >= echo_hist_size)
            {
                state.echo_hist_pos = 0;
            }

            state.t_echo_ptr = (ushort)((state.t_esa << 8) + state.echo_offset);
            echo_read(Convert.ToBoolean(0));

            //FIR
            int l = calc_fir(0, Convert.ToBoolean(0));
            int r = calc_fir(0, Convert.ToBoolean(1));

            state.t_echo_in[0] = l;
            state.t_echo_in[1] = r;
        }

        private void echo_23()
        {
            int l = calc_fir(1, Convert.ToBoolean(0)) + calc_fir(2, Convert.ToBoolean(0));
            int r = calc_fir(1, Convert.ToBoolean(1)) + calc_fir(2, Convert.ToBoolean(1));

            state.t_echo_in[0] += l;
            state.t_echo_in[1] += r;

            echo_read(Convert.ToBoolean(1));
        }

        private void echo_24()
        {
            int l = calc_fir(3, Convert.ToBoolean(0)) + calc_fir(4, Convert.ToBoolean(0)) + calc_fir(5, Convert.ToBoolean(0));
            int r = calc_fir(3, Convert.ToBoolean(1)) + calc_fir(4, Convert.ToBoolean(1)) + calc_fir(5, Convert.ToBoolean(1));

            state.t_echo_in[0] += l;
            state.t_echo_in[1] += r;
        }

        private void echo_25()
        {
            int l = state.t_echo_in[0] + calc_fir(6, Convert.ToBoolean(0));
            int r = state.t_echo_in[1] + calc_fir(6, Convert.ToBoolean(1));

            l = (short)l;
            r = (short)r;

            l += (short)calc_fir(7, Convert.ToBoolean(0));
            r += (short)calc_fir(7, Convert.ToBoolean(1));

            state.t_echo_in[0] = Bit.sclamp(16, l) & ~1;
            state.t_echo_in[1] = Bit.sclamp(16, r) & ~1;
        }

        private void echo_26()
        {   //left output volumes
            //(save sample for next clock so we can output both together)
            state.t_main_out[0] = echo_output(Convert.ToBoolean(0));

            //echo feedback
            int l = state.t_echo_out[0] + (short)((state.t_echo_in[0] * (sbyte)state.regs[(int)GlobalReg.efb]) >> 7);
            int r = state.t_echo_out[1] + (short)((state.t_echo_in[1] * (sbyte)state.regs[(int)GlobalReg.efb]) >> 7);

            state.t_echo_out[0] = Bit.sclamp(16, l) & ~1;
            state.t_echo_out[1] = Bit.sclamp(16, r) & ~1;
        }

        private void echo_27()
        {   //output
            int outl = state.t_main_out[0];
            int outr = echo_output(Convert.ToBoolean(1));
            state.t_main_out[0] = 0;
            state.t_main_out[1] = 0;

            //global muting isn't this simple
            //(turns DAC on and off or something, causing small ~37-sample pulse when first muted)
            if (Convert.ToBoolean(state.regs[(int)GlobalReg.flg] & 0x40))
            {
                outl = 0;
                outr = 0;
            }

            //output sample to DAC
            Audio.audio.sample((short)outl, (short)outr);
        }

        private void echo_28()
        {
            state.t_echo_disabled = state.regs[(int)GlobalReg.flg];
        }

        private void echo_29()
        {
            state.t_esa = state.regs[(int)GlobalReg.esa];

            if (!Convert.ToBoolean(state.echo_offset))
            {
                state.echo_length = (state.regs[(int)GlobalReg.edl] & 0x0f) << 11;
            }

            state.echo_offset += 4;
            if (state.echo_offset >= state.echo_length)
            {
                state.echo_offset = 0;
            }

            //write left echo
            echo_write(Convert.ToBoolean(0));

            state.t_echo_disabled = state.regs[(int)GlobalReg.flg];
        }

        private void echo_30()
        {   //write right echo
            echo_write(Convert.ToBoolean(1));
        }

        //dsp
        private static void Enter()
        {
            dsp.enter();
        }

        private void tick()
        {
            step(3 * 8);
            synchronize_smp();
        }

        private Processor _processor = new Processor();
        public Processor Processor
        {
            get
            {
                return _processor;
            }
        }
    }
}
#endif