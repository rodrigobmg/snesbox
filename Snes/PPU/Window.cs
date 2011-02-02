#if !FAST_PPU
using System;
using Nall;

namespace Snes
{
    partial class PPU
    {
        private partial class Window
        {
            public PPU self;
            public T t = new T();
            public Regs regs = new Regs();
            public Output output = new Output();

            public void scanline()
            {
                t.x = 0;
            }

            public void run()
            {
                bool main, sub;

                test(
                    out main, out sub,
                    regs.bg1_one_enable, regs.bg1_one_invert,
                    regs.bg1_two_enable, regs.bg1_two_invert,
                    regs.bg1_mask, regs.bg1_main_enable, regs.bg1_sub_enable
                    );
                if (main)
                {
                    self.bg1.output.main.priority = 0;
                }
                if (sub)
                {
                    self.bg1.output.sub.priority = 0;
                }

                test(
                    out main, out sub,
                    regs.bg2_one_enable, regs.bg2_one_invert,
                    regs.bg2_two_enable, regs.bg2_two_invert,
                    regs.bg2_mask, regs.bg2_main_enable, regs.bg2_sub_enable
                    );
                if (main)
                {
                    self.bg2.output.main.priority = 0;
                }
                if (sub)
                {
                    self.bg2.output.sub.priority = 0;
                }

                test(
                    out main, out sub,
                    regs.bg3_one_enable, regs.bg3_one_invert,
                    regs.bg3_two_enable, regs.bg3_two_invert,
                    regs.bg3_mask, regs.bg3_main_enable, regs.bg3_sub_enable
                    );
                if (main)
                {
                    self.bg3.output.main.priority = 0;
                }
                if (sub)
                {
                    self.bg3.output.sub.priority = 0;
                }

                test(
                    out main, out sub,
                    regs.bg4_one_enable, regs.bg4_one_invert,
                    regs.bg4_two_enable, regs.bg4_two_invert,
                    regs.bg4_mask, regs.bg4_main_enable, regs.bg4_sub_enable
                    );
                if (main)
                {
                    self.bg4.output.main.priority = 0;
                }
                if (sub)
                {
                    self.bg4.output.sub.priority = 0;
                }

                test(
                    out main, out sub,
                    regs.oam_one_enable, regs.oam_one_invert,
                    regs.oam_two_enable, regs.oam_two_invert,
                    regs.oam_mask, regs.oam_main_enable, regs.oam_sub_enable
                    );
                if (main)
                {
                    self.oam.output.main.priority = 0;
                }
                if (sub)
                {
                    self.oam.output.sub.priority = 0;
                }

                test(
                    out main, out sub,
                    regs.col_one_enable, regs.col_one_invert,
                    regs.col_two_enable, regs.col_two_invert,
                    regs.col_mask, true, true
                    );

                switch (regs.col_main_mask)
                {
                    case 0:
                        main = true;
                        break;
                    case 1:
                        break;
                    case 2:
                        main = !main;
                        break;
                    case 3:
                        main = false;
                        break;
                }

                switch (regs.col_sub_mask)
                {
                    case 0:
                        sub = true;
                        break;
                    case 1:
                        break;
                    case 2:
                        sub = !sub;
                        break;
                    case 3:
                        sub = false;
                        break;
                }

                output.main.color_enable = main;
                output.sub.color_enable = sub;

                t.x++;
            }

            public void reset()
            {
                t.x = 0;
                regs.bg1_one_enable = false;
                regs.bg1_one_invert = false;
                regs.bg1_two_enable = false;
                regs.bg1_two_invert = false;
                regs.bg2_one_enable = false;
                regs.bg2_one_invert = false;
                regs.bg2_two_enable = false;
                regs.bg2_two_invert = false;
                regs.bg3_one_enable = false;
                regs.bg3_one_invert = false;
                regs.bg3_two_enable = false;
                regs.bg3_two_invert = false;
                regs.bg4_one_enable = false;
                regs.bg4_one_invert = false;
                regs.bg4_two_enable = false;
                regs.bg4_two_invert = false;
                regs.oam_one_enable = false;
                regs.oam_one_invert = false;
                regs.oam_two_enable = false;
                regs.oam_two_invert = false;
                regs.col_one_enable = false;
                regs.col_one_invert = false;
                regs.col_two_enable = false;
                regs.col_two_invert = false;
                regs.one_left = 0;
                regs.one_right = 0;
                regs.two_left = 0;
                regs.two_right = 0;
                regs.bg1_mask = 0;
                regs.bg2_mask = 0;
                regs.bg3_mask = 0;
                regs.bg4_mask = 0;
                regs.oam_mask = 0;
                regs.col_mask = 0;
                regs.bg1_main_enable = Convert.ToBoolean(0);
                regs.bg1_sub_enable = Convert.ToBoolean(0);
                regs.bg2_main_enable = Convert.ToBoolean(0);
                regs.bg2_sub_enable = Convert.ToBoolean(0);
                regs.bg3_main_enable = Convert.ToBoolean(0);
                regs.bg3_sub_enable = Convert.ToBoolean(0);
                regs.bg4_main_enable = Convert.ToBoolean(0);
                regs.bg4_sub_enable = Convert.ToBoolean(0);
                regs.oam_main_enable = Convert.ToBoolean(0);
                regs.oam_sub_enable = Convert.ToBoolean(0);
                regs.col_main_mask = 0;
                regs.col_sub_mask = 0;
                output.main.color_enable = Convert.ToBoolean(0);
                output.sub.color_enable = Convert.ToBoolean(0);
            }

            public void serialize(Serializer s)
            {
                s.integer(t.x, "t.x");

                s.integer(regs.bg1_one_enable, "regs.bg1_one_enable");
                s.integer(regs.bg1_one_invert, "regs.bg1_one_invert");
                s.integer(regs.bg1_two_enable, "regs.bg1_two_enable");
                s.integer(regs.bg1_two_invert, "regs.bg1_two_invert");

                s.integer(regs.bg2_one_enable, "regs.bg2_one_enable");
                s.integer(regs.bg2_one_invert, "regs.bg2_one_invert");
                s.integer(regs.bg2_two_enable, "regs.bg2_two_enable");
                s.integer(regs.bg2_two_invert, "regs.bg2_two_invert");

                s.integer(regs.bg3_one_enable, "regs.bg3_one_enable");
                s.integer(regs.bg3_one_invert, "regs.bg3_one_invert");
                s.integer(regs.bg3_two_enable, "regs.bg3_two_enable");
                s.integer(regs.bg3_two_invert, "regs.bg3_two_invert");

                s.integer(regs.bg4_one_enable, "regs.bg4_one_enable");
                s.integer(regs.bg4_one_invert, "regs.bg4_one_invert");
                s.integer(regs.bg4_two_enable, "regs.bg4_two_enable");
                s.integer(regs.bg4_two_invert, "regs.bg4_two_invert");

                s.integer(regs.oam_one_enable, "regs.oam_one_enable");
                s.integer(regs.oam_one_invert, "regs.oam_one_invert");
                s.integer(regs.oam_two_enable, "regs.oam_two_enable");
                s.integer(regs.oam_two_invert, "regs.oam_two_invert");

                s.integer(regs.col_one_enable, "regs.col_one_enable");
                s.integer(regs.col_one_invert, "regs.col_one_invert");
                s.integer(regs.col_two_enable, "regs.col_two_enable");
                s.integer(regs.col_two_invert, "regs.col_two_invert");

                s.integer(regs.one_left, "regs.one_left");
                s.integer(regs.one_right, "regs.one_right");
                s.integer(regs.two_left, "regs.two_left");
                s.integer(regs.two_right, "regs.two_right");

                s.integer(regs.bg1_mask, "regs.bg1_mask");
                s.integer(regs.bg2_mask, "regs.bg2_mask");
                s.integer(regs.bg3_mask, "regs.bg3_mask");
                s.integer(regs.bg4_mask, "regs.bg4_mask");
                s.integer(regs.oam_mask, "regs.oam_mask");
                s.integer(regs.col_mask, "regs.col_mask");

                s.integer(regs.bg1_main_enable, "regs.bg1_main_enable");
                s.integer(regs.bg1_sub_enable, "regs.bg1_sub_enable");
                s.integer(regs.bg2_main_enable, "regs.bg2_main_enable");
                s.integer(regs.bg2_sub_enable, "regs.bg2_sub_enable");
                s.integer(regs.bg3_main_enable, "regs.bg3_main_enable");
                s.integer(regs.bg3_sub_enable, "regs.bg3_sub_enable");
                s.integer(regs.bg4_main_enable, "regs.bg4_main_enable");
                s.integer(regs.bg4_sub_enable, "regs.bg4_sub_enable");
                s.integer(regs.oam_main_enable, "regs.oam_main_enable");
                s.integer(regs.oam_sub_enable, "regs.oam_sub_enable");

                s.integer(regs.col_main_mask, "regs.col_main_mask");
                s.integer(regs.col_sub_mask, "regs.col_sub_mask");

                s.integer(output.main.color_enable, "output.main.color_enable");
                s.integer(output.sub.color_enable, "output.sub.color_enable");
            }

            public Window(PPU self_)
            {
                self = self_;
            }

            private void test(out bool main, out bool sub, bool one_enable, bool one_invert, bool two_enable, bool two_invert, byte mask, bool main_enable, bool sub_enable)
            {
                uint x = t.x;
                bool output = false;

                if (one_enable == false && two_enable == false)
                {
                    output = false;
                }
                else if (one_enable == true && two_enable == false)
                {
                    output = (x >= regs.one_left && x <= regs.one_right) ^ one_invert;
                }
                else if (one_enable == false && two_enable == true)
                {
                    output = (x >= regs.two_left && x <= regs.two_right) ^ two_invert;
                }
                else
                {
                    bool one = (x >= regs.one_left && x <= regs.one_right) ^ one_invert;
                    bool two = (x >= regs.two_left && x <= regs.two_right) ^ two_invert;
                    switch (mask)
                    {
                        case 0:
                            output = (one | two) == Convert.ToBoolean(1);
                            break;
                        case 1:
                            output = (one & two) == Convert.ToBoolean(1);
                            break;
                        case 2:
                            output = (one ^ two) == Convert.ToBoolean(1);
                            break;
                        case 3:
                            output = (one ^ two) == Convert.ToBoolean(0);
                            break;
                    }
                }

                main = main_enable ? output : false;
                sub = sub_enable ? output : false;
            }
        }
    }
}
#endif