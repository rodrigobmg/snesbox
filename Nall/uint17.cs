
namespace Nall
{
    public struct uint17
    {
        private uint data;
        private const int bits = 17;

        public static explicit operator uint(uint17 number)
        {
            return number.data;
        }

        public static uint17 operator ++(uint17 number)
        {
            return number + 1;
        }

        public static uint17 operator --(uint17 number)
        {
            return number - 1;
        }

        public uint Assign(uint i)
        {
            return data = Bit.uclip(bits, i);
        }

        public static uint17 operator |(uint17 number, uint i)
        {
            return new uint17(Bit.uclip(bits, number.data | i));
        }

        public static uint17 operator ^(uint17 number, uint i)
        {
            return new uint17(Bit.uclip(bits, number.data ^ i));
        }

        public static uint17 operator &(uint17 number, uint i)
        {
            return new uint17(Bit.uclip(bits, number.data & i));
        }

        public static uint17 operator <<(uint17 number, int i)
        {
            return new uint17(Bit.uclip(bits, number.data << i));
        }

        public static uint17 operator >>(uint17 number, int i)
        {
            return new uint17(Bit.uclip(bits, number.data >> i));
        }

        public static uint17 operator +(uint17 number, uint i)
        {
            return new uint17(Bit.uclip(bits, number.data + i));
        }

        public static uint17 operator -(uint17 number, uint i)
        {
            return new uint17(Bit.uclip(bits, number.data - i));
        }

        public static uint17 operator *(uint17 number, uint i)
        {
            return new uint17(Bit.uclip(bits, number.data * i));
        }

        public static uint17 operator /(uint17 number, uint i)
        {
            return new uint17(Bit.uclip(bits, number.data / i));
        }

        public static uint17 operator %(uint17 number, uint i)
        {
            return new uint17(Bit.uclip(bits, number.data % i));
        }

        public uint17(uint i)
        {
            data = Bit.uclip(bits, i);
        }
    }
}
