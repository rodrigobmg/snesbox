#if !FAST_PPU
namespace Snes
{
    partial class PPU
    {
        private partial class Sprite
        {
            public class TileItem
            {
                public ushort x;
                public ushort priority;
                public ushort palette;
                public bool hflip;
                public byte d0, d1, d2, d3;
            }
        }
    }
}
#endif