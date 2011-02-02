
namespace Snes
{
    partial class PPUCounter
    {
        private class History
        {
            public bool[] field = new bool[2048];
            public ushort[] vcounter = new ushort[2048];
            public ushort[] hcounter = new ushort[2048];

            public int index;
        }
    }
}
