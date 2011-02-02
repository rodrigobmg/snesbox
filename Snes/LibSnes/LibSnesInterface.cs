using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

namespace Snes
{
    class LibSnesInterface : Interface
    {
        public static LibSnesInterface inter = new LibSnesInterface();

        public event EventHandler<VideoRefreshEventArgs> pvideo_refresh = null;
        public event EventHandler<AudioRefreshEventArgs> paudio_sample = null;
        public event EventHandler pinput_poll = null;
        public event EventHandler<InputStateEventArgs> pinput_state = null;

        public void video_refresh(ArraySegment<ushort> data, uint width, uint height)
        {
            if (!ReferenceEquals(pvideo_refresh, null))
            {
                pvideo_refresh(null, Video.GetRefreshEventArgs(data, width, height));
            }

            if (!ReferenceEquals(paudio_sample, null))
            {
                var audioArgs = Audio.GetRefreshEventArgs();
                if (audioArgs.Buffer.Length > 0)
                {
                    paudio_sample(null, audioArgs);
                }
            }
        }

        public void audio_sample(ushort l_sample, ushort r_sample)
        {
            Audio.AddSample(l_sample, r_sample);
        }

        public void input_poll()
        {
            if (!ReferenceEquals(pinput_poll, null))
            {
                pinput_poll(this, EventArgs.Empty);
            }
        }

        public short input_poll(bool port, Input.Device device, uint index, uint id)
        {
            if (!ReferenceEquals(pinput_state, null))
            {
                var args = new InputStateEventArgs((Port)Convert.ToInt32(port), (Device)(device), index, id);
                pinput_state(null, args);
                return args.State;
            }
            return 0;
        }

        static class Video
        {
            private static Color[] _buffer;

            public static VideoRefreshEventArgs GetRefreshEventArgs(ArraySegment<ushort> data, uint width, uint height)
            {
                var interlace = (height >= 240);
                var pitch = interlace ? 1024U : 2048U;
                pitch >>= 1;

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var color = data.Array[data.Offset + ((y * pitch) + x)];
                        var b = ((color >> 10) & 31) * 8;
                        var red = (b + b / 35);
                        b = ((color >> 5) & 31) * 8;
                        var green = (b + b / 35);
                        b = ((color >> 0) & 31) * 8;
                        var blue = (b + b / 35);
                        var alpha = (255);

                        _buffer[y * 512 + x] = new Color(red, green, blue, alpha);
                    }
                }

                return new VideoRefreshEventArgs(_buffer, new Rectangle(0, 0, (int)width, (int)height));
            }

            static Video()
            {
                _buffer = new Color[512 * 512];
            }
        }

        static class Audio
        {
            private static Collection<uint> _buffer;

            public static void AddSample(ushort left, ushort right)
            {
                _buffer.Add((uint)((right << 16) | left));
            }

            public static AudioRefreshEventArgs GetRefreshEventArgs()
            {
                var audioBuffer = new byte[_buffer.Count * 4];
                int bufferIndex = 0;

                for (int i = 0; i < _buffer.Count; i++)
                {
                    var samples = BitConverter.GetBytes(_buffer[i]);
                    audioBuffer[bufferIndex++] = samples[0];
                    audioBuffer[bufferIndex++] = samples[1];
                    audioBuffer[bufferIndex++] = samples[2];
                    audioBuffer[bufferIndex++] = samples[3];
                }

                _buffer.Clear();
                return new AudioRefreshEventArgs(audioBuffer);
            }

            static Audio()
            {
                _buffer = new Collection<uint>();
            }
        }
    }
}
