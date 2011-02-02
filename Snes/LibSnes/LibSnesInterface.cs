using System;

namespace Snes
{
    class LibSnesInterface : Interface
    {
        public static LibSnesInterface inter = new LibSnesInterface();

        public event LibSnes.SnesVideoRefresh pvideo_refresh = null;
        public event LibSnes.SnesAudioSample paudio_sample = null;
        public event LibSnes.SnesInputPoll pinput_poll = null;
        public event LibSnes.SnesInputState pinput_state = null;

        public void video_refresh(ArraySegment<ushort> data, uint width, uint height)
        {
            if (!ReferenceEquals(pvideo_refresh, null))
                pvideo_refresh(data, width, height);
        }

        public void audio_sample(ushort l_sample, ushort r_sample)
        {
            if (!ReferenceEquals(paudio_sample, null))
                paudio_sample(l_sample, r_sample);
        }

        public void input_poll()
        {
            if (!ReferenceEquals(pinput_poll, null))
                pinput_poll();
        }

        public short input_poll(bool port, Input.Device device, uint index, uint id)
        {
            if (!ReferenceEquals(pinput_state, null))
                return pinput_state(port, (uint)device, index, id);
            return 0;
        }
    }
}
