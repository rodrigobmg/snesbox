using System;

namespace SuperGameBoy
{
    public static class SuperGameBoy
    {
        public static void sgb_rom(byte[] data, uint size) { throw new NotImplementedException(); }
        public static void sgb_ram(byte[] data, uint size) { throw new NotImplementedException(); }
        public static void sgb_rtc(byte[] data, uint size) { throw new NotImplementedException(); }
        public static bool sgb_init(bool version) { throw new NotImplementedException(); }
        public static void sgb_term() { throw new NotImplementedException(); }
        public static void sgb_power() { throw new NotImplementedException(); }
        public static void sgb_reset() { throw new NotImplementedException(); }
        public static void sgb_row(uint row) { throw new NotImplementedException(); }
        public static byte sgb_read(ushort addr) { throw new NotImplementedException(); }
        public static void sgb_write(ushort addr, byte data) { throw new NotImplementedException(); }
        public static uint sgb_run(uint[] samplebuffer, uint clocks) { throw new NotImplementedException(); }
        public static void sgb_save() { throw new NotImplementedException(); }
    }
}
