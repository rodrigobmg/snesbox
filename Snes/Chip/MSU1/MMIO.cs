
namespace Snes
{
    partial class MSU1
    {
        private class MMIO
        {
            uint data_offset;
            uint audio_offset;
            ushort audio_track;
            byte audio_volume;
            bool data_busy;
            bool audio_busy;
            bool audio_repeat;
            bool audio_play;
        }
    }
}
