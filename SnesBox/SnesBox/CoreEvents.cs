using System;

namespace SnesBox
{
    public class AudioUpdatedEventArgs : EventArgs
    {
        uint[] _audioBuffer;
        int _sampleCount;

        public AudioUpdatedEventArgs(uint[] audioBuffer, int sampleCount)
        {
            _audioBuffer = audioBuffer;
            _sampleCount = sampleCount;
        }

        public uint[] AudioBuffer { get { return _audioBuffer; } }
        public int SampleCount { get { return _sampleCount; } }
    }

    public class VideoUpdatedEventArgs : EventArgs
    {
        ArraySegment<ushort> _videoBuffer;
        int _width;
        int _height;

        public VideoUpdatedEventArgs(ArraySegment<ushort> videoBuffer, int width, int height)
        {
            _videoBuffer = videoBuffer;
            _width = width;
            _height = height;
        }

        public ArraySegment<ushort> VideoBuffer { get { return _videoBuffer; } }
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
    }

    public delegate void AudioUpdatedEventHandler(object sender, AudioUpdatedEventArgs e);
    public delegate void VideoUpdatedEventHandler(object sender, VideoUpdatedEventArgs e);
}
