#if PERFORMANCE
namespace Snes
{
    partial class PPU
    {
        private class Display
        {
            public bool interlace;
            public bool overscan;
            public uint width;
            public uint height;
            public uint frameskip;
            public uint framecounter;
        }
    }
}
#endif