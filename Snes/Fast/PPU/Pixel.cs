#if FAST_PPU
namespace Snes
{
    partial class PPU
    {
        public class Pixel
        {
            //bgr555 color data for main/subscreen pixels: 0x0000 = transparent / use palette color # 0
            //needs to be bgr555 instead of palette index for direct color mode ($2130 bit 0) to work
            public ushort src_main, src_sub;
            //indicates source of palette # for main/subscreen (BG1-4, OAM, or back)
            public byte bg_main, bg_sub;
            //color_exemption -- true when bg == OAM && palette index >= 192, disables color add/sub effects
            public byte ce_main, ce_sub;
            //priority level of src_n. to set src_n,
            //the priority of the pixel must be >pri_n
            public byte pri_main, pri_sub;
        }
    }
}
#endif
