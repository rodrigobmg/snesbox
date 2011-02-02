#if ACCURACY
namespace Snes
{
    partial class PPU
    {
        partial class Background
        {
            public class Output
            {
                public class Pixel
                {
                    public uint priority;  //0 = none (transparent)
                    public byte palette;
                    public ushort tile;
                }
                public Pixel main = new Pixel();
                public Pixel sub = new Pixel();
            }
        }
    }

    partial class PPU
    {
        partial class Sprite
        {
            public class Output
            {
                public class Flag
                {
                    public uint priority;  //0 = none (transparent)
                    public byte palette;
                }
                public Flag main = new Flag();
                public Flag sub = new Flag();
            }
        }
    }

    partial class PPU
    {
        partial class Window
        {
            public class Output
            {
                public class Flag
                {
                    public bool color_enable;
                }
                public Flag main = new Flag();
                public Flag sub = new Flag();
            }
        }
    }
}
#endif
