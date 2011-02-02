using System.Collections.ObjectModel;

namespace Snes
{
    class CheatCode
    {
        public bool enabled;
        public Collection<uint> addr = new Collection<uint>();
        public Collection<byte> data = new Collection<byte>();

        public bool Assign(string s)
        {
            addr.Clear();
            data.Clear();

            var list = s.Replace(" ", "").Split(new char[] { '+' });

            for (uint i = 0; i < list.Length; i++)
            {
                uint addr_;
                byte data_;
                Cheat.Type type_;
                if (Cheat.decode(list[i], out addr_, out data_, out type_) == false)
                {
                    addr.Clear();
                    data.Clear();
                    return false;
                }

                addr.Add(addr_);
                data.Add(data_);
            }

            return true;
        }

        public CheatCode()
        {
            enabled = false;
        }
    }
}
