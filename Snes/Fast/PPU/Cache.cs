#if FAST_PPU
namespace Snes
{
    partial class PPU
    {
        public class Cache
        {
            //$2101
            public byte oam_basesize;
            public byte oam_nameselect;
            public ushort oam_tdaddr;

            //$210d-$210e
            public ushort m7_hofs, m7_vofs;

            //$211b-$2120
            public ushort m7a, m7b, m7c, m7d, m7x, m7y;
        }
    }
}
#endif
