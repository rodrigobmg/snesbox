#if COMPATIBILITY
namespace Snes
{
    partial class PPU
    {
        public class SpriteItem
        {
            public byte width, height;
            public ushort x, y;
            public byte character;
            public bool use_nameselect;
            public bool vflip, hflip;
            public byte palette;
            public byte priority;
            public bool size;
        }
    }
}
#endif
