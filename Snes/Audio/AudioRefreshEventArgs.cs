using System;

namespace Snes
{
    public class AudioRefreshEventArgs : EventArgs
    {
        public byte[] Buffer { get; private set; }

        public AudioRefreshEventArgs(byte[] buffer)
        {
            Buffer = buffer;
        }
    }
}
