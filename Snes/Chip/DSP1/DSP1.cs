using System;
using Nall;

namespace Snes
{
    public delegate void Callback(short[] input, short[] output);

    partial class DSP1
    {
        public static DSP1 dsp1 = new DSP1();

        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public void serialize(Serializer s)
        {
            throw new NotImplementedException();
        }

        // The DSP-1 status register has 16 bits, but only
        // the upper 8 bits can be accessed from an external device, so all these
        // positions are referred to the upper byte (bits D8 to D15)

        public enum SrFlags { DRC = 0x04, DRS = 0x10, RQM = 0x80 }

        // According to Overload's docs, these are the meanings of the flags:
        // DRC: The Data Register Control (DRC) bit specifies the data transfer length to and from the host CPU.
        //   0: Data transfer to and from the DSP-1 is 16 bits.
        //   1: Data transfer to and from the DSP-1 is 8 bits.
        // DRS: The Data Register Status (DRS) bit indicates the data transfer status in the case of transfering 16-bit data.
        //   0: Data transfer has terminated.
        //   1: Data transfer in progress.
        // RQM: The Request for Master (RQM) indicates that the DSP1 is requesting host CPU for data read/write.
        //   0: Internal Data Register Transfer.
        //   1: External Data Register Transfer.

        public DSP1() { /*throw new NotImplementedException();*/ }
        public byte getSr() { throw new NotImplementedException(); } // return the status register's high byte
        public byte getDr() { throw new NotImplementedException(); }
        public void setDr(byte iDr) { throw new NotImplementedException(); }

        private enum FsmMajorState { WAIT_COMMAND, READ_DATA, WRITE_DATA }
        private enum MaxDataAccesses { MAX_READS = 7, MAX_WRITES = 1024 }

        private readonly Command[] mCommandTable;
        private readonly short[] MaxAZS_Exp = new short[16];
        private readonly short[] SinTable;
        private readonly short[] MulTable;
        private readonly ushort[] DataRom;

        private SharedData shared;

        private byte mSr; // status register
        private int mSrLowByteAccess;
        private ushort mDr; // "internal" representation of the data register
        private uint mFsmMajorState; // current major state of the FSM
        private byte mCommand; // current command processed by the FSM
        private byte mDataCounter; // #ushort read/writes counter used by the FSM
        private short[] mReadBuffer = new short[(int)MaxDataAccesses.MAX_READS];
        private short[] mWriteBuffer = new short[(int)MaxDataAccesses.MAX_WRITES];
        private bool mFreeze; // need explanation?  ;)

        private void fsmStep(bool read, ref byte data) { throw new NotImplementedException(); } // FSM logic

        // commands
        private void memoryTest(short[] input, short[] output) { throw new NotImplementedException(); }
        private void memoryDump(short[] input, short[] output) { throw new NotImplementedException(); }
        private void memorySize(short[] input, short[] output) { throw new NotImplementedException(); }
        private void multiply(short[] input, short[] output) { throw new NotImplementedException(); }
        private void multiply2(short[] input, short[] output) { throw new NotImplementedException(); }
        private void inverse(short[] input, short[] output) { throw new NotImplementedException(); }
        private void triangle(short[] input, short[] output) { throw new NotImplementedException(); }
        private void radius(short[] input, short[] output) { throw new NotImplementedException(); }
        private void range(short[] input, short[] output) { throw new NotImplementedException(); }
        private void range2(short[] input, short[] output) { throw new NotImplementedException(); }
        private void distance(short[] input, short[] output) { throw new NotImplementedException(); }
        private void rotate(short[] input, short[] output) { throw new NotImplementedException(); }
        private void polar(short[] input, short[] output) { throw new NotImplementedException(); }
        private void attitudeA(short[] input, short[] output) { throw new NotImplementedException(); }
        private void attitudeB(short[] input, short[] output) { throw new NotImplementedException(); }
        private void attitudeC(short[] input, short[] output) { throw new NotImplementedException(); }
        private void objectiveA(short[] input, short[] output) { throw new NotImplementedException(); }
        private void objectiveB(short[] input, short[] output) { throw new NotImplementedException(); }
        private void objectiveC(short[] input, short[] output) { throw new NotImplementedException(); }
        private void subjectiveA(short[] input, short[] output) { throw new NotImplementedException(); }
        private void subjectiveB(short[] input, short[] output) { throw new NotImplementedException(); }
        private void subjectiveC(short[] input, short[] output) { throw new NotImplementedException(); }
        private void scalarA(short[] input, short[] output) { throw new NotImplementedException(); }
        private void scalarB(short[] input, short[] output) { throw new NotImplementedException(); }
        private void scalarC(short[] input, short[] output) { throw new NotImplementedException(); }
        private void gyrate(short[] input, short[] output) { throw new NotImplementedException(); }
        private void parameter(short[] input, short[] output) { throw new NotImplementedException(); }
        private void raster(short[] input, short[] output) { throw new NotImplementedException(); }
        private void target(short[] input, short[] output) { throw new NotImplementedException(); }
        private void project(short[] input, short[] output) { throw new NotImplementedException(); }

        // auxiliar functions
        private short sin(short Angle) { throw new NotImplementedException(); }
        private short cos(short Angle) { throw new NotImplementedException(); }
        private void inverse(short Coefficient, short Exponent, ref short iCoefficient, ref short iExponent) { throw new NotImplementedException(); }
        private short denormalizeAndClip(short C, short E) { throw new NotImplementedException(); }
        private void normalize(short m, ref short Coefficient, ref short Exponent) { throw new NotImplementedException(); }
        private void normalizeDouble(int Product, ref short Coefficient, ref short Exponent) { throw new NotImplementedException(); }
        private short shiftR(short C, short E) { throw new NotImplementedException(); }
    }
}
