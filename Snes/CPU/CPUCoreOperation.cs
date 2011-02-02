using System.Collections;

namespace Snes
{
    public delegate IEnumerable CPUCoreOp(CPUCoreOpArgument args);

    public class CPUCoreOperation
    {
        private CPUCoreOp op { get; set; }
        private CPUCoreOpArgument args { get; set; }

        public CPUCoreOperation(CPUCoreOp op, CPUCoreOpArgument args)
        {
            this.op = op;
            this.args = args;
        }

        public IEnumerable Invoke()
        {
            foreach (var e in op(args))
            {
                yield return e;
            };
        }
    }
}
