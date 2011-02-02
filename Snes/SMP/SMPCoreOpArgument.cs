
namespace Snes
{
    public class SMPCoreOpArgument
    {
        public byte x_byte { get; set; }
        public byte y_byte { get; set; }
        public ushort x_ushort { get; set; }
        public ushort y_ushort { get; set; }
        public int to { get; set; }
        public int from { get; set; }
        public int n { get; set; }
        public int i { get; set; }
        public int flag { get; set; }
        public int value { get; set; }
        public int mask { get; set; }
        public SMPCoreOp op_func { get; set; }
        public int op { get; set; }
        public int adjust { get; set; }
    }
}
