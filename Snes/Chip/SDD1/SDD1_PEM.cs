using System;

namespace Snes
{
    partial class SDD1_PEM
    {
        public SDD1_PEM(SDD1_BG associatedBG0, SDD1_BG associatedBG1, SDD1_BG associatedBG2, SDD1_BG associatedBG3, SDD1_BG associatedBG4, SDD1_BG associatedBG5, SDD1_BG associatedBG6, SDD1_BG associatedBG7) { throw new NotImplementedException(); }
        public void prepareDecomp() { throw new NotImplementedException(); }
        public byte getBit(byte context) { throw new NotImplementedException(); }

        private static readonly State[] evolution_table;
        private SDD1_ContextInfo[] contextInfo = new SDD1_ContextInfo[32];
        private SDD1_BG[] BG = new SDD1_BG[8];
    }
}
