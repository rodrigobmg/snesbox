using System;

namespace Snes
{
    partial class SuperFX
    {
        public class Regs
        {
            public byte pipeline;
            public ushort ramaddr;

            public Reg16[] r = new Reg16[16];    //general purpose registers
            public Sfr sfr;        //status flag register
            public byte pbr;        //program bank register
            public byte rombr;      //game pack ROM bank register
            public bool rambr;       //game pack RAM bank register
            public ushort cbr;       //cache base register
            public byte scbr;       //screen base register
            public Scmr scmr;      //screen mode register
            public byte colr;       //color register
            public Por por;        //plot option register
            public bool bramr;       //back-up RAM register
            public byte vcr;        //version code register
            public Cfgr cfgr;      //config register
            public bool clsr;        //clock select register

            public uint romcl;   //clock ticks until romdr is valid
            public byte romdr;      //ROM buffer data register

            public uint ramcl;   //clock ticks until ramdr is valid
            public ushort ramar;     //RAM buffer address register
            public byte ramdr;      //RAM buffer data register

            public uint sreg, dreg;
            public Reg16 sr() { throw new NotImplementedException(); }  //source register (from)
            public Reg16 dr() { throw new NotImplementedException(); }  //destination register (to)

            public void reset() { throw new NotImplementedException(); }
        }
    }
}
