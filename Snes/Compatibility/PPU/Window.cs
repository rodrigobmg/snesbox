#if COMPATIBILITY
namespace Snes
{
    partial class PPU
    {
        public class Window
        {
            public byte[] main = new byte[256], sub = new byte[256];
        }
    }
}
#endif
