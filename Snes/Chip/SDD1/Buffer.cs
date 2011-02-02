
namespace Snes
{
    partial class SDD1
    {
        private class Buffer
        {
            byte[] data = new byte[65536];   //pointer to decompressed S-DD1 data
            ushort offset;       //read index into S-DD1 decompression buffer
            uint size;       //length of data buffer; reads decrement counter, set ready to false at 0
            bool ready;          //true when data[] is valid; false to invoke sdd1emu.decompress()
        }
    }
}
