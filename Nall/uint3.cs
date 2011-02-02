
namespace Nall
{
    public struct uint3
    {
        private uint data;
        private const int bits = 3;

        public static explicit operator uint(uint3 number)
        {
            return number.data;
        }

        public static uint3 operator ++(uint3 number)
        {
            return new uint3(number + 1);
        }

        public static uint3 operator --(uint3 number)
        {
            return new uint3(number - 1);
        }

        public uint Assign(uint i)
        {
            return data = Bit.uclip(bits, i);
        }

        public static uint operator |(uint3 number, uint i)
        {
            return Bit.uclip(bits, number.data | i);
        }

        public static uint operator ^(uint3 number, uint i)
        {
            return Bit.uclip(bits, number.data ^ i);
        }

        public static uint operator &(uint3 number, uint i)
        {
            return Bit.uclip(bits, number.data & i);
        }

        public static uint operator <<(uint3 number, int i)
        {
            return Bit.uclip(bits, number.data << i);
        }

        public static uint operator >>(uint3 number, int i)
        {
            return Bit.uclip(bits, number.data >> i);
        }

        public static uint operator +(uint3 number, uint i)
        {
            return Bit.uclip(bits, number.data + i);
        }

        public static uint operator -(uint3 number, uint i)
        {
            return Bit.uclip(bits, number.data - i);
        }

        public static uint operator *(uint3 number, uint i)
        {
            return Bit.uclip(bits, number.data * i);
        }

        public static uint operator /(uint3 number, uint i)
        {
            return Bit.uclip(bits, number.data / i);
        }

        public static uint operator %(uint3 number, uint i)
        {
            return Bit.uclip(bits, number.data % i);
        }

        public uint3(uint i)
        {
            data = Bit.uclip(bits, i);
        }
    }
}
