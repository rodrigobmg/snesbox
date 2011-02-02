
namespace Snes
{
    abstract class Memory
    {
        public virtual uint size()
        {
            return 0;
        }

        public abstract byte read(uint addr);
        public abstract void write(uint addr, byte data);
    }
}
