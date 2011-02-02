
namespace Snes
{
    partial class DSP2
    {
        public class Status
        {
            bool waiting_for_command;
            uint command;
            uint in_count, in_index;
            uint out_count, out_index;

            byte[] parameters = new byte[512];
            byte[] output = new byte[512];

            byte op05transparent;
            bool op05haslen;
            int op05len;
            bool op06haslen;
            int op06len;
            ushort op09word1;
            ushort op09word2;
            bool op0dhaslen;
            int op0doutlen;
            int op0dinlen;
        }
    }
}
