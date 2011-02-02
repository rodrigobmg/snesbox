#if ACCURACY
namespace Snes
{
    partial class PPU
    {
        private partial class Sprite
        {
            public class State
            {
                public uint x;
                public uint y;

                public uint item_count;
                public uint tile_count;

                public bool active;
                public byte[][] item = new byte[2][];
                public TileItem[][] tile = new TileItem[2][];

                public State()
                {
                    for (int i = 0; i < item.Length; i++)
                    {
                        item[i] = new byte[32];
                    }

                    for (int i = 0; i < tile.Length; i++)
                    {
                        tile[i] = new TileItem[34];
                        for (int j = 0; j < tile[i].Length; j++)
                        {
                            tile[i][j] = new TileItem();
                        }
                    }
                }
            }
        }
    }
}
#endif