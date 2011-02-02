using System.Collections;

namespace Snes
{
    abstract class Memory
    {
        public virtual uint size()
        {
            return 0;
        }

        public abstract IEnumerable read(uint addr, Result result);
        public abstract IEnumerable write(uint addr, byte data);
    }
}
