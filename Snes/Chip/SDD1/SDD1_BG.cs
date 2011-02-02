using System;

namespace Snes
{
    class SDD1_BG
    {
        public SDD1_BG(SDD1_GCD associatedGCD, byte code) { throw new NotImplementedException(); }
        public void prepareDecomp() { throw new NotImplementedException(); }
        public byte getBit(byte[] endOfRun) { throw new NotImplementedException(); }

        private readonly byte code_num;
        private byte MPScount;
        private byte LPSind;
        private SDD1_GCD GCD;
    }
}
