
namespace Snes
{
    public delegate void CPUCoreOp(CPUCoreOpArgument args);

    public class CPUCoreOperation
    {
        private CPUCoreOp op { get; set; }
        private CPUCoreOpArgument args { get; set; }

        public CPUCoreOperation(CPUCoreOp op, CPUCoreOpArgument args)
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
