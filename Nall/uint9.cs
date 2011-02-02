
namespace Nall
{
    public struct uint9
    {
        private uint data;
        private const int bits = 9;

        public static explicit operator uint(uint9 number)
        {
            return number.data;
        }

        public static uint9 operator ++(uint9 number)
        {
            return number + 1;
        }

        public static uint9 operator --(uint9 number)
        {
            return number - 1;
        }

        public uint Assign(uint i)
        {
            return data = Bit.uclip(bits, i);
        }

        public static uint9 operator |(uint9 number, uint i)
        {
            return new uint9(Bit.uclip(bits, number.data | i));
        }

        public static uint9 operator ^(uint9 number, uint i)
        {
            return new uint9(Bit.uclip(bits, number.data ^ i));
        }

        public static uint9 operator &(uint9 number, uint i)
        {
            return new uint9(Bit.uclip(bits, number.data & i));
        }

        public static uint9 operator <<(uint9 number, int i)
        {
            return new uint9(Bit.uclip(bits, number.data << i));
        }

        public static uint9 operator >>(uint9 number, int i)
        {
            return new uint9(Bit.uclip(bits, number.data >> i));
        }

        public static uint9 operator +(uint9 number, uint i)
        {
            return new uint9(Bit.uclip(bits, number.data + i));
        }

        public static uint9 operator -(uint9 number, uint i)
        {
            return new uint9(Bit.uclip(bits, number.data - i));
        }

        public static uint9 operator *(uint9 number, uint i)
        {
            return new uint9(Bit.uclip(bits, number.data * i));
        }

        public static uint9 operator /(uint9 number, uint i)
        {
            return new uint9(Bit.uclip(bits, number.data / i));
        }

        public static uint9 operator %(uint9 number, uint i)
        {
            return new uint9(Bit.uclip(bits, number.data % i));
        }

        public uint9(uint i)
        {
            data = Bit.uclip(bits, i);
        }
    }
}
