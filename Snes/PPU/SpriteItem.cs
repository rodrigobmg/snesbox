#if !FAST_PPU
using System;

namespace Snes
{
    partial class PPU
    {
        private partial class Sprite
        {
            public class SpriteItem
            {
                public ushort x;
                public ushort y;
                public byte character;
                public bool nameselect;
                public bool vflip;
                public bool hflip;
                public byte priority;
                public byte palette;
                public bool size;

                private static readonly uint[] Width1 = { 8, 8, 8, 16, 16, 32, 16, 16 };
                private static readonly uint[] Width2 = { 16, 32, 64, 32, 64, 64, 32, 32 };
                private static readonly uint[] Height1 = { 8, 8, 8, 16, 16, 32, 32, 32 };
                private static readonly uint[] Height2 = { 16, 32, 64, 32, 64, 64, 64, 32 };

                public uint width()
                {
                    if (size == Convert.ToBoolean(0))
                    {
                        return Width1[ppu.oam.regs.base_size];
                    }
                    else
                    {
                        return Width2[ppu.oam.regs.base_size];
                    }
                }

                public uint height()
                {
                    if (size == Convert.ToBoolean(0))
                    {
                        if (ppu.oam.regs.interlace && ppu.oam.regs.base_size >= 6)
                        {
                            return 16;
                        }
                        return Height1[ppu.oam.regs.base_size];
                    }
                    else
                    {
                        return Height2[ppu.oam.regs.base_size];
                    }
                }
            }
        }
    }
}
#endif