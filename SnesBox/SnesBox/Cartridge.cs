using System;
using Snes;

namespace SnesBox
{
    public abstract class Cartridge
    {
        protected static byte[] MakeUtf8Array(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            var utf8 = new byte[System.Text.Encoding.UTF8.GetByteCount(s) + 1];
            System.Text.Encoding.UTF8.GetBytes(s, 0, s.Length, utf8, 0);
            return utf8;
        }

        protected byte[] _romData;
        protected string _romXml;

        public byte[] RomData { get { return _romData; } set { _romData = value; } }
        public string RomXml { get { return _romXml; } set { _romXml = value; } }

        internal abstract void Load(Snes snes);
        internal abstract void Refresh();
    }

    public class NormalCartridge : Cartridge
    {
        protected byte[] _sram;
        protected byte[] _rtc;

        public byte[] Sram { get { return _sram; } set { _sram = value; } }
        public byte[] Rtc { get { return _rtc; } set { _rtc = value; } }

        internal override void Load(Snes snes)
        {
            LibSnes.snes_load_cartridge_normal(MakeUtf8Array(_romXml), _romData, (uint)_romData.Length);
        }

        internal override void Refresh()
        {

        }
    }

    public class BsxSlottedCartridge : NormalCartridge
    {
        protected byte[] _bsxData;
        protected string _bsxXml;

        public byte[] BsxData { get { return _bsxData; } set { _bsxData = value; } }
        public string BsxXml { get { return _bsxXml; } set { _bsxXml = value; } }

        internal override void Load(Snes snes)
        {
            throw new NotImplementedException();
        }

        internal override void Refresh()
        {
            throw new NotImplementedException();
        }
    }

    public class BsxCartridge : Cartridge
    {
        protected byte[] _bsxData;
        protected string _bsxXml;

        byte[] _ram;
        byte[] _pram;

        public byte[] BsxData { get { return _bsxData; } set { _bsxData = value; } }
        public string BsxXml { get { return _bsxXml; } set { _bsxXml = value; } }

        public byte[] Ram { get { return _ram; } set { _ram = value; } }
        public byte[] Pram { get { return _pram; } set { _pram = value; } }

        internal override void Load(Snes snes)
        {
            throw new NotImplementedException();
        }

        internal override void Refresh()
        {
            throw new NotImplementedException();
        }
    }

    public class SufamiTurboCartridge : Cartridge
    {
        protected byte[] _stAData;
        protected string _stAXml;
        protected byte[] _stBData;
        protected string _stBXml;

        protected byte[] _stARam;
        protected byte[] _stBRam;

        public byte[] StAData { get { return _stAData; } set { _stAData = value; } }
        public string stAXml { get { return _stAXml; } set { _stAXml = value; } }
        public byte[] StBData { get { return _stBData; } set { _stBData = value; } }
        public string stBXml { get { return _stBXml; } set { _stBXml = value; } }

        public byte[] StARam { get { return _stARam; } set { _stARam = value; } }
        public byte[] StBRam { get { return _stBRam; } set { _stBRam = value; } }

        internal override void Load(Snes snes)
        {
            throw new NotImplementedException();
        }

        internal override void Refresh()
        {
            throw new NotImplementedException();
        }
    }

    public class SuperGameBoyCartridge : Cartridge
    {
        protected byte[] _gbData;
        protected string _gbXml;

        protected byte[] _gbRam;
        protected byte[] _gbRtc;

        public byte[] GbData { get { return _gbData; } set { _gbData = value; } }
        public string GbXml { get { return _gbXml; } set { _gbXml = value; } }

        public byte[] GbRam { get { return _gbRam; } set { _gbRam = value; } }
        public byte[] GbRtc { get { return _gbRtc; } set { _gbRtc = value; } }

        internal override void Load(Snes snes)
        {
            throw new NotImplementedException();
        }

        internal override void Refresh()
        {
            throw new NotImplementedException();
        }
    }
}
