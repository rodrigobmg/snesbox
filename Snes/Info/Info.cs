
namespace Snes
{
    static class Info
    {
        public const string Name = "bsnes";
        public const string Version = "072";
        public const uint SerializerVersion = 14;
        public const string Profile =
#if PERFORMANCE
 "Performance";
#elif COMPATIBILITY
 "Compatibility";
#else
 "Accuracy";
#endif
    }
}
