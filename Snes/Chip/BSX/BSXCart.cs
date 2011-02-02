using System;
using System.Collections;

namespace Snes
{
    partial class BSXCart : IMMIO
    {
        public static BSXCart bsxcart = new BSXCart();

        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public IEnumerable mmio_read(uint addr, Result result) { throw new NotImplementedException(); }
        public IEnumerable mmio_write(uint addr, byte data) { throw new NotImplementedException(); }

        public BSXCart() { /*throw new NotImplementedException();*/ }

        private Regs regs;

        private void update_memory_map() { throw new NotImplementedException(); }
    }
}
