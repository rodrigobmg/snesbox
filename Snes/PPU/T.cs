#if !FAST_PPU
namespace Snes
{
    partial class PPU
    {
        partial class Background
        {
            public class T
            {
                public uint x;
                public uint mosaic_y;
                public uint mosaic_countdown;
            }
        }
    }

    partial class PPU
    {
        partial class Window
        {
            public class T
            {
                public uint x;
            }
        }
    }
}
#endif
