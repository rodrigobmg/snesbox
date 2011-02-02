using System;

namespace Snes
{
    partial class SuperFX
    {
        public class Reg16
        {
            public delegate void Modify(ushort i);
            public ushort data;
            public Modify on_modify;

            public static explicit operator uint(Reg16 reg16) { throw new NotImplementedException(); }
            public ushort Assign(ushort i) { throw new NotImplementedException(); }

            public static Reg16 operator ++(Reg16 reg16) { throw new NotImplementedException(); }
            public static Reg16 operator --(Reg16 reg16) { throw new NotImplementedException(); }
            public static uint operator |(Reg16 reg16, uint i) { throw new NotImplementedException(); }
            public static uint operator ^(Reg16 reg16, uint i) { throw new NotImplementedException(); }
            public static uint operator &(Reg16 reg16, uint i) { throw new NotImplementedException(); }
            public static uint operator <<(Reg16 reg16, int i) { throw new NotImplementedException(); }
            public static uint operator >>(Reg16 reg16, int i) { throw new NotImplementedException(); }
            public static uint operator +(Reg16 reg16, uint i) { throw new NotImplementedException(); }
            public static uint operator -(Reg16 reg16, uint i) { throw new NotImplementedException(); }
            public static uint operator *(Reg16 reg16, uint i) { throw new NotImplementedException(); }
            public static uint operator /(Reg16 reg16, uint i) { throw new NotImplementedException(); }
            public static uint operator %(Reg16 reg16, uint i) { throw new NotImplementedException(); }

            public Reg16() { throw new NotImplementedException(); }
        }
    }
}
