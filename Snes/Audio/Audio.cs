using Nall;

namespace Snes
{
    class Audio
    {
        public static Audio audio = new Audio();

        public void coprocessor_enable(bool state)
        {
            coprocessor = state;

            dsp_rdoffset = cop_rdoffset = 0;
            dsp_wroffset = cop_wroffset = 0;
            dsp_length = cop_length = 0;

            r_sum_l = r_sum_r = 0;
        }

        public void coprocessor_frequency(double input_frequency)
        {
            double output_frequency;
            output_frequency = System.system.apu_frequency / 768.0;
            r_step = input_frequency / output_frequency;
            r_frac = 0;
        }

        public void sample(short left, short right)
        {
            if (coprocessor == false)
            {
                System.system.Interface.audio_sample((ushort)left, (ushort)right);
            }
            else
            {
                dsp_buffer[dsp_wroffset] = (uint)(((ushort)left << 0) + ((ushort)right << 16));
                dsp_wroffset = (dsp_wroffset + 1) & 32767;
                dsp_length = (dsp_length + 1) & 32767;
                flush();
            }
        }

        public void coprocessor_sample(short left, short right)
        {
            if (r_frac >= 1.0)
            {
                r_frac -= 1.0;
                r_sum_l += left;
                r_sum_r += right;
                return;
            }

            r_sum_l += (int)(left * r_frac);
            r_sum_r += (int)(right * r_frac);

            ushort output_left = (ushort)Bit.sclamp(16, (int)(r_sum_l / r_step));
            ushort output_right = (ushort)Bit.sclamp(16, (int)(r_sum_r / r_step));

            double first = 1.0 - r_frac;
            r_sum_l = (int)(left * first);
            r_sum_r = (int)(right * first);
            r_frac = r_step - first;

            cop_buffer[cop_wroffset] = (uint)((output_left << 0) + (output_right << 16));
            cop_wroffset = (cop_wroffset + 1) & 32767;
            cop_length = (cop_length + 1) & 32767;
            flush();
        }

        public void init() { }

        private bool coprocessor;
        private uint[] dsp_buffer = new uint[32768];
        private uint[] cop_buffer = new uint[32768];
        private uint dsp_rdoffset, cop_rdoffset;
        private uint dsp_wroffset, cop_wroffset;
        private uint dsp_length, cop_length;

        private double r_step, r_frac;
        private int r_sum_l, r_sum_r;

        private void flush()
        {
            while (dsp_length > 0 && cop_length > 0)
            {
                uint dsp_sample = dsp_buffer[dsp_rdoffset];
                uint cop_sample = cop_buffer[cop_rdoffset];

                dsp_rdoffset = (dsp_rdoffset + 1) & 32767;
                cop_rdoffset = (cop_rdoffset + 1) & 32767;

                dsp_length--;
                cop_length--;

                int dsp_left = (short)(dsp_sample >> 0);
                int dsp_right = (short)(dsp_sample >> 16);

                int cop_left = (short)(cop_sample >> 0);
                int cop_right = (short)(cop_sample >> 16);

                System.system.Interface.audio_sample((ushort)Bit.sclamp(16, (dsp_left + cop_left) / 2), (ushort)Bit.sclamp(16, (dsp_right + cop_right) / 2));
            }
        }
    }
}
