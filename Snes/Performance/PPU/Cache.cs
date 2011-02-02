#if PERFORMANCE
using System;
using Nall;

namespace Snes
{
    partial class PPU
    {
        private class Cache
        {
            public byte[][] tiledata = new byte[3][];
            public byte[][] tilevalid = new byte[3][];

            void render_line_2(ref ArraySegment<byte> output, ref uint color, uint d0, uint d1, byte mask)
            {
                color = Bit.ToBit(d0 & mask) << 0;
                color |= Bit.ToBit(d1 & mask) << 1;
                output.Array[output.Offset] = (byte)color;
                output = new ArraySegment<byte>(output.Array, output.Offset + 1, output.Count - 1);
            }

            void render_line_4(ref ArraySegment<byte> output, ref uint color, uint d0, uint d1, uint d2, uint d3, byte mask)
            {
                color = Bit.ToBit(d0 & mask) << 0;
                color |= Bit.ToBit(d1 & mask) << 1;
                color |= Bit.ToBit(d2 & mask) << 2;
                color |= Bit.ToBit(d3 & mask) << 3;
                output.Array[output.Offset] = (byte)color;
                output = new ArraySegment<byte>(output.Array, output.Offset + 1, output.Count - 1);
            }

            void render_line_8(ref ArraySegment<byte> output, ref uint color, uint d0, uint d1, uint d2, uint d3, uint d4, uint d5, uint d6, uint d7, byte mask)
            {
                color = Bit.ToBit(d0 & mask) << 0;
                color |= Bit.ToBit(d1 & mask) << 1;
                color |= Bit.ToBit(d2 & mask) << 2;
                color |= Bit.ToBit(d3 & mask) << 3;
                color |= Bit.ToBit(d4 & mask) << 4;
                color |= Bit.ToBit(d5 & mask) << 5;
                color |= Bit.ToBit(d6 & mask) << 6;
                color |= Bit.ToBit(d7 & mask) << 7;
                output.Array[output.Offset] = (byte)color;
                output = new ArraySegment<byte>(output.Array, output.Offset + 1, output.Count - 1);
            }

            public ArraySegment<byte> tile_2bpp(uint tile)
            {
                if (tilevalid[0][tile] == 0)
                {
                    tilevalid[0][tile] = 1;
                    ArraySegment<byte> output = new ArraySegment<byte>(tiledata[0], (int)(tile << 6), (int)(tiledata[0].Length - (tile << 6)));
                    uint offset = tile << 4;
                    uint y = 8;
                    uint color = default(uint), d0, d1;
                    while (Convert.ToBoolean(y--))
                    {
                        d0 = StaticRAM.vram[offset + 0];
                        d1 = StaticRAM.vram[offset + 1];
                        render_line_2(ref output, ref color, d0, d1, 0x80);
                        render_line_2(ref output, ref color, d0, d1, 0x40);
                        render_line_2(ref output, ref color, d0, d1, 0x20);
                        render_line_2(ref output, ref color, d0, d1, 0x10);
                        render_line_2(ref output, ref color, d0, d1, 0x08);
                        render_line_2(ref output, ref color, d0, d1, 0x04);
                        render_line_2(ref output, ref color, d0, d1, 0x02);
                        render_line_2(ref output, ref color, d0, d1, 0x01);
                        offset += 2;
                    }
                }
                return new ArraySegment<byte>(tiledata[0], (int)(tile << 6), (int)(tiledata[0].Length - (tile << 6)));
            }

            public ArraySegment<byte> tile_4bpp(uint tile)
            {
                if (tilevalid[1][tile] == 0)
                {
                    tilevalid[1][tile] = 1;
                    ArraySegment<byte> output = new ArraySegment<byte>(tiledata[1], (int)(tile << 6), (int)(tiledata[1].Length - (tile << 6)));
                    uint offset = tile << 5;
                    uint y = 8;
                    uint color = default(uint), d0, d1, d2, d3;
                    while (Convert.ToBoolean(y--))
                    {
                        d0 = StaticRAM.vram[offset + 0];
                        d1 = StaticRAM.vram[offset + 1];
                        d2 = StaticRAM.vram[offset + 16];
                        d3 = StaticRAM.vram[offset + 17];
                        render_line_4(ref output, ref color, d0, d1, d2, d3, 0x80);
                        render_line_4(ref output, ref color, d0, d1, d2, d3, 0x40);
                        render_line_4(ref output, ref color, d0, d1, d2, d3, 0x20);
                        render_line_4(ref output, ref color, d0, d1, d2, d3, 0x10);
                        render_line_4(ref output, ref color, d0, d1, d2, d3, 0x08);
                        render_line_4(ref output, ref color, d0, d1, d2, d3, 0x04);
                        render_line_4(ref output, ref color, d0, d1, d2, d3, 0x02);
                        render_line_4(ref output, ref color, d0, d1, d2, d3, 0x01);
                        offset += 2;
                    }
                }
                return new ArraySegment<byte>(tiledata[1], (int)(tile << 6), (int)(tiledata[1].Length - (tile << 6)));
            }

            public ArraySegment<byte> tile_8bpp(uint tile)
            {
                if (tilevalid[2][tile] == 0)
                {
                    tilevalid[2][tile] = 1;
                    ArraySegment<byte> output = new ArraySegment<byte>(tiledata[2], (int)(tile << 6), (int)(tiledata[2].Length - (tile << 6)));
                    uint offset = tile << 6;
                    uint y = 8;
                    uint color = default(uint), d0, d1, d2, d3, d4, d5, d6, d7;
                    while (Convert.ToBoolean(y--))
                    {
                        d0 = StaticRAM.vram[offset + 0];
                        d1 = StaticRAM.vram[offset + 1];
                        d2 = StaticRAM.vram[offset + 16];
                        d3 = StaticRAM.vram[offset + 17];
                        d4 = StaticRAM.vram[offset + 32];
                        d5 = StaticRAM.vram[offset + 33];
                        d6 = StaticRAM.vram[offset + 48];
                        d7 = StaticRAM.vram[offset + 49];
                        render_line_8(ref output, ref color, d0, d1, d2, d3, d4, d5, d6, d7, 0x80);
                        render_line_8(ref output, ref color, d0, d1, d2, d3, d4, d5, d6, d7, 0x40);
                        render_line_8(ref output, ref color, d0, d1, d2, d3, d4, d5, d6, d7, 0x20);
                        render_line_8(ref output, ref color, d0, d1, d2, d3, d4, d5, d6, d7, 0x10);
                        render_line_8(ref output, ref color, d0, d1, d2, d3, d4, d5, d6, d7, 0x08);
                        render_line_8(ref output, ref color, d0, d1, d2, d3, d4, d5, d6, d7, 0x04);
                        render_line_8(ref output, ref color, d0, d1, d2, d3, d4, d5, d6, d7, 0x02);
                        render_line_8(ref output, ref color, d0, d1, d2, d3, d4, d5, d6, d7, 0x01);
                        offset += 2;
                    }
                }
                return new ArraySegment<byte>(tiledata[2], (int)(tile << 6), (int)(tiledata[2].Length - (tile << 6)));
            }

            public ArraySegment<byte> tile(uint bpp, uint tile)
            {
                switch (bpp)
                {
                    case 0:
                        return tile_2bpp(tile);
                    case 1:
                        return tile_4bpp(tile);
                    case 2:
                    default:
                        return tile_8bpp(tile);
                }
            }

            public void serialize(Serializer s)
            {   //rather than save ~512KB worth of cached tiledata, invalidate it all
                tilevalid[0].Initialize();
                tilevalid[1].Initialize();
                tilevalid[2].Initialize();
            }

            public Cache(PPU self)
            {
                tiledata[0] = new byte[262144];
                tiledata[1] = new byte[131072];
                tiledata[2] = new byte[65536];
                tilevalid[0] = new byte[4096];
                tilevalid[1] = new byte[2048];
                tilevalid[2] = new byte[1024];
            }

            public PPU self;
        }
    }
}
#endif