
namespace Nall
{
    public struct uint10
    {
        private uint data;
        private const int bits = 10;

        public static explicit operator uint(uint10 number)
        {
            return number.data;
        }

        public static uint10 operator ++(uint10 number)
        {
            return number + 1;
        }

        public static uint10 operator --(uint10 number)
        {
            return number - 1;
        }

        public uint Assign(uint i)
        {
            return data = Bit.uclip(bits, i);
        }

        public static uint10 operator |(uint10 number, uint i)
        {
            return new uint10(Bit.uclip(bits, number.data | i));
        }

        public static uint10 operator ^(uint10 number, uint i)
        {
            return new uint10(Bit.uclip(bits, number.data ^ i));
        }

        public static uint10 operator &(uint10 number, uint i)
        {
            return new uint10(Bit.uclip(bits, number.data & i));
        }

        public static uint10 operator <<(uint10 number, int i)
        {
            return new uint10(Bit.uclip(bits, number.data << i));
        }

        public static uint10 operator >>(uint10 number, int i)
        {
            return new uint10(Bit.uclip(bits, number.data >> i));
        }

        public static uint10 operator +(uint10 number, uint i)
        {
            return new uint10(Bit.uclip(bits, number.data + i));
        }

        public static uint10 operator -(uint10 number, uint i)
        {
            return new uint10(Bit.uclip(bits, number.data - i));
        }

        public static uint10 operator *(uint10 number, uint i)
        {
            return new uint10(Bit.uclip(bits, number.data * i));
        }

        public static uint10 operator /(uint10 number, uint i)
        {
            return new uint10(Bit.uclip(bits, number.data / i));
        }

        public static uint10 operator %(uint10 number, uint i)
        {
            return new uint10(Bit.uclip(bits, number.data % i));
        }

        public uint10(uint i)
        {
            data = Bit.uclip(bits, i);
        }
    }
}
