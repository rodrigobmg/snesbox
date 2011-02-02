#if FAST_PPU
namespace Snes
{
    partial class PPU
    {
        public class BackgroundInfo
        {
            public ushort tw, th;  //tile width, height
            public ushort mx, my;  //screen mask x, y
            public ushort scx, scy; //sc index offsets
        }
    }
}
#endif
