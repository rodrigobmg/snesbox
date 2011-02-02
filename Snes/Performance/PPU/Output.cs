#if PERFORMANCE
using Nall;

namespace Snes
{
    partial class PPU
    {
        private partial class Sprite
        {
            public class Output
            {
                public byte[] palette = new byte[256];
                public byte[] priority = new byte[256];
            }
        }
    }

    partial class PPU
    {
        private partial class Screen
        {
            public class Output
            {
                public class Pixel
                {
                    public uint color;
                    public uint priority;
                    public uint source;
                }
                public Pixel[] main = new Pixel[256];
                public Pixel[] sub = new Pixel[256];

                public Output()
                {
                    Utility.InstantiateArrayElements(main);
                    Utility.InstantiateArrayElements(sub);
                }

                public void plot_main(uint x, uint color, uint priority, uint source)
                {
                    if (priority > main[x].priority)
                    {
                        main[x].color = color;
                        main[x].priority = priority;
                        main[x].source = source;
                    }
                }

                public void plot_sub(uint x, uint color, uint priority, uint source)
                {
                    if (priority > sub[x].priority)
                    {
                        sub[x].color = color;
                        sub[x].priority = priority;
                        sub[x].source = source;
                    }
                }
            }
        }
    }
}
#endif