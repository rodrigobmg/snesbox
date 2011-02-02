
namespace Snes
{
    partial class SA1
    {
        public class Status
        {
            byte tick_counter;

            bool interrupt_pending;
            ushort interrupt_vector;

            ushort scanlines;
            ushort vcounter;
            ushort hcounter;
        }
    }
}
