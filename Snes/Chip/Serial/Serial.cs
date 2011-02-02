using System;
using System.Collections;
using Nall;

namespace Snes
{
    class Serial : ICoprocessor, IMMIO
    {
        public static Serial serial = new Serial();

        public static void Enter() { throw new NotImplementedException(); }
        public void enter() { throw new NotImplementedException(); }
        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public void serialize(Serializer s)
        {
            throw new NotImplementedException();
        }

        public byte latch { get; private set; }

        public void add_clocks(uint clocks) { throw new NotImplementedException(); }
        public byte read() { throw new NotImplementedException(); }
        public void write(byte data) { throw new NotImplementedException(); }

        public IEnumerable mmio_read(uint addr, Result result) { throw new NotImplementedException(); }
        public IEnumerable mmio_write(uint addr, byte data) { throw new NotImplementedException(); }

        private IMMIO r4016, r4017;

        public Coprocessor Coprocessor
        {
            get { throw new NotImplementedException(); }
        }
    }
}
