using System;
using Microsoft.Xna.Framework;

namespace Snes
{
    public class VideoRefreshEventArgs : EventArgs
    {
        public Color[] Buffer { get; private set; }
        public Rectangle Destination { get; private set; }

        public VideoRefreshEventArgs(Color[] buffer, Rectangle destination)
        {
            Buffer = buffer;
            Destination = destination;
        }
    }
}
