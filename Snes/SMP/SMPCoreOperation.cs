using System.Collections;

namespace Snes
{
    public delegate IEnumerable SMPCoreOp(SMPCoreOpArgument args, SMPCoreOpResult result);

    public class SMPCoreOperation
    {
        private SMPCoreOp op { get; set; }
        private SMPCoreOpArgument args { get; set; }
        private static SMPCoreOpResult result = new SMPCoreOpResult();

        public SMPCoreOperation(SMPCoreOp op, SMPCoreOpArgument args)
        {
            this.op = op;
            this.args = args;
        }

        public IEnumerable Invoke()
        {
            foreach (var e in op(args, result))
            {
                yield return e;
            };
        }
    }
}
