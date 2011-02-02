
namespace Snes
{
    partial class Input
    {
        public partial class Port
        {
            public Device device;
            public uint counter0; //read counters
            public uint counter1;

            public Superscope superscope = new Superscope();
            public Justifier justifier = new Justifier();
        }
    }
}
