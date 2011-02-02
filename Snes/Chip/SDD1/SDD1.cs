using System;
using System.Collections;
using Nall;

namespace Snes
{
    partial class SDD1 : Memory, IMMIO
    {
        public static SDD1 sdd1 = new SDD1();

        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public IEnumerable mmio_read(uint addr, Result result) { throw new NotImplementedException(); }
        public IEnumerable mmio_write(uint addr, byte data) { throw new NotImplementedException(); }

        public override IEnumerable read(uint addr, Result result) { throw new NotImplementedException(); }
        public override IEnumerable write(uint addr, byte data) { throw new NotImplementedException(); }

        public void serialize(Serializer s)
        {
            throw new NotImplementedException();
        }

        public SDD1() { /*throw new NotImplementedException();*/ }

        private IMMIO[] cpu_mmio = new IMMIO[0x80];  //bus spying hooks to glean information for class dma[]

        private byte sdd1_enable;     //channel bit-mask
        private byte xfer_enable;     //channel bit-mask
        private uint[] mmc = new uint[4];       //memory map controller ROM indices

        private DMA[] dma = new DMA[8];
        private SDD1Emu sdd1emu;
        private Buffer buffer;
    }
}
