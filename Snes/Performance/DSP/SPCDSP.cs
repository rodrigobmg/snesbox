#if COMPATIBILITY || PERFORMANCE
using System;
using System.Diagnostics;
using System.IO;

namespace Snes
{
	partial class SPCDSP
	{
		public delegate void DSPCopyFunction(Stream io, byte[] state, uint size);

		// Setup

		private void Clamp16(ref int io)
		{
			if ((short)io != io)
				io = (io >> 31) ^ 0x7FFF;
		}


		// Initializes DSP and has it use the 64K RAM provided
		public void init(object ram_64k)
		{
			m.ram = (byte[])ram_64k;
			mute_voices(0);
			disable_surround(false);
			set_output(null, 0);
			reset();

#if DEBUG
			unchecked
			{
				// be sure this sign-extends
				Debug.Assert((short)0x8000 == -0x8000);

				// be sure right shift preserves sign
				Debug.Assert((-1 >> 1) == -1);

				// check clamp macro
				int i;
				i = +0x8000;
				Clamp16(ref i);
				Debug.Assert(i == +0x7FFF);
				i = -0x8001;
				Clamp16(ref i);
				Debug.Assert(i == -0x8000);
			}
#endif
		}

		// Sets destination for output samples. If out is NULL or out_size is 0,
		// doesn't generate any.
		public void set_output(short[] _out, int size)
		{
			Debug.Assert((size & 1) == 0); // must be even
			if (ReferenceEquals(_out, null))
			{
				_out = m.extra;
				size = extra_size;
			}
			m._out = new ArraySegment<short>(_out, 0, size);
		}

		// Number of samples written to output since it was last set, always
		// a multiple of 2. Undefined if more samples were generated than
		// output buffer could hold.
		public int sample_count()
		{
			return m._out.Offset;
		}

		// Emulation

		public static readonly byte[] initial_regs = new byte[SPCDSP.register_count] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

		// Resets DSP to power-on state
		public void reset()
		{
			load(initial_regs);
		}

		// Emulates pressing reset switch on SNES
		public void soft_reset()
		{
			m.regs[(int)GlobalReg.flg] = 0xE0;
			soft_reset_common();
		}

		// Reads/writes DSP registers. For accuracy, you must first call run()
		// to catch the DSP up to present.
		public int read(int addr)
		{
			Debug.Assert((uint)addr < register_count);
			return m.regs[addr];
		}

		public void write(int addr, int data)
		{
			Debug.Assert((uint)addr < register_count);

			m.regs[addr] = (byte)data;
			switch ((VoiceReg)(addr & 0x0F))
			{
				case VoiceReg.envx:
					m.envx_buf = (byte)data;
					break;
				case VoiceReg.outx:
					m.outx_buf = (byte)data;
					break;
				case (VoiceReg)0x0C:
					if (addr == (int)GlobalReg.kon)
						m.new_kon = (byte)data;

					if (addr == (int)GlobalReg.endx) // always cleared, regardless of data written
					{
						m.endx_buf = 0;
						m.regs[(int)GlobalReg.endx] = 0;
					}
					break;
			}
		}

		public bool Phase(int n, int clocks_remain)
		{
			return Convert.ToBoolean(n) && !Convert.ToBoolean(--clocks_remain);
		}

		// Runs DSP for specified number of clocks (~1024000 per second). Every 32 clocks
		// a pair of samples is be generated.
		public void run(int clocks_remain)
		{
			Debug.Assert(clocks_remain > 0);

			int phase = m.phase;
			m.phase = (phase + clocks_remain) & 31;
			do
			{
				switch (phase)
				{
					case 0:
						voice_V5(new ArraySegment<Voice>(m.voices, 0, m.voices.Length - 0));
						voice_V2(new ArraySegment<Voice>(m.voices, 1, m.voices.Length - 1));
						if (Phase(1, clocks_remain))
							break;
						goto case 1;
					case 1:
						voice_V6(new ArraySegment<Voice>(m.voices, 0, m.voices.Length - 0));
						voice_V3(new ArraySegment<Voice>(m.voices, 1, m.voices.Length - 1));
						if (Phase(2, clocks_remain))
							break;
						goto case 2;
					case 2:
						voice_V7_V4_V1(new ArraySegment<Voice>(m.voices, 0, m.voices.Length - 0));
						if (Phase(3, clocks_remain))
							break;
						goto case 3;
					case 3:
						voice_V8_V5_V2(new ArraySegment<Voice>(m.voices, 0, m.voices.Length - 0));
						if (Phase(4, clocks_remain))
							break;
						goto case 4;
					case 4:
						voice_V9_V6_V3(new ArraySegment<Voice>(m.voices, 0, m.voices.Length - 0));
						if (Phase(5, clocks_remain))
							break;
						goto case 5;
					case 5:
						voice_V7_V4_V1(new ArraySegment<Voice>(m.voices, 1, m.voices.Length - 1));
						if (Phase(6, clocks_remain))
							break;
						goto case 6;
					case 6:
						voice_V8_V5_V2(new ArraySegment<Voice>(m.voices, 1, m.voices.Length - 1));
						if (Phase(7, clocks_remain))
							break;
						goto case 7;
					case 7:
						voice_V9_V6_V3(new ArraySegment<Voice>(m.voices, 1, m.voices.Length - 1));
						if (Phase(8, clocks_remain))
							break;
						goto case 8;
					case 8:
						voice_V7_V4_V1(new ArraySegment<Voice>(m.voices, 2, m.voices.Length - 2));
						if (Phase(9, clocks_remain))
							break;
						goto case 9;
					case 9:
						voice_V8_V5_V2(new ArraySegment<Voice>(m.voices, 2, m.voices.Length - 2));
						if (Phase(10, clocks_remain))
							break;
						goto case 10;
					case 10:
						voice_V9_V6_V3(new ArraySegment<Voice>(m.voices, 2, m.voices.Length - 2));
						if (Phase(11, clocks_remain))
							break;
						goto case 11;
					case 11:
						voice_V7_V4_V1(new ArraySegment<Voice>(m.voices, 3, m.voices.Length - 3));
						if (Phase(12, clocks_remain))
							break;
						goto case 12;
					case 12:
						voice_V8_V5_V2(new ArraySegment<Voice>(m.voices, 3, m.voices.Length - 3));
						if (Phase(13, clocks_remain))
							break;
						goto case 13;
					case 13:
						voice_V9_V6_V3(new ArraySegment<Voice>(m.voices, 3, m.voices.Length - 3));
						if (Phase(14, clocks_remain))
							break;
						goto case 14;
					case 14:
						voice_V7_V4_V1(new ArraySegment<Voice>(m.voices, 4, m.voices.Length - 4));
						if (Phase(15, clocks_remain))
							break;
						goto case 15;
					case 15:
						voice_V8_V5_V2(new ArraySegment<Voice>(m.voices, 4, m.voices.Length - 4));
						if (Phase(16, clocks_remain))
							break;
						goto case 16;
					case 16:
						voice_V9_V6_V3(new ArraySegment<Voice>(m.voices, 4, m.voices.Length - 4));
						if (Phase(17, clocks_remain))
							break;
						goto case 17;
					case 17:
						voice_V1(new ArraySegment<Voice>(m.voices, 0, m.voices.Length - 0));
						voice_V7(new ArraySegment<Voice>(m.voices, 5, m.voices.Length - 5));
						voice_V4(new ArraySegment<Voice>(m.voices, 6, m.voices.Length - 6));
						if (Phase(18, clocks_remain))
							break;
						goto case 18;
					case 18:
						voice_V8_V5_V2(new ArraySegment<Voice>(m.voices, 5, m.voices.Length - 5));
						if (Phase(19, clocks_remain))
							break;
						goto case 19;
					case 19:
						voice_V9_V6_V3(new ArraySegment<Voice>(m.voices, 5, m.voices.Length - 5));
						if (Phase(20, clocks_remain))
							break;
						goto case 20;
					case 20:
						voice_V1(new ArraySegment<Voice>(m.voices, 1, m.voices.Length - 1));
						voice_V7(new ArraySegment<Voice>(m.voices, 6, m.voices.Length - 6));
						voice_V4(new ArraySegment<Voice>(m.voices, 7, m.voices.Length - 7));
						if (Phase(21, clocks_remain))
							break;
						goto case 21;
					case 21:
						voice_V8(new ArraySegment<Voice>(m.voices, 6, m.voices.Length - 6));
						voice_V5(new ArraySegment<Voice>(m.voices, 7, m.voices.Length - 7));
						voice_V2(new ArraySegment<Voice>(m.voices, 0, m.voices.Length - 0));
						if (Phase(22, clocks_remain))
							break;
						goto case 22;
					case 22:
						voice_V3a(new ArraySegment<Voice>(m.voices, 0, m.voices.Length - 0));
						voice_V9(new ArraySegment<Voice>(m.voices, 6, m.voices.Length - 6));
						voice_V6(new ArraySegment<Voice>(m.voices, 7, m.voices.Length - 7));
						echo_22();
						if (Phase(23, clocks_remain))
							break;
						goto case 23;
					case 23:
						voice_V7(new ArraySegment<Voice>(m.voices, 7, m.voices.Length - 7));
						echo_23();
						if (Phase(24, clocks_remain))
							break;
						goto case 24;
					case 24:
						voice_V8(new ArraySegment<Voice>(m.voices, 7, m.voices.Length - 7));
						echo_24();
						if (Phase(25, clocks_remain))
							break;
						goto case 25;
					case 25:
						voice_V3b(new ArraySegment<Voice>(m.voices, 0, m.voices.Length - 0));
						voice_V9(new ArraySegment<Voice>(m.voices, 7, m.voices.Length - 7));
						echo_25();
						if (Phase(26, clocks_remain))
							break;
						goto case 26;
					case 26:
						echo_26();
						if (Phase(27, clocks_remain))
							break;
						goto case 27;
					case 27:
						misc_27();
						echo_27();
						if (Phase(28, clocks_remain))
							break;
						goto case 28;
					case 28:
						misc_28();
						echo_28();
						if (Phase(29, clocks_remain))
							break;
						goto case 29;
					case 29:
						misc_29();
						echo_29();
						if (Phase(30, clocks_remain))
							break;
						goto case 30;
					case 30:
						misc_30();
						voice_V3c(new ArraySegment<Voice>(m.voices, 0, m.voices.Length - 0));
						echo_30();
						if (Phase(31, clocks_remain))
							break;
						goto case 31;
					case 31:
						voice_V4(new ArraySegment<Voice>(m.voices, 0, m.voices.Length - 0));
						voice_V1(new ArraySegment<Voice>(m.voices, 2, m.voices.Length - 2));
						break;
				}
			}
			while (Convert.ToBoolean(--clocks_remain));
		}

		// Sound control

		// Mutes voices corresponding to non-zero bits in mask (issues repeated KOFF events).
		// Reduces emulation accuracy.
		public const int voice_count = 8;

		public void mute_voices(int mask)
		{
			m.mute_mask = mask;
		}

		// State

		// Resets DSP and uses supplied values to initialize registers
		public const int register_count = 128;

		public void load(byte[] regs)
		{
			Array.Copy(regs, m.regs, m.regs.Length);

			//TODO: What the HELL is this doing?
			//memset( &m.regs [register_count], 0, offsetof (state_t,ram) - register_count );

			// Internal state
			for (int i = voice_count; --i >= 0; )
			{
				Voice v = m.voices[i];
				v.brr_offset = 1;
				v.vbit = 1 << i;
				v.regs = new ArraySegment<byte>(m.regs, i * 0x10, m.regs.Length - (i * 0x10));
			}
			m.new_kon = m.regs[(int)GlobalReg.kon];
			m.t_dir = m.regs[(int)GlobalReg.dir];
			m.t_esa = m.regs[(int)GlobalReg.esa];

			soft_reset_common();
		}

		// Saves/loads exact emulator state
		public const int state_size = 640; // maximum space needed when saving

		public void copy_state(Stream io, DSPCopyFunction copy)
		{
			SPCStateCopier copier = new SPCStateCopier(io, copy);

			// DSP registers
			copier.copy(m.regs, register_count, "m.regs");

			// Internal state

			// Voices
			for (int i = 0; i < voice_count; i++)
			{
				Voice v = m.voices[i];

				// BRR buffer
				for (int j = 0; j < brr_buf_size; j++)
				{
					int s = v.buf[j];
					copier.SPCCopy(sizeof(short), s, "s");
					//v.buf[j] = v.buf[j + brr_buf_size] = s;
				}

				copier.SPCCopy(sizeof(ushort), v.interp_pos, "v->interp_pos");
				copier.SPCCopy(sizeof(ushort), v.brr_addr, "v->brr_addr");
				copier.SPCCopy(sizeof(ushort), v.env, "v->env");
				copier.SPCCopy(sizeof(short), v.hidden_env, "v->hidden_env");
				copier.SPCCopy(sizeof(byte), v.buf_pos, "v->buf_pos");
				copier.SPCCopy(sizeof(byte), v.brr_offset, "v->brr_offset");
				copier.SPCCopy(sizeof(byte), v.kon_delay, "v->kon_delay");
				{
					int mode = (int)v.env_mode;
					copier.SPCCopy(sizeof(byte), mode, "m");
					//v.env_mode = (EnvMode)mode;
				}
				copier.SPCCopy(sizeof(byte), v.t_envx_out, "v->t_envx_out");

				copier.extra();
			}

			// Echo history
			for (int i = 0; i < echo_hist_size; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					//TODO: bsnes uses echo_hist_pos here, is this a bug?
					int s = m.echo_hist[i][j];
					copier.SPCCopy(sizeof(short), s, "s");
					//m.echo_hist[i][j] = s; // write back at offset 0
				}
			}
			//m.echo_hist_pos = new ArraySegment<int[]>(m.echo_hist, 0, m.echo_hist.Length);
			//for (int i = 0; i < echo_hist_size; i++)
			//{
			//    Array.Copy(m.echo_hist[i], m.echo_hist[echo_hist_size + i], m.echo_hist[i].Length);
			//}

			// Misc
			copier.SPCCopy(sizeof(byte), m.every_other_sample, "m.every_other_sample");
			copier.SPCCopy(sizeof(byte), m.kon, "m.kon");

			copier.SPCCopy(sizeof(ushort), m.noise, "m.noise");
			copier.SPCCopy(sizeof(ushort), m.counter, "m.counter");
			copier.SPCCopy(sizeof(ushort), m.echo_offset, "m.echo_offset");
			copier.SPCCopy(sizeof(ushort), m.echo_length, "m.echo_length");
			copier.SPCCopy(sizeof(byte), m.phase, "m.phase");

			copier.SPCCopy(sizeof(byte), m.new_kon, "m.new_kon");
			copier.SPCCopy(sizeof(byte), m.endx_buf, "m.endx_buf");
			copier.SPCCopy(sizeof(byte), m.envx_buf, "m.envx_buf");
			copier.SPCCopy(sizeof(byte), m.outx_buf, "m.outx_buf");

			copier.SPCCopy(sizeof(byte), m.t_pmon, "m.t_pmon");
			copier.SPCCopy(sizeof(byte), m.t_non, "m.t_non");
			copier.SPCCopy(sizeof(byte), m.t_eon, "m.t_eon");
			copier.SPCCopy(sizeof(byte), m.t_dir, "m.t_dir");
			copier.SPCCopy(sizeof(byte), m.t_koff, "m.t_koff");

			copier.SPCCopy(sizeof(ushort), m.t_brr_next_addr, "m.t_brr_next_addr");
			copier.SPCCopy(sizeof(byte), m.t_adsr0, "m.t_adsr0");
			copier.SPCCopy(sizeof(byte), m.t_brr_header, "m.t_brr_header");
			copier.SPCCopy(sizeof(byte), m.t_brr_byte, "m.t_brr_byte");
			copier.SPCCopy(sizeof(byte), m.t_srcn, "m.t_srcn");
			copier.SPCCopy(sizeof(byte), m.t_esa, "m.t_esa");
			copier.SPCCopy(sizeof(byte), m.t_echo_enabled, "m.t_echo_enabled");

			copier.SPCCopy(sizeof(short), m.t_main_out[0], "m.t_main_out [0]");
			copier.SPCCopy(sizeof(short), m.t_main_out[1], "m.t_main_out [1]");
			copier.SPCCopy(sizeof(short), m.t_echo_out[0], "m.t_echo_out [0]");
			copier.SPCCopy(sizeof(short), m.t_echo_out[1], "m.t_echo_out [1]");
			copier.SPCCopy(sizeof(short), m.t_echo_in[0], "m.t_echo_in [0]");
			copier.SPCCopy(sizeof(short), m.t_echo_in[1], "m.t_echo_in [1]");

			copier.SPCCopy(sizeof(ushort), m.t_dir_addr, "m.t_dir_addr");
			copier.SPCCopy(sizeof(ushort), m.t_pitch, "m.t_pitch");
			copier.SPCCopy(sizeof(short), m.t_output, "m.t_output");
			copier.SPCCopy(sizeof(ushort), m.t_echo_ptr, "m.t_echo_ptr");
			copier.SPCCopy(sizeof(byte), m.t_looped, "m.t_looped");

			copier.extra();
		}

		// Returns non-zero if new key-on events occurred since last call
		public bool check_kon()
		{
			bool old = m.kon_check;
			m.kon_check = Convert.ToBoolean(0);
			return old;
		}

		// DSP register addresses

		// Global registers
		public enum GlobalReg { mvoll = 0x0c, mvolr = 0x1c, evoll = 0x2c, evolr = 0x3c, kon = 0x4c, koff = 0x5c, flg = 0x6c, endx = 0x7c, efb = 0x0d, pmon = 0x2d, non = 0x3d, eon = 0x4d, dir = 0x5d, esa = 0x6d, edl = 0x7d, fir = 0x0f }

		// Voice registers
		private enum VoiceReg { voll = 0x00, volr = 0x01, pitchl = 0x02, pitchh = 0x03, srcn = 0x04, adsr0 = 0x05, adsr1 = 0x06, gain = 0x07, envx = 0x08, outx = 0x09 }

		public const int extra_size = 16;

		public short[] extra()
		{
			return m.extra;
		}

		public ArraySegment<short> out_pos()
		{
			return m._out;
		}

		public void disable_surround(bool disable) { } // not supported

		public const int echo_hist_size = 8;

		public enum EnvMode { release, attack, decay, sustain }

		public const int brr_buf_size = 12;

		private const int brr_block_size = 9;

		private State m = new State();

		private void init_counter()
		{
			m.counter = 0;
		}

		private const int simple_counter_range = 2048 * 5 * 3; // 30720
		private void run_counters()
		{
			if (--m.counter < 0)
			{
				m.counter = simple_counter_range - 1;
			}
		}

		private static readonly uint[] counter_rates = new uint[32] { simple_counter_range + 1 /* never fires*/, 2048, 1536, 1280, 1024, 768, 640, 512, 384, 320, 256, 192, 160, 128, 96, 80, 64, 48, 40, 32, 24, 20, 16, 12, 10, 8, 6, 5, 4, 3, 2, 1 };
		private static readonly uint[] counter_offsets = new uint[32] { 1, 0, 1040, 536, 0, 1040, 536, 0, 1040, 536, 0, 1040, 536, 0, 1040, 536, 0, 1040, 536, 0, 1040, 536, 0, 1040, 536, 0, 1040, 536, 0, 1040, 0, 0 };

		private uint read_counter(int rate)
		{
			return ((uint)m.counter + counter_offsets[rate]) % counter_rates[rate];
		}

		private static readonly short[] gauss = new short[512]
		{
			0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
			1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   2,   2,   2,   2,   2,
			2,   2,   3,   3,   3,   3,   3,   4,   4,   4,   4,   4,   5,   5,   5,   5,
			6,   6,   6,   6,   7,   7,   7,   8,   8,   8,   9,   9,   9,  10,  10,  10,
			11,  11,  11,  12,  12,  13,  13,  14,  14,  15,  15,  15,  16,  16,  17,  17,
			18,  19,  19,  20,  20,  21,  21,  22,  23,  23,  24,  24,  25,  26,  27,  27,
			28,  29,  29,  30,  31,  32,  32,  33,  34,  35,  36,  36,  37,  38,  39,  40,
			41,  42,  43,  44,  45,  46,  47,  48,  49,  50,  51,  52,  53,  54,  55,  56,
			58,  59,  60,  61,  62,  64,  65,  66,  67,  69,  70,  71,  73,  74,  76,  77,
			78,  80,  81,  83,  84,  86,  87,  89,  90,  92,  94,  95,  97,  99, 100, 102,
			104, 106, 107, 109, 111, 113, 115, 117, 118, 120, 122, 124, 126, 128, 130, 132,
			134, 137, 139, 141, 143, 145, 147, 150, 152, 154, 156, 159, 161, 163, 166, 168,
			171, 173, 175, 178, 180, 183, 186, 188, 191, 193, 196, 199, 201, 204, 207, 210,
			212, 215, 218, 221, 224, 227, 230, 233, 236, 239, 242, 245, 248, 251, 254, 257,
			260, 263, 267, 270, 273, 276, 280, 283, 286, 290, 293, 297, 300, 304, 307, 311,
			314, 318, 321, 325, 328, 332, 336, 339, 343, 347, 351, 354, 358, 362, 366, 370,
			374, 378, 381, 385, 389, 393, 397, 401, 405, 410, 414, 418, 422, 426, 430, 434,
			439, 443, 447, 451, 456, 460, 464, 469, 473, 477, 482, 486, 491, 495, 499, 504,
			508, 513, 517, 522, 527, 531, 536, 540, 545, 550, 554, 559, 563, 568, 573, 577,
			582, 587, 592, 596, 601, 606, 611, 615, 620, 625, 630, 635, 640, 644, 649, 654,
			659, 664, 669, 674, 678, 683, 688, 693, 698, 703, 708, 713, 718, 723, 728, 732,
			737, 742, 747, 752, 757, 762, 767, 772, 777, 782, 787, 792, 797, 802, 806, 811,
			816, 821, 826, 831, 836, 841, 846, 851, 855, 860, 865, 870, 875, 880, 884, 889,
			894, 899, 904, 908, 913, 918, 923, 927, 932, 937, 941, 946, 951, 955, 960, 965,
			969, 974, 978, 983, 988, 992, 997,1001,1005,1010,1014,1019,1023,1027,1032,1036,
			1040,1045,1049,1053,1057,1061,1066,1070,1074,1078,1082,1086,1090,1094,1098,1102,
			1106,1109,1113,1117,1121,1125,1128,1132,1136,1139,1143,1146,1150,1153,1157,1160,
			1164,1167,1170,1174,1177,1180,1183,1186,1190,1193,1196,1199,1202,1205,1207,1210,
			1213,1216,1219,1221,1224,1227,1229,1232,1234,1237,1239,1241,1244,1246,1248,1251,
			1253,1255,1257,1259,1261,1263,1265,1267,1269,1270,1272,1274,1275,1277,1279,1280,
			1282,1283,1284,1286,1287,1288,1290,1291,1292,1293,1294,1295,1296,1297,1297,1298,
			1299,1300,1300,1301,1302,1302,1303,1303,1303,1304,1304,1304,1304,1304,1305,1305,
		};

		private int interpolate(Voice v)
		{ 	// Make pointers into gaussian based on fractional position between samples
			int offset = v.interp_pos >> 4 & 0xFF;
			ArraySegment<short> fwd = new ArraySegment<short>(gauss, 255 - offset, gauss.Length - (255 - offset));
			ArraySegment<short> rev = new ArraySegment<short>(gauss, offset, gauss.Length - offset); // mirror left half of gaussian

			ArraySegment<int> _in = new ArraySegment<int>(v.buf, (v.interp_pos >> 12) + v.buf_pos, v.buf.Length - ((v.interp_pos >> 12) + v.buf_pos));
			int _out;
			_out = (fwd.Array[fwd.Offset + 0] * _in.Array[_in.Offset + 0]) >> 11;
			_out += (fwd.Array[fwd.Offset + 256] * _in.Array[_in.Offset + 1]) >> 11;
			_out += (rev.Array[rev.Offset + 256] * _in.Array[_in.Offset + 2]) >> 11;
			_out = (short)_out;
			_out += (rev.Array[rev.Offset + 0] * _in.Array[_in.Offset + 3]) >> 11;

			Clamp16(ref _out);
			_out &= ~1;
			return _out;
		}

		private void run_envelope(Voice v)
		{
			int env = v.env;
			if (v.env_mode == EnvMode.release) // 60%
			{
				if ((env -= 0x8) < 0)
				{
					env = 0;
				}
				v.env = env;
			}
			else
			{
				int rate;
				int env_data = v.regs.Array[v.regs.Offset + (int)VoiceReg.adsr1];
				if (Convert.ToBoolean(m.t_adsr0 & 0x80)) // 99% ADSR
				{
					if (v.env_mode >= EnvMode.decay) // 99%
					{
						env--;
						env -= env >> 8;
						rate = env_data & 0x1F;
						if (v.env_mode == EnvMode.decay) // 1%
						{
							rate = (m.t_adsr0 >> 3 & 0x0E) + 0x10;
						}
					}
					else // env_attack
					{
						rate = (m.t_adsr0 & 0x0F) * 2 + 1;
						env += rate < 31 ? 0x20 : 0x400;
					}
				}
				else // GAIN
				{
					int mode;
					env_data = v.regs.Array[v.regs.Offset + (int)VoiceReg.gain];
					mode = env_data >> 5;
					if (mode < 4) // direct
					{
						env = env_data * 0x10;
						rate = 31;
					}
					else
					{
						rate = env_data & 0x1F;
						if (mode == 4) // 4: linear decrease
						{
							env -= 0x20;
						}
						else if (mode < 6) // 5: exponential decrease
						{
							env--;
							env -= env >> 8;
						}
						else // 6,7: linear increase
						{
							env += 0x20;
							if (mode > 6 && (uint)v.hidden_env >= 0x600)
							{
								env += 0x8 - 0x20; // 7: two-slope linear increase
							}
						}
					}
				}

				// Sustain level
				if ((env >> 8) == (env_data >> 5) && v.env_mode == EnvMode.decay)
				{
					v.env_mode = EnvMode.sustain;
				}

				v.hidden_env = env;

				// uint cast because linear decrease going negative also triggers this
				if ((uint)env > 0x7FF)
				{
					env = (env < 0 ? 0 : 0x7FF);
					if (v.env_mode == EnvMode.attack)
					{
						v.env_mode = EnvMode.decay;
					}
				}

				if (!Convert.ToBoolean(read_counter(rate)))
				{
					v.env = env; // nothing else is controlled by the counter
				}
			}
		}

		private void decode_brr(Voice v)
		{
			// Arrange the four input nybbles in 0xABCD order for easy decoding
			int nybbles = m.t_brr_byte * 0x100 + m.ram[(v.brr_addr + v.brr_offset + 1) & 0xFFFF];

			int header = m.t_brr_header;

			// Write to next four samples in circular buffer
			ArraySegment<int> pos = new ArraySegment<int>(v.buf, v.buf_pos, v.buf.Length - v.buf_pos);
			ArraySegment<int> end;
			if ((v.buf_pos += 4) >= brr_buf_size)
			{
				v.buf_pos = 0;
			}

			// Decode four samples
			for (end = new ArraySegment<int>(pos.Array, pos.Offset + 4, pos.Count - 4); pos.Offset < end.Offset; pos = new ArraySegment<int>(pos.Array, pos.Offset + 1, pos.Count - 1), nybbles <<= 4)
			{
				// Extract nybble and sign-extend
				int s = (short)nybbles >> 12;

				// Shift sample based on header
				int shift = header >> 4;
				s = (s << shift) >> 1;
				if (shift >= 0xD) // handle invalid range
				{
					s = (s >> 25) << 11; // same as: s = (s < 0 ? -0x800 : 0)
				}

				// Apply IIR filter (8 is the most commonly used)
				int filter = header & 0x0C;
				int p1 = pos.Array[pos.Offset + brr_buf_size - 1];
				int p2 = pos.Array[pos.Offset + brr_buf_size - 2] >> 1;
				if (filter >= 8)
				{
					s += p1;
					s -= p2;
					if (filter == 8) // s += p1 * 0.953125 - p2 * 0.46875
					{
						s += p2 >> 4;
						s += (p1 * -3) >> 6;
					}
					else // s += p1 * 0.8984375 - p2 * 0.40625
					{
						s += (p1 * -13) >> 7;
						s += (p2 * 3) >> 4;
					}
				}
				else if (Convert.ToBoolean(filter)) // s += p1 * 0.46875
				{
					s += p1 >> 1;
					s += (-p1) >> 5;
				}

				// Adjust and write sample
				Clamp16(ref  s);
				s = (short)(s * 2);
				pos.Array[pos.Offset + brr_buf_size] = pos.Array[pos.Offset + 0] = s; // second copy simplifies wrap-around
			}
		}

		private void misc_27()
		{
			m.t_pmon = m.regs[(int)GlobalReg.pmon] & 0xFE; // voice 0 doesn't support PMON
		}

		private void misc_28()
		{
			m.t_non = m.regs[(int)GlobalReg.non];
			m.t_eon = m.regs[(int)GlobalReg.eon];
			m.t_dir = m.regs[(int)GlobalReg.dir];
		}

		private void misc_29()
		{
			if ((m.every_other_sample ^= 1) != 0)
			{
				m.new_kon &= ~m.kon; // clears KON 63 clocks after it was last read
			}
		}

		private void misc_30()
		{
			if (Convert.ToBoolean(m.every_other_sample))
			{
				m.kon = m.new_kon;
				m.t_koff = m.regs[(int)GlobalReg.koff] | m.mute_mask;
			}

			run_counters();

			// Noise
			if (!Convert.ToBoolean(read_counter(m.regs[(int)GlobalReg.flg] & 0x1F)))
			{
				int feedback = (m.noise << 13) ^ (m.noise << 14);
				m.noise = (feedback & 0x4000) ^ (m.noise >> 1);
			}
		}

		private void voice_output(Voice v, int ch)
		{	// Apply left/right volume
			int amp = (m.t_output * (sbyte)v.regs.Array[v.regs.Offset + (int)VoiceReg.voll + ch]) >> 7;

			// Add to output total
			m.t_main_out[ch] += amp;
			Clamp16(ref m.t_main_out[ch]);

			// Optionally add to echo total
			if (Convert.ToBoolean(m.t_eon & v.vbit))
			{
				m.t_echo_out[ch] += amp;
				Clamp16(ref m.t_echo_out[ch]);
			}
		}

		private void voice_V1(ArraySegment<Voice> v)
		{
			m.t_dir_addr = m.t_dir * 0x100 + m.t_srcn * 4;
			m.t_srcn = v.Array[v.Offset].regs.Array[v.Array[v.Offset].regs.Offset + (int)VoiceReg.srcn];
		}

		private void voice_V2(ArraySegment<Voice> v)
		{ 	// Read sample pointer (ignored if not needed)
			ArraySegment<byte> entry = new ArraySegment<byte>(m.ram, m.t_dir_addr, m.ram.Length - m.t_dir_addr);
			if (!Convert.ToBoolean(v.Array[v.Offset].kon_delay))
			{
				entry = new ArraySegment<byte>(entry.Array, entry.Offset + 2, entry.Count - 2);
			}
			m.t_brr_next_addr = BitConverter.ToUInt16(entry.Array, entry.Offset);

			m.t_adsr0 = v.Array[v.Offset].regs.Array[v.Array[v.Offset].regs.Offset + (int)VoiceReg.adsr0];

			// Read pitch, spread over two clocks
			m.t_pitch = v.Array[v.Offset].regs.Array[v.Array[v.Offset].regs.Offset + (int)VoiceReg.pitchl];
		}

		private void voice_V3(ArraySegment<Voice> v)
		{
			voice_V3a(v);
			voice_V3b(v);
			voice_V3c(v);
		}

		private void voice_V3a(ArraySegment<Voice> v)
		{
			m.t_pitch += (v.Array[v.Offset].regs.Array[v.Array[v.Offset].regs.Offset + (int)VoiceReg.pitchh] & 0x3F) << 8;
		}

		private void voice_V3b(ArraySegment<Voice> v)
		{ 	// Read BRR header and byte
			m.t_brr_byte = m.ram[(v.Array[v.Offset].brr_addr + v.Array[v.Offset].brr_offset) & 0xFFFF];
			m.t_brr_header = m.ram[v.Array[v.Offset].brr_addr]; // brr_addr doesn't need masking
		}

		private void voice_V3c(ArraySegment<Voice> v)
		{
			// Pitch modulation using previous voice's output
			if (Convert.ToBoolean(m.t_pmon & v.Array[v.Offset].vbit))
				m.t_pitch += ((m.t_output >> 5) * m.t_pitch) >> 10;

			if (Convert.ToBoolean(v.Array[v.Offset].kon_delay))
			{
				// Get ready to start BRR decoding on next sample
				if (v.Array[v.Offset].kon_delay == 5)
				{
					v.Array[v.Offset].brr_addr = m.t_brr_next_addr;
					v.Array[v.Offset].brr_offset = 1;
					v.Array[v.Offset].buf_pos = 0;
					m.t_brr_header = 0; // header is ignored on this sample
					m.kon_check = true;
				}

				// Envelope is never run during KON
				v.Array[v.Offset].env = 0;
				v.Array[v.Offset].hidden_env = 0;

				// Disable BRR decoding until last three samples
				v.Array[v.Offset].interp_pos = 0;
				if (Convert.ToBoolean(--v.Array[v.Offset].kon_delay & 3))
				{
					v.Array[v.Offset].interp_pos = 0x4000;
				}

				// Pitch is never added during KON
				m.t_pitch = 0;
			}

			// Gaussian interpolation
			{
				int output = interpolate(v.Array[v.Offset]);

				// Noise
				if (Convert.ToBoolean(m.t_non & v.Array[v.Offset].vbit))
				{
					output = (short)(m.noise * 2);
				}

				// Apply envelope
				m.t_output = (output * v.Array[v.Offset].env) >> 11 & ~1;
				v.Array[v.Offset].t_envx_out = (byte)(v.Array[v.Offset].env >> 4);
			}

			// Immediate silence due to end of sample or soft reset
			if (Convert.ToBoolean(m.regs[(int)GlobalReg.flg] & 0x80) || (m.t_brr_header & 3) == 1)
			{
				v.Array[v.Offset].env_mode = EnvMode.release;
				v.Array[v.Offset].env = 0;
			}

			if (Convert.ToBoolean(m.every_other_sample))
			{
				// KOFF
				if (Convert.ToBoolean(m.t_koff & v.Array[v.Offset].vbit))
				{
					v.Array[v.Offset].env_mode = EnvMode.release;
				}

				// KON
				if (Convert.ToBoolean(m.kon & v.Array[v.Offset].vbit))
				{
					v.Array[v.Offset].kon_delay = 5;
					v.Array[v.Offset].env_mode = EnvMode.attack;
				}
			}

			// Run envelope for next sample
			if (!Convert.ToBoolean(v.Array[v.Offset].kon_delay))
			{
				run_envelope(v.Array[v.Offset]);
			}
		}

		private void voice_V4(ArraySegment<Voice> v)
		{ 	// Decode BRR
			m.t_looped = 0;
			if (v.Array[v.Offset].interp_pos >= 0x4000)
			{
				decode_brr(v.Array[v.Offset]);

				if ((v.Array[v.Offset].brr_offset += 2) >= brr_block_size)
				{
					// Start decoding next BRR block
					Debug.Assert(v.Array[v.Offset].brr_offset == brr_block_size);
					v.Array[v.Offset].brr_addr = (v.Array[v.Offset].brr_addr + brr_block_size) & 0xFFFF;
					if (Convert.ToBoolean(m.t_brr_header & 1))
					{
						v.Array[v.Offset].brr_addr = m.t_brr_next_addr;
						m.t_looped = v.Array[v.Offset].vbit;
					}
					v.Array[v.Offset].brr_offset = 1;
				}
			}

			// Apply pitch
			v.Array[v.Offset].interp_pos = (v.Array[v.Offset].interp_pos & 0x3FFF) + m.t_pitch;

			// Keep from getting too far ahead (when using pitch modulation)
			if (v.Array[v.Offset].interp_pos > 0x7FFF)
			{
				v.Array[v.Offset].interp_pos = 0x7FFF;
			}

			// Output left
			voice_output(v.Array[v.Offset], 0);
		}

		private void voice_V5(ArraySegment<Voice> v)
		{ 	// Output right
			voice_output(v.Array[v.Offset], 1);

			// ENDX, OUTX, and ENVX won't update if you wrote to them 1-2 clocks earlier
			int endx_buf = m.regs[(int)GlobalReg.endx] | m.t_looped;

			// Clear bit in ENDX if KON just began
			if (v.Array[v.Offset].kon_delay == 5)
			{
				endx_buf &= ~v.Array[v.Offset].vbit;
			}
			m.endx_buf = (byte)endx_buf;
		}

		private void voice_V6(ArraySegment<Voice> v)
		{
			m.outx_buf = (byte)(m.t_output >> 8);
		}

		private void voice_V7(ArraySegment<Voice> v)
		{ 	// Update ENDX
			m.regs[(int)GlobalReg.endx] = m.endx_buf;

			m.envx_buf = v.Array[v.Offset].t_envx_out;
		}

		private void voice_V8(ArraySegment<Voice> v)
		{ 	// Update OUTX
			v.Array[v.Offset].regs.Array[v.Array[v.Offset].regs.Offset + (int)VoiceReg.outx] = m.outx_buf;
		}

		private void voice_V9(ArraySegment<Voice> v)
		{ 	// Update ENVX
			v.Array[v.Offset].regs.Array[v.Array[v.Offset].regs.Offset + (int)VoiceReg.envx] = m.envx_buf;
		}

		private void voice_V7_V4_V1(ArraySegment<Voice> v)
		{
			voice_V7(v);
			voice_V1(new ArraySegment<Voice>(v.Array, v.Offset + 3, v.Count - 3));
			voice_V4(new ArraySegment<Voice>(v.Array, v.Offset + 1, v.Count - 1));
		}

		private void voice_V8_V5_V2(ArraySegment<Voice> v)
		{
			voice_V8(v);
			voice_V5(new ArraySegment<Voice>(v.Array, v.Offset + 1, v.Count - 1));
			voice_V2(new ArraySegment<Voice>(v.Array, v.Offset + 2, v.Count - 2));
		}

		private void voice_V9_V6_V3(ArraySegment<Voice> v)
		{
			voice_V9(v);
			voice_V6(new ArraySegment<Voice>(v.Array, v.Offset + 1, v.Count - 1));
			voice_V3(new ArraySegment<Voice>(v.Array, v.Offset + 2, v.Count - 2));
		}

		private void echo_read(int ch)
		{
			int s = BitConverter.ToInt16(m.ram, m.t_echo_ptr + ch * 2);
			// second copy simplifies wrap-around handling
			m.echo_hist_pos.Array[m.echo_hist_pos.Offset + 0][ch] = m.echo_hist_pos.Array[m.echo_hist_pos.Offset + 8][ch] = s >> 1;
		}

		private int echo_output(int ch)
		{
			int _out = (short)((m.t_main_out[ch] * (sbyte)m.regs[(int)GlobalReg.mvoll + ch * 0x10]) >> 7) +
				(short)((m.t_echo_in[ch] * (sbyte)m.regs[(int)GlobalReg.evoll + ch * 0x10]) >> 7);
			Clamp16(ref _out);
			return _out;
		}

		private void echo_write(int ch)
		{
			if (!Convert.ToBoolean(m.t_echo_enabled & 0x20))
			{
				Array.Copy(BitConverter.GetBytes((ushort)m.t_echo_out[ch]), 0, m.ram, m.t_echo_ptr + ch * 2, 2);
			}
			m.t_echo_out[ch] = 0;
		}

		private int CalcFir(int i, int ch)
		{
			return ((m.echo_hist_pos.Array[m.echo_hist_pos.Offset + i + 1][ch] * (sbyte)m.regs[(int)GlobalReg.fir + i * 0x10]) >> 6);
		}

		private void echo_22()
		{ 	// History
			if (m.echo_hist_pos.Offset + 1 >= echo_hist_size)
			{
				m.echo_hist_pos = new ArraySegment<int[]>(m.echo_hist, 0, m.echo_hist.Length);
			}
			else
			{
				m.echo_hist_pos = new ArraySegment<int[]>(m.echo_hist_pos.Array, m.echo_hist_pos.Offset + 1, m.echo_hist_pos.Count - 1);
			}

			m.t_echo_ptr = (m.t_esa * 0x100 + m.echo_offset) & 0xFFFF;
			echo_read(0);

			// FIR (using l and r temporaries below helps compiler optimize)
			int l = CalcFir(0, 0);
			int r = CalcFir(0, 1);

			m.t_echo_in[0] = l;
			m.t_echo_in[1] = r;
		}

		private void echo_23()
		{
			int l = CalcFir(1, 0) + CalcFir(2, 0);
			int r = CalcFir(1, 1) + CalcFir(2, 1);

			m.t_echo_in[0] += l;
			m.t_echo_in[1] += r;

			echo_read(1);
		}

		private void echo_24()
		{
			int l = CalcFir(3, 0) + CalcFir(4, 0) + CalcFir(5, 0);
			int r = CalcFir(3, 1) + CalcFir(4, 1) + CalcFir(5, 1);

			m.t_echo_in[0] += l;
			m.t_echo_in[1] += r;
		}

		private void echo_25()
		{
			int l = m.t_echo_in[0] + CalcFir(6, 0);
			int r = m.t_echo_in[1] + CalcFir(6, 1);

			l = (short)l;
			r = (short)r;

			l += (short)CalcFir(7, 0);
			r += (short)CalcFir(7, 1);

			Clamp16(ref l);
			Clamp16(ref r);

			m.t_echo_in[0] = l & ~1;
			m.t_echo_in[1] = r & ~1;
		}

		private void echo_26()
		{ 	// Left output volumes
			// (save sample for next clock so we can output both together)
			m.t_main_out[0] = echo_output(0);

			// Echo feedback
			int l = m.t_echo_out[0] + (short)((m.t_echo_in[0] * (sbyte)m.regs[(int)GlobalReg.efb]) >> 7);
			int r = m.t_echo_out[1] + (short)((m.t_echo_in[1] * (sbyte)m.regs[(int)GlobalReg.efb]) >> 7);

			Clamp16(ref l);
			Clamp16(ref r);

			m.t_echo_out[0] = l & ~1;
			m.t_echo_out[1] = r & ~1;
		}

		private void WRITE_SAMPLES(int l, int r, ref ArraySegment<short> _out)
		{
			_out.Array[_out.Offset + 0] = (short)l;
			_out.Array[_out.Offset + 1] = (short)r;
			_out = new ArraySegment<short>(_out.Array, _out.Offset + 2, _out.Count - 2);
			if (_out.Offset >= m._out.Array.Length)
			{
				Debug.Assert(_out.Offset == m._out.Array.Length);
				//TODO: fix this assert
				//Debug.Assert(m._out.Array.Length != m.extra[extra_size] ||
				//    (m.extra <= m.out_begin && m.extra < m.extra[extra_size]));
				_out = new ArraySegment<short>(m.extra, 0, m.extra.Length);
				//TODO: determine what's really happening here in bsnes code
				m._out = new ArraySegment<short>(m.extra, extra_size, m.extra.Length - extra_size);
			}
		}

		private void echo_27()
		{	// Output
			int l = m.t_main_out[0];
			int r = echo_output(1);
			m.t_main_out[0] = 0;
			m.t_main_out[1] = 0;

			// global muting isn't this simple (turns DAC on and off
			// or something, causing small ~37-sample pulse when first muted)
			if (Convert.ToBoolean(m.regs[(int)GlobalReg.flg] & 0x40))
			{
				l = 0;
				r = 0;
			}

			// Output sample to DAC
#if SPC_DSP_OUT_HOOK
		SPC_DSP_OUT_HOOK( l, r );
#else
			var _out = m._out;
			WRITE_SAMPLES(l, r, ref _out);
			m._out = _out;
#endif
		}

		private void echo_28()
		{
			m.t_echo_enabled = m.regs[(int)GlobalReg.flg];
		}

		private void echo_29()
		{
			m.t_esa = m.regs[(int)GlobalReg.esa];

			if (!Convert.ToBoolean(m.echo_offset))
			{
				m.echo_length = (m.regs[(int)GlobalReg.edl] & 0x0F) * 0x800;
			}

			m.echo_offset += 4;
			if (m.echo_offset >= m.echo_length)
			{
				m.echo_offset = 0;
			}

			// Write left echo
			echo_write(0);

			m.t_echo_enabled = m.regs[(int)GlobalReg.flg];
		}

		private void echo_30()
		{ 	// Write right echo
			echo_write(1);
		}

		private void soft_reset_common()
		{
			Debug.Assert(!ReferenceEquals(m.ram, null)); // init() must have been called already

			m.noise = 0x4000;
			m.echo_hist_pos = new ArraySegment<int[]>(m.echo_hist, 0, m.echo_hist.Length);
			m.every_other_sample = 1;
			m.echo_offset = 0;
			m.phase = 0;

			init_counter();
		}
	}
}
#endif
