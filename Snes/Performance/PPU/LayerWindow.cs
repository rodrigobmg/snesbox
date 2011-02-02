#if PERFORMANCE
using System;
using System.Linq;
using Nall;

namespace Snes
{
    partial class PPU
    {
        private class LayerWindow
        {
            public bool one_enable;
            public bool one_invert;
            public bool two_enable;
            public bool two_invert;

            public uint mask;

            public bool main_enable;
            public bool sub_enable;

            public byte[] main = new byte[256];
            public byte[] sub = new byte[256];

            public void render(bool screen)
            {
                byte[] output;
                if (screen == Convert.ToBoolean(0))
                {
                    output = main;
                    if (main_enable == false)
                    {
                        Array.Copy(Enumerable.Repeat((byte)0, 256).ToArray(), output, 256);
                        return;
                    }
                }
                else
                {
                    output = sub;
                    if (sub_enable == false)
                    {
                        Array.Copy(Enumerable.Repeat((byte)0, 256).ToArray(), output, 256);
                        return;
                    }
                }

                if (one_enable == false && two_enable == false)
                {
                    Array.Copy(Enumerable.Repeat((byte)0, 256).ToArray(), output, 256);
                    return;
                }

                if (one_enable == true && two_enable == false)
                {
                    bool set = Convert.ToBoolean(1) ^ one_invert, clr = !set;
                    for (uint x = 0; x < 256; x++)
                    {
                        output[x] = Convert.ToByte((x >= ppu.regs.window_one_left && x <= ppu.regs.window_one_right) ? set : clr);
                    }
                    return;
                }

                if (one_enable == false && two_enable == true)
                {
                    bool set = Convert.ToBoolean(1) ^ two_invert, clr = !set;
                    for (uint x = 0; x < 256; x++)
                    {
                        output[x] = Convert.ToByte((x >= ppu.regs.window_two_left && x <= ppu.regs.window_two_right) ? set : clr);
                    }
                    return;
                }

                for (uint x = 0; x < 256; x++)
                {
                    bool one_mask = (x >= ppu.regs.window_one_left && x <= ppu.regs.window_one_right) ^ one_invert;
                    bool two_mask = (x >= ppu.regs.window_two_left && x <= ppu.regs.window_two_right) ^ two_invert;
                    switch (mask)
                    {
                        case 0:
                            output[x] = Convert.ToByte(one_mask | two_mask == Convert.ToBoolean(1));
                            break;
                        case 1:
                            output[x] = Convert.ToByte(one_mask & two_mask == Convert.ToBoolean(1));
                            break;
                        case 2:
                            output[x] = Convert.ToByte(one_mask ^ two_mask == Convert.ToBoolean(1));
                            break;
                        case 3:
                            output[x] = Convert.ToByte(one_mask ^ two_mask == Convert.ToBoolean(0));
                            break;
                    }
                }
            }

            public void serialize(Serializer s)
            {
                s.integer(one_enable, "one_enable");
                s.integer(one_invert, "one_invert");
                s.integer(two_enable, "two_enable");
                s.integer(two_invert, "two_invert");

                s.integer(mask, "mask");

                s.integer(main_enable, "main_enable");
                s.integer(sub_enable, "sub_enable");

                s.array(main, "main");
                s.array(sub, "sub");
            }
        }
    }
}
#endif