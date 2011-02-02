#if ACCURACY
using Nall;
namespace Snes
{
    partial class PPU
    {
        public class Regs
        {
            public byte ppu1_mdr;
            public byte ppu2_mdr;

            public ushort vram_readbuffer;
            public byte oam_latchdata;
            public byte cgram_latchdata;
            public byte bgofs_latchdata;
            public byte mode7_latchdata;
            public bool counters_latched;
            public bool latch_hcounter;
            public bool latch_vcounter;

            public uint10 oam_iaddr;
            public uint9 cgram_iaddr;

            //$2100  INIDISP
            public bool display_disable;
            public uint display_brightness;

            //$2102  OAMADDL
            //$2103  OAMADDH
            public uint10 oam_baseaddr;
            public uint10 oam_addr;
            public bool oam_priority;

            //$2105  BGMODE
            public bool bg3_priority;
            public byte bgmode;

            //$210d  BG1HOFS
            public ushort mode7_hoffset;

            //$210e  BG1VOFS
            public ushort mode7_voffset;

            //$2115  VMAIN
            public bool vram_incmode;
            public byte vram_mapping;
            public byte vram_incsize;

            //$2116  VMADDL
            //$2117  VMADDH
            public ushort vram_addr;

            //$211a  M7SEL
            public byte mode7_repeat;
            public bool mode7_vflip;
            public bool mode7_hflip;

            //$211b  M7A
            public ushort m7a;

            //$211c  M7B
            public ushort m7b;

            //$211d  M7C
            public ushort m7c;

            //$211e  M7D
            public ushort m7d;

            //$211f  M7X
            public ushort m7x;

            //$2120  M7Y
            public ushort m7y;

            //$2121  CGADD
            public uint9 cgram_addr;

            //$2133  SETINI
            public bool mode7_extbg;
            public bool pseudo_hires;
            public bool overscan;
            public bool interlace;

            //$213c  OPHCT
            public ushort hcounter;

            //$213d  OPVCT
            public ushort vcounter;
        }
    }

    partial class PPU
    {
        partial class Background
        {
            public class Regs
            {
                public uint tiledata_addr;
                public uint screen_addr;
                public uint screen_size;
                public uint mosaic;
                public bool tile_size;

                public uint mode;
                public uint priority0;
                public uint priority1;

                public bool main_enable;
                public bool sub_enable;

                public uint hoffset;
                public uint voffset;
            }
        }
    }

    partial class PPU
    {
        partial class Screen
        {
            public class Regs
            {
                public bool addsub_mode;
                public bool direct_color;

                public bool color_mode;
                public bool color_halve;
                public bool bg1_color_enable;
                public bool bg2_color_enable;
                public bool bg3_color_enable;
                public bool bg4_color_enable;
                public bool oam_color_enable;
                public bool back_color_enable;

                public byte color_b;
                public byte color_g;
                public byte color_r;
            }
        }
    }

    partial class PPU
    {
        partial class Sprite
        {
            public class Regs
            {
                public bool main_enable;
                public bool sub_enable;
                public bool interlace;

                public byte base_size;
                public byte nameselect;
                public ushort tiledata_addr;
                public byte first_sprite;

                public uint priority0;
                public uint priority1;
                public uint priority2;
                public uint priority3;

                public bool time_over;
                public bool range_over;
            }
        }
    }

    partial class PPU
    {
        partial class Window
        {
            public class Regs
            {
                public bool bg1_one_enable;
                public bool bg1_one_invert;
                public bool bg1_two_enable;
                public bool bg1_two_invert;

                public bool bg2_one_enable;
                public bool bg2_one_invert;
                public bool bg2_two_enable;
                public bool bg2_two_invert;

                public bool bg3_one_enable;
                public bool bg3_one_invert;
                public bool bg3_two_enable;
                public bool bg3_two_invert;

                public bool bg4_one_enable;
                public bool bg4_one_invert;
                public bool bg4_two_enable;
                public bool bg4_two_invert;

                public bool oam_one_enable;
                public bool oam_one_invert;
                public bool oam_two_enable;
                public bool oam_two_invert;

                public bool col_one_enable;
                public bool col_one_invert;
                public bool col_two_enable;
                public bool col_two_invert;

                public byte one_left;
                public byte one_right;
                public byte two_left;
                public byte two_right;

                public byte bg1_mask;
                public byte bg2_mask;
                public byte bg3_mask;
                public byte bg4_mask;
                public byte oam_mask;
                public byte col_mask;

                public bool bg1_main_enable;
                public bool bg1_sub_enable;
                public bool bg2_main_enable;
                public bool bg2_sub_enable;
                public bool bg3_main_enable;
                public bool bg3_sub_enable;
                public bool bg4_main_enable;
                public bool bg4_sub_enable;
                public bool oam_main_enable;
                public bool oam_sub_enable;

                public byte col_main_mask;
                public byte col_sub_mask;
            }
        }
    }
}
#endif