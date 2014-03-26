using System;

namespace Nall
{
    public class Serializer
    {
        public enum Mode { Load, Save, Size };

        public Mode mode()
        {
            return imode;
        }

        public byte[] data()
        {
            return idata;
        }

        public uint size()
        {
            return isize;
        }

        public uint capacity()
        {
            return icapacity;
        }

        public void integer(bool value, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            integer(value);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void integer(bool value)
        {
            uint size = 1U;
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < size; n++)
                {
                    idata[isize++] = (byte)(Convert.ToUInt32(value) >> (int)(n << 3));
                }
            }
            else if (imode == Mode.Load)
            {
                value = Convert.ToBoolean(0);
                for (uint n = 0; n < size; n++)
                {
                    value = Convert.ToBoolean(idata[isize++] << (int)(n << 3));
                }
            }
            else if (imode == Mode.Size)
            {
                isize += size;
            }
        }

        public void integer(byte value, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            integer(value);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void integer(byte value)
        {
            uint size = sizeof(byte);
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < size; n++)
                {
                    idata[isize++] = (byte)(value >> (int)(n << 3));
                }
            }
            else if (imode == Mode.Load)
            {
                value = 0;
                for (uint n = 0; n < size; n++)
                {
                    value |= (byte)(idata[isize++] << (int)(n << 3));
                }
            }
            else if (imode == Mode.Size)
            {
                isize += size;
            }
        }

        public void integer(short value, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            integer(value);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void integer(short value)
        {
            uint size = sizeof(short);
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < size; n++)
                {
                    idata[isize++] = (byte)(value >> (int)(n << 3));
                }
            }
            else if (imode == Mode.Load)
            {
                value = 0;
                for (uint n = 0; n < size; n++)
                {
                    value |= (short)(idata[isize++] << (int)(n << 3));
                }
            }
            else if (imode == Mode.Size)
            {
                isize += size;
            }
        }

        public void integer(ushort value, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            integer(value);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void integer(ushort value)
        {
            uint size = sizeof(ushort);
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < size; n++)
                {
                    idata[isize++] = (byte)(value >> (int)(n << 3));
                }
            }
            else if (imode == Mode.Load)
            {
                value = 0;
                for (uint n = 0; n < size; n++)
                {
                    value |= (ushort)(idata[isize++] << (int)(n << 3));
                }
            }
            else if (imode == Mode.Size)
            {
                isize += size;
            }
        }

        public void integer(uint value, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            integer(value);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void integer(uint value)
        {
            uint size = sizeof(uint);
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < size; n++)
                {
                    idata[isize++] = (byte)(value >> (int)(n << 3));
                }
            }
            else if (imode == Mode.Load)
            {
                value = 0;
                for (uint n = 0; n < size; n++)
                {
                    value |= (uint)(idata[isize++] << (int)(n << 3));
                }
            }
            else if (imode == Mode.Size)
            {
                isize += size;
            }
        }

        public void integer(int value, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            integer(value);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void integer(int value)
        {
            uint size = sizeof(int);
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < size; n++)
                {
                    idata[isize++] = (byte)(value >> (int)(n << 3));
                }
            }
            else if (imode == Mode.Load)
            {
                value = 0;
                for (uint n = 0; n < size; n++)
                {
                    value |= (idata[isize++] << (int)(n << 3));
                }
            }
            else if (imode == Mode.Size)
            {
                isize += size;
            }
        }

        public void integer(long value, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            integer(value);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void integer(long value)
        {
            uint size = sizeof(long);
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < size; n++)
                {
                    idata[isize++] = (byte)(value >> (int)(n << 3));
                }
            }
            else if (imode == Mode.Load)
            {
                value = 0;
                for (uint n = 0; n < size; n++)
                {
                    value |= (uint)(idata[isize++] << (int)(n << 3));
                }
            }
            else if (imode == Mode.Size)
            {
                isize += size;
            }
        }

        public void array(int[] array_, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            array(array_);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void array(int[] array)
        {
            for (uint n = 0; n < array.Length; n++)
            {
                integer(array[n]);
            }
        }

        public void array(int[] array_, uint size, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            array(array_, size);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void array(int[] array, uint size)
        {
            for (uint n = 0; n < size; n++)
            {
                integer(array[n]);
            }
        }

        public void array(byte[] array_, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            array(array_);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void array(byte[] array)
        {
            for (uint n = 0; n < array.Length; n++)
            {
                integer(array[n]);
            }
        }

        public void array(byte[] array_, uint size, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            array(array_, size);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void array(byte[] array, uint size)
        {
            for (uint n = 0; n < size; n++)
            {
                integer(array[n]);
            }
        }

        public void array(bool[] array_, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            array(array_);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void array(bool[] array)
        {
            for (uint n = 0; n < array.Length; n++)
            {
                integer(array[n]);
            }
        }

        public void array(ushort[] array_, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            array(array_);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void array(ushort[] array)
        {
            for (uint n = 0; n < array.Length; n++)
            {
                integer(array[n]);
            }
        }

        public void array(short[] array_, string name)
        {
            if (imode == Mode.Save)
            {
                for (uint n = 0; n < name.Length; n++)
                {
                    idata[isize++] = (byte)name[(int)n];
                }
                idata[isize++] = (byte)':';
                idata[isize++] = (byte)' ';
            }
            else if (imode == Mode.Size)
            {
                isize += (uint)name.Length + 3;
            }

            array(array_);
            if (imode == Mode.Save)
            {
                idata[isize++] = (byte)'\n';
            }
        }

        public void array(short[] array)
        {
            for (uint n = 0; n < array.Length; n++)
            {
                integer(array[n]);
            }
        }


        //copy
        public Serializer Copy(Serializer s)
        {

            imode = s.imode;
            idata = new byte[s.icapacity];
            isize = s.isize;
            icapacity = s.icapacity;

            Array.Copy(s.idata, idata, (int)s.icapacity);
            return this;
        }

        public Serializer(Serializer s)
        {
            Copy(s);
        }

        //construction
        public Serializer()
        {
            imode = Mode.Size;
            idata = null;
            isize = 0;
        }

        public Serializer(uint capacity)
        {
            imode = Mode.Save;
            idata = new byte[capacity];
            isize = 0;
            icapacity = capacity;
        }

        public Serializer(byte[] data, uint capacity)
        {
            imode = Mode.Load;
            idata = new byte[capacity];
            isize = 0;
            icapacity = capacity;
            Array.Copy(data, idata, (int)capacity);
        }

        private Mode imode;
        private byte[] idata;
        private uint isize;
        private uint icapacity;
    }
}
