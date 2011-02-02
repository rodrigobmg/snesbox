
namespace Snes
{
    static class Info
    {
        public const string Name = "bsnes";
        public const string Version = "068";
        public const uint SerializerVersion = 12;
        public const string Profile =
#if (FAST_CPU && FAST_DSP && FAST_PPU)
 "Performance";
#elif (FAST_DSP && FAST_PPU)
 "Compatibility";
#else
 "Accuracy";
#endif
    }
}
