using System.Threading;
using Nall;

namespace Snes
{
    class Processor
    {
        public Thread thread;
        public uint frequency;
        public long clock;

        public void create(string name, ThreadStart entryPoint, uint frequency_)
        {
            if (!ReferenceEquals(thread, null))
                Libco.Delete(thread);

            thread = Libco.Create(name, 65536 * sizeof(int), entryPoint);
            frequency = frequency_;
            clock = 0;
        }

        public void serialize(Serializer s)
        {
            s.integer(frequency, "frequency");
            s.integer(clock, "clock");
        }

        public Processor()
        {
            thread = null;
        }
    }
}
