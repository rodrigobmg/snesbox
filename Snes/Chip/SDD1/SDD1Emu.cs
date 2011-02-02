using System;

namespace Snes
{
    class SDD1Emu
    {
        public SDD1Emu() { throw new NotImplementedException(); }
        public void decompress(uint in_buf, ushort out_len, byte[] out_buf) { throw new NotImplementedException(); }

        private SDD1_IM IM;
        private SDD1_GCD GCD;
        private SDD1_BG BG0;
        private SDD1_BG BG1;
        private SDD1_BG BG2;
        private SDD1_BG BG3;
        private SDD1_BG BG4;
        private SDD1_BG BG5;
        private SDD1_BG BG6;
        private SDD1_BG BG7;
        private SDD1_PEM PEM;
        private SDD1_CM CM;
        private SDD1_OL OL;
    }
}
