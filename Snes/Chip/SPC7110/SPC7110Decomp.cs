using System;
using Nall;

namespace Snes
{
    partial class SPC7110Decomp
    {
        public byte read() { throw new NotImplementedException(); }
        public void init(uint mode, uint offset, uint index) { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public void serialize(Serializer s)
        {
            throw new NotImplementedException();
        }

        public SPC7110Decomp() { throw new NotImplementedException(); }

        private uint decomp_mode;
        private uint decomp_offset;

        //read() will spool chunks half the size of decomp_buffer_size
        private const int decomp_buffer_size = 64; //must be >= 64, and must be a power of two
        private byte[] decomp_buffer;
        private uint decomp_buffer_rdoffset;
        private uint decomp_buffer_wroffset;
        private uint decomp_buffer_length;

        private void write(byte data) { throw new NotImplementedException(); }
        private byte dataread() { throw new NotImplementedException(); }

        private void mode0(bool init) { throw new NotImplementedException(); }
        private void mode1(bool init) { throw new NotImplementedException(); }
        private void mode2(bool init) { throw new NotImplementedException(); }

        private static readonly byte[,] evolution_table = new byte[53, 4];
        private static readonly byte[,] mode2_context_table = new byte[32, 2];

        private ContextState[] context = new ContextState[32];

        private byte probability(uint n) { throw new NotImplementedException(); }
        private byte next_lps(uint n) { throw new NotImplementedException(); }
        private byte next_mps(uint n) { throw new NotImplementedException(); }
        private bool toggle_invert(uint n) { throw new NotImplementedException(); }

        private uint[,] morton16 = new uint[2, 256];
        private uint[,] morton32 = new uint[4, 256];
        private uint morton_2x8(uint data) { throw new NotImplementedException(); }
        private uint morton_4x8(uint data) { throw new NotImplementedException(); }
    }
}
