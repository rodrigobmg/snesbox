#if COMPATIBILITY || PERFORMANCE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Snes
{
	class SPCStateCopier
	{
		private SPCDSP.DSPCopyFunction func;
		private Stream buf;

		public SPCStateCopier(Stream p, SPCDSP.DSPCopyFunction f)
		{
			func = f;
			buf = p;
		}

		public void copy(byte[] state, uint size, string name)
		{
			name += ": ";
			var nameBytes = new UTF8Encoding().GetBytes(name);
			var list = new List<byte>();
			list.AddRange(nameBytes);
			list.AddRange(state);
			list.AddRange(new byte[] { (byte)'\n' });
			func(buf, list.ToArray(), (uint)list.ToArray().Length);
		}

		public int copy_int(int state, int size, string name)
		{
			byte[] s;
			if (size < 2)
			{
				s = new byte[1];
				s[0] = (byte)state;
			}
			else
			{
				s = BitConverter.GetBytes(size == 1 ? (byte)state : (ushort)state);
			}
			name += ": ";
			var nameBytes = new UTF8Encoding().GetBytes(name);
			var list = new List<byte>();
			list.AddRange(nameBytes);
			list.AddRange(s);
			list.AddRange(new byte[] { (byte)'\n' });
			func(buf, list.ToArray(), (uint)list.ToArray().Length);
			if (size < 2)
			{
				return s[0];
			}
			else
			{
				return BitConverter.ToUInt16(s, 0);
			}
		}

		public void skip(int count)
		{
			if (count > 0)
			{
				byte[] temp = new byte[64];

				do
				{
					int n = temp.Length;
					if (n > count)
						n = count;
					count -= n;
					func(buf, temp, (uint)n);
				}
				while (Convert.ToBoolean(count));
			}
		}

		public void extra()
		{
			int n = 0;
			SPCCopy(sizeof(byte), n, "n");
			skip(n);
		}

		public void SPCCopy(int size, object state, string name)
		{
			var state_ = copy_int(Convert.ToInt32(state), size, name);
			//TODO: Fix this assertion
			//Debug.Assert(Convert.ToInt32(state) == state_);
		}
	}
}
#endif
