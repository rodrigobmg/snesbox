using System;
using System.Collections;
using Nall;

namespace Snes
{
    class SRTC : IMMIO
    {
        public static SRTC srtc = new SRTC();

        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public IEnumerable mmio_read(uint addr, Result result) { throw new NotImplementedException(); }
        public IEnumerable mmio_write(uint addr, byte data) { throw new NotImplementedException(); }

        public void serialize(Serializer s)
        {
            throw new NotImplementedException();
        }

        public SRTC() { /*throw new NotImplementedException();*/ }

        private static readonly uint[] months = new uint[12];
        private enum RtcMode { RtcReady, RtcCommand, RtcRead, RtcWrite }
        private uint rtc_mode;
        private int rtc_index;

        private void update_time() { throw new NotImplementedException(); }
        private uint weekday(uint year, uint month, uint day) { throw new NotImplementedException(); }
    }
}
