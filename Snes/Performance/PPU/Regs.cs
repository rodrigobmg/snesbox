#if PERFORMANCE
namespace Snes
{
    partial class PPU
    {
        private class Regs
        {
            //internal
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

            //$2100
            public bool display_disable;
            public uint display_brightness;

            //$2102-$2103
            public ushort oam_baseaddr;
            public ushort oam_addr;
            public bool oam_priority;

            //$2105
            public bool bg3_priority;
            public uint bgmode;

            //$210d
            public ushort mode7_hoffset;

            //$210e
            public ushort mode7_voffset;

            //$2115
            public bool vram_incmode;
            public uint vram_mapping;
            public uint vram_incsize;

            //$2116-$2117
            public ushort vram_addr;

            //$211a
            public uint mode7_repeat;
            public bool mode7_vflip;
            public bool mode7_hflip;

            //$211b-$2120
            public ushort m7a;
            public ushort m7b;
            public ushort m7c;
            public ushort m7d;
            public ushort m7x;
            public ushort m7y;

            //$2121
            public ushort cgram_addr;

            //$2126-$212a
            public uint window_one_left;
            public uint window_one_right;
            public uint window_two_left;
            public uint window_two_right;

            //$2133
            public bool mode7_extbg;
            public bool pseudo_hires;
            public bool overscan;
            public bool interlace;

            //$213c
            public ushort hcounter;

            //$213d
            public ushort vcounter;
        }
    }

    partial class PPU
    {
        private partial class Background
        {
            public class Regs
            {
                public uint mode;
                public uint priority0;
                public uint priority1;

                public bool tile_size;
                public uint mosaic;

                public uint screen_addr;
                public uint screen_size;
                public uint tiledata_addr;

                public uint hoffset;
                public uint voffset;

                public bool main_enable;
                public bool sub_enable;
            }
        }
    }

    partial class PPU
    {
        private partial class Sprite
        {
            public class Regs
            {
                public uint priority0;
                public uint priority1;
                public uint priority2;
                public uint priority3;

                public uint base_size;
                public uint nameselect;
                public uint tiledata_addr;
                public uint first_sprite;

                public bool main_enable;
                public bool sub_enable;

                public bool interlace;

                public bool time_over;
                public bool range_over;
            }
        }
    }

    partial class PPU
    {
        private partial class Screen
        {
            public class Regs
            {
                public bool addsub_mode;
                public bool direct_color;

                public bool color_mode;
                public bool color_halve;
                public bool[] color_enable = new bool[7];

                public uint color_b;
                public uint color_g;
                public uint color_r;
                public uint color;
            }
        }
    }
}
#endif