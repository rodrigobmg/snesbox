using System;

namespace Snes
{
    class SDD1_OL
    {
        public SDD1_OL(SDD1_CM associatedCM) { throw new NotImplementedException(); }
        public void prepareDecomp(uint first_byte, ushort out_len, byte[] out_buf) { throw new NotImplementedException(); }
        public void launch() { throw new NotImplementedException(); }

        private byte bitplanesInfo;
        private ushort length;
        private byte[] buffer;
        private SDD1_CM CM;
    }
}
