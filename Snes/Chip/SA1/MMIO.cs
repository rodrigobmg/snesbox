
namespace Snes
{
    partial class SA1
    {
        public class MMIO
        {
            //$2200 CCNT
            bool sa1_irq;
            bool sa1_rdyb;
            bool sa1_resb;
            bool sa1_nmi;
            byte smeg;

            //$2201 SIE
            bool cpu_irqen;
            bool chdma_irqen;

            //$2202 SIC
            bool cpu_irqcl;
            bool chdma_irqcl;

            //$2203,$2204 CRV
            ushort crv;

            //$2205,$2206 CNV
            ushort cnv;

            //$2207,$2208 CIV
            ushort civ;

            //$2209 SCNT
            bool cpu_irq;
            bool cpu_ivsw;
            bool cpu_nvsw;
            byte cmeg;

            //$220a CIE
            bool sa1_irqen;
            bool timer_irqen;
            bool dma_irqen;
            bool sa1_nmien;

            //$220b CIC
            bool sa1_irqcl;
            bool timer_irqcl;
            bool dma_irqcl;
            bool sa1_nmicl;

            //$220c,$220d SNV
            ushort snv;

            //$220e,$220f SIV
            ushort siv;

            //$2210 TMC
            bool hvselb;
            bool ven;
            bool hen;

            //$2212,$2213
            ushort hcnt;

            //$2214,$2215
            ushort vcnt;

            //$2220 CXB
            bool cbmode;
            byte cb;

            //$2221 DXB
            bool dbmode;
            byte db;

            //$2222 EXB
            bool ebmode;
            byte eb;

            //$2223 FXB
            bool fbmode;
            byte fb;

            //$2224 BMAPS
            byte sbm;

            //$2225 BMAP
            bool sw46;
            byte cbm;

            //$2226 SBWE
            bool swen;

            //$2227 CBWE
            bool cwen;

            //$2228 BWPA
            byte bwp;

            //$2229 SIWP
            byte siwp;

            //$222a CIWP
            byte ciwp;

            //$2230 DCNT
            bool dmaen;
            bool dprio;
            bool cden;
            bool cdsel;
            bool dd;
            byte sd;

            //$2231 CDMA
            bool chdend;
            byte dmasize;
            byte dmacb;

            //$2232-$2234 SDA
            uint dsa;

            //$2235-$2237 DDA
            uint dda;

            //$2238,$2239 DTC
            ushort dtc;

            //$223f BBF
            bool bbf;

            //$2240-224f BRF
            byte[] brf = new byte[16];

            //$2250 MCNT
            bool acm;
            bool md;

            //$2251,$2252 MA
            ushort ma;

            //$2253,$2254 MB
            ushort mb;

            //$2258 VBD
            bool hl;
            byte vb;

            //$2259-$225b VDA
            uint va;
            byte vbit;

            //$2300 SFR
            bool cpu_irqfl;
            bool chdma_irqfl;

            //$2301 CFR
            bool sa1_irqfl;
            bool timer_irqfl;
            bool dma_irqfl;
            bool sa1_nmifl;

            //$2302,$2303 HCR
            ushort hcr;

            //$2304,$2305 VCR
            ushort vcr;

            //$2306-230a MR
            ulong mr;

            //$230b OF
            bool overflow;
        }
    }
}
