
namespace Nall
{
    public struct uint2
    {
        private uint data;
        private const int bits = 2;

        public static explicit operator uint(uint2 number)
        {
            return number.data;
        }

        public static uint2 operator ++(uint2 number)
        {
            return number + 1;
        }

        public static uint2 operator --(uint2 number)
        {
            return number - 1;
        }

        public uint Assign(uint i)
        {
            return data = Bit.uclip(bits, i);
        }

        public static uint2 operator |(uint2 number, uint i)
        {
            return new uint2(Bit.uclip(bits, number.data | i));
        }

        public static uint2 operator ^(uint2 number, uint i)
        {
            return new uint2(Bit.uclip(bits, number.data ^ i));
        }

        public static uint2 operator &(uint2 number, uint i)
        {
            return new uint2(Bit.uclip(bits, number.data & i));
        }

        public static uint2 operator <<(uint2 number, int i)
        {
            return new uint2(Bit.uclip(bits, number.data << i));
        }

        public static uint2 operator >>(uint2 number, int i)
        {
            return new uint2(Bit.uclip(bits, number.data >> i));
        }

        public static uint2 operator +(uint2 number, uint i)
        {
            return new uint2(Bit.uclip(bits, number.data + i));
        }

        public static uint2 operator -(uint2 number, uint i)
        {
            return new uint2(Bit.uclip(bits, number.data - i));
        }

        public static uint2 operator *(uint2 number, uint i)
        {
            return new uint2(Bit.uclip(bits, number.data * i));
        }

        public static uint2 operator /(uint2 number, uint i)
        {
            return new uint2(Bit.uclip(bits, number.data / i));
        }

        public static uint2 operator %(uint2 number, uint i)
        {
            return new uint2(Bit.uclip(bits, number.data % i));
        }

        public uint2(uint i)
        {
            data = Bit.uclip(bits, i);
        }
    }
}
