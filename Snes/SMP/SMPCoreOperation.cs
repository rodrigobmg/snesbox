
namespace Snes
{
    public delegate SMPCoreOpResult SMPCoreOp(SMPCoreOpArgument args);

    public class SMPCoreOperation
    {
        private SMPCoreOp op { get; set; }
        private SMPCoreOpArgument args { get; set; }

        public SMPCoreOperation(SMPCoreOp op, SMPCoreOpArgument args)
        {
            this.op = op;
            this.args = args;
        }

        public void Invoke()
        {
            op(args);
        }
    }
}
