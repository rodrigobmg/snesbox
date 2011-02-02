#if COMPATIBILITY
namespace Snes
{
    partial class PPU
    {
        public class Regs
        {
            //open bus support
            public byte ppu1_mdr, ppu2_mdr;

            //bg line counters
            public ushort[] bg_y = new ushort[4];

            //internal state
            public ushort oam_iaddr;
            public ushort cgram_iaddr;

            //$2100
            public bool display_disable;
            public byte display_brightness;

            //$2101
            public byte oam_basesize;
            public byte oam_nameselect;
            public ushort oam_tdaddr;

            //$2102-$2103
            public ushort oam_baseaddr;
            public ushort oam_addr;
            public bool oam_priority;
            public byte oam_firstsprite;

            //$2104
            public byte oam_latchdata;

            //$2105
            public bool[] bg_tilesize = new bool[4];
            public bool bg3_priority;
            public byte bg_mode;

            //$2106
            public byte mosaic_size;
            public bool[] mosaic_enabled = new bool[4];
            public ushort mosaic_countdown;

            //$2107-$210a
            public ushort[] bg_scaddr = new ushort[4];
            public byte[] bg_scsize = new byte[4];

            //$210b-$210c
            public ushort[] bg_tdaddr = new ushort[4];

            //$210d-$2114
            public byte bg_ofslatch;
            public ushort m7_hofs, m7_vofs;
            public ushort[] bg_hofs = new ushort[4];
            public ushort[] bg_vofs = new ushort[4];

            //$2115
            public bool vram_incmode;
            public byte vram_mapping;
            public byte vram_incsize;

            //$2116-$2117
            public ushort vram_addr;

            //$211a
            public byte mode7_repeat;
            public bool mode7_vflip;
            public bool mode7_hflip;

            //$211b-$2120
            public byte m7_latch;
            public ushort m7a, m7b, m7c, m7d, m7x, m7y;

            //$2121
            public ushort cgram_addr;

            //$2122
            public byte cgram_latchdata;

            //$2123-$2125
            public bool[] window1_enabled = new bool[6];
            public bool[] window1_invert = new bool[6];
            public bool[] window2_enabled = new bool[6];
            public bool[] window2_invert = new bool[6];

            //$2126-$2129
            public byte window1_left, window1_right;
            public byte window2_left, window2_right;

            //$212a-$212b
            public byte[] window_mask = new byte[6];

            //$212c-$212d
            public bool[] bg_enabled = new bool[5], bgsub_enabled = new bool[5];

            //$212e-$212f
            public bool[] window_enabled = new bool[5], sub_window_enabled = new bool[5];

            //$2130
            public byte color_mask, colorsub_mask;
            public bool addsub_mode;
            public bool direct_color;

            //$2131
            public bool color_mode, color_halve;
            public bool[] color_enabled = new bool[6];

            //$2132
            public byte color_r, color_g, color_b;
            public ushort color_rgb;

            //$2133
            //overscan and interlace are checked once per frame to
            //determine if entire frame should be interlaced/non-interlace
            //and overscan adjusted. therefore, the variables act sort of
            //like a buffer, but they do still affect internal rendering
            public bool mode7_extbg;
            public bool pseudo_hires;
            public bool overscan;
            public ushort scanlines;
            public bool oam_interlace;
            public bool interlace;

            //$2137
            public ushort hcounter, vcounter;
            public bool latch_hcounter, latch_vcounter;
            public bool counters_latched;

            //$2139-$213a
            public ushort vram_readbuffer;

            //$213e
            public bool time_over, range_over;
            public ushort oam_itemcount, oam_tilecount;
        }
    }
}
#endif
