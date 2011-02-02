#if PERFORMANCE
using System;
using System.Linq;
using Nall;

namespace Snes
{
    partial class PPU
    {
        private class ColorWindow
        {
            public bool one_enable;
            public bool one_invert;
            public bool two_enable;
            public bool two_invert;

            public uint mask;

            public uint main_mask;
            public uint sub_mask;

            public byte[] main = new byte[256];
            public byte[] sub = new byte[256];

            public void render(bool screen)
            {
                byte[] output = (screen == Convert.ToBoolean(0) ? main : sub);
                bool set = Convert.ToBoolean(1), clr = Convert.ToBoolean(0);

                switch (screen == Convert.ToBoolean(0) ? main_mask : sub_mask)
                {
                    case 0:
                        Array.Copy(Enumerable.Repeat((byte)1, 256).ToArray(), output, 256);
                        return; //always
                    case 1:
                        set = Convert.ToBoolean(1); 
                        clr = Convert.ToBoolean(0);
                        break; //inside window only
                    case 2:
                        set = Convert.ToBoolean(0);
                        clr = Convert.ToBoolean(1);
                        break; //outside window only
                    case 3:
                        Array.Copy(Enumerable.Repeat((byte)0, 256).ToArray(), output, 256);
                        return; //never
                }

                if (one_enable == false && two_enable == false)
                {
                    Array.Copy(Enumerable.Repeat(Convert.ToByte(clr), 256).ToArray(), output, 256);
                    return;
                }

                if (one_enable == true && two_enable == false)
                {
                    if (one_invert)
                    {
                        set ^= Convert.ToBoolean(1);
                        clr ^= Convert.ToBoolean(1);
                    }
                    for (uint x = 0; x < 256; x++)
                    {
                        output[x] = Convert.ToByte((x >= ppu.regs.window_one_left && x <= ppu.regs.window_one_right) ? set : clr);
                    }
                    return;
                }

                if (one_enable == false && two_enable == true)
                {
                    if (two_invert)
                    {
                        set ^= Convert.ToBoolean(1);
                        clr ^= Convert.ToBoolean(1);
                    }
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
                            output[x] = Convert.ToByte(one_mask | two_mask == Convert.ToBoolean(1) ? set : clr);
                            break;
                        case 1:
                            output[x] = Convert.ToByte(one_mask & two_mask == Convert.ToBoolean(1) ? set : clr);
                            break;
                        case 2:
                            output[x] = Convert.ToByte(one_mask ^ two_mask == Convert.ToBoolean(1) ? set : clr);
                            break;
                        case 3:
                            output[x] = Convert.ToByte(one_mask ^ two_mask == Convert.ToBoolean(0) ? set : clr);
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

                s.integer(main_mask, "main_mask");
                s.integer(sub_mask, "sub_mask");

                s.array(main, "main");
                s.array(sub, "sub");
            }
        }
    }
}
#endif