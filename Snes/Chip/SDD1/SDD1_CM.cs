using System;

namespace Snes
{
    class SDD1_CM
    {
        public SDD1_CM(SDD1_PEM associatedPEM) { throw new NotImplementedException(); }
        public void prepareDecomp(uint first_byte) { throw new NotImplementedException(); }
        public byte getBit() { throw new NotImplementedException(); }

        private byte bitplanesInfo;
        private byte contextBitsInfo;
        private byte bit_number;
        private byte currBitplane;
        private ushort[] prevBitplaneBits = new ushort[8];
        private SDD1_PEM PEM;
    }
}
