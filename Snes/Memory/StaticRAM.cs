﻿
using System.Collections;
namespace Snes
{
    class StaticRAM : Memory
    {
        public static StaticRAM wram = new StaticRAM(128 * 1024);
        public static StaticRAM apuram = new StaticRAM(64 * 1024);
        public static StaticRAM vram = new StaticRAM(64 * 1024);
        public static StaticRAM oam = new StaticRAM(544);
        public static StaticRAM cgram = new StaticRAM(512);

        public byte[] data()
        {
            return data_;
        }

        public override uint size()
        {
            return size_;
        }

        public override IEnumerable read(uint addr, Result result)
        {
            result.Value = data_[addr];
            yield break;
        }

        public override IEnumerable write(uint addr, byte n)
        {
            data_[addr] = n;
            yield break;
        }

        public byte this[uint addr]
        {
            get
            {
                return data_[addr];
            }
            set
            {
                data_[addr] = value;
            }
        }

        public StaticRAM(uint n)
        {
            size_ = n;
            data_ = new byte[size_];
        }

        private byte[] data_;
        private uint size_;
    }
}
