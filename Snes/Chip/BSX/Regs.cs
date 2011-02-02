
namespace Snes
{
    partial class BSXBase
    {
        private class Regs
        {
            private byte r2188, r2189, r218a, r218b;
            private byte r218c, r218d, r218e, r218f;
            private byte r2190, r2191, r2192, r2193;
            private byte r2194, r2195, r2196, r2197;
            private byte r2198, r2199, r219a, r219b;
            private byte r219c, r219d, r219e, r219f;

            private byte r2192_counter;
            private byte r2192_hour, r2192_minute, r2192_second;
        }
    }

    partial class BSXCart
    {
        private class Regs
        {
            private byte[] r = new byte[16];
        }
    }

    partial class BSXFlash
    {
        private class Regs
        {
            private uint command;
            private byte write_old;
            private byte write_new;

            private bool flash_enable;
            private bool read_enable;
            private bool write_enable;
        }
    }
}
