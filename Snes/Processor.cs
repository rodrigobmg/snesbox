using System.Collections;
using Nall;

namespace Snes
{
    class Result
    {
        public byte Value;
    }

    class Processor
    {
        public IEnumerator thread;
        public uint frequency;
        public long clock;

        public void create(IEnumerable entryPoint, uint frequency_)
        {
            thread = entryPoint.GetEnumerator();
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
