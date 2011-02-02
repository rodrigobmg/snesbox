
namespace Nall
{
    public class ModuloArray
    {
        public int this[int index]
        {
            get { return buffer[size + index]; }
        }

        public int read(int index)
        {
            return buffer[size + index];
        }

        public void write(uint index, int value)
        {
            buffer[index] = buffer[index + size] = buffer[index + size + size] = value;
        }

        public void serialize(Serializer s)
        {
            s.array(buffer, (uint)(size * 3), "buffer");
        }

        public ModuloArray(int size_)
        {
            size = size_;
            buffer = new int[size * 3];
        }

        private int size;
        private int[] buffer;
    }
}
