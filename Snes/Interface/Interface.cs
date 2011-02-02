using System;

namespace Snes
{
    interface Interface
    {
        void video_refresh(ArraySegment<ushort> data, uint width, uint height);

        void audio_sample(ushort l_sample, ushort r_sample);

        void input_poll();

        short input_poll(bool port, Input.Device device, uint index, uint id);
    }
}
