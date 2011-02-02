
namespace Snes
{
    partial class SA1
    {
        public class DMA
        {
            enum CDEN { DmaNormal = 0, DmaCharConversion = 1 }
            enum SD { SourceROM = 0, SourceBWRAM = 1, SourceIRAM = 2 }
            enum DD { DestIRAM = 0, DestBWRAM = 1 }
            uint line;
        }
    }
}
