
namespace Nall
{
    public struct uint24
    {
        private uint data;
        private const int bits = 24;

        public static explicit operator uint(uint24 number)
        {
            return number.data;
        }

        public static uint24 operator ++(uint24 number)
        {
            return number + 1;
        }

        public static uint24 operator --(uint24 number)
        {
            return number - 1;
        }

        public uint Assign(uint i)
        {
            return data = Bit.uclip(bits, i);
        }

        public static uint24 operator |(uint24 number, uint i)
        {
            return new uint24(Bit.uclip(bits, number.data | i));
        }

        public static uint24 operator ^(uint24 number, uint i)
        {
            return new uint24(Bit.uclip(bits, number.data ^ i));
        }

        public static uint24 operator &(uint24 number, uint i)
        {
            return new uint24(Bit.uclip(bits, number.data & i));
        }

        public static uint24 operator <<(uint24 number, int i)
        {
            return new uint24(Bit.uclip(bits, number.data << i));
        }

        public static uint24 operator >>(uint24 number, int i)
        {
            return new uint24(Bit.uclip(bits, number.data >> i));
        }

        public static uint24 operator +(uint24 number, uint i)
        {
            return new uint24(Bit.uclip(bits, number.data + i));
        }

        public static uint24 operator -(uint24 number, uint i)
        {
            return new uint24(Bit.uclip(bits, number.data - i));
        }

        public static uint24 operator *(uint24 number, uint i)
        {
            return new uint24(Bit.uclip(bits, number.data * i));
        }

        public static uint24 operator /(uint24 number, uint i)
        {
            return new uint24(Bit.uclip(bits, number.data / i));
        }

        public static uint24 operator %(uint24 number, uint i)
        {
            return new uint24(Bit.uclip(bits, number.data % i));
        }

        public uint24(uint i)
        {
            data = Bit.uclip(bits, i);
        }
    }
}
