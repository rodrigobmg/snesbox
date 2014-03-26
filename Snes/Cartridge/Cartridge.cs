using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Nall;

namespace Snes
{
    partial class Cartridge
    {
        public static Cartridge cartridge = new Cartridge();

        public enum Mode : uint { Normal, BsxSlotted, Bsx, SufamiTurbo, SuperGameBoy }
        public enum Region : uint { NTSC, PAL }
        public enum SuperGameBoyVersion : uint { Version1, Version2 }

        //assigned externally to point to file-system datafiles (msu1 and serial)
        //example: "/path/to/filename.sfc" would set this to "/path/to/filename"
        public string basename { get; set; }

        public bool loaded { get; private set; }
        public uint crc32 { get; private set; }
        public string sha256 { get; private set; }

        public Mode mode { get; private set; }
        public Region region { get; private set; }
        public uint ram_size { get; private set; }
        public uint spc7110_data_rom_offset { get; private set; }
        public SuperGameBoyVersion supergameboy_version { get; private set; }
        public uint supergameboy_ram_size { get; private set; }
        public uint supergameboy_rtc_size { get; private set; }

        public bool has_bsx_slot { get; private set; }
        public bool has_superfx { get; private set; }
        public bool has_sa1 { get; private set; }
        public bool has_srtc { get; private set; }
        public bool has_sdd1 { get; private set; }
        public bool has_spc7110 { get; private set; }
        public bool has_spc7110rtc { get; private set; }
        public bool has_cx4 { get; private set; }
        public bool has_dsp1 { get; private set; }
        public bool has_dsp2 { get; private set; }
        public bool has_dsp3 { get; private set; }
        public bool has_dsp4 { get; private set; }
        public bool has_obc1 { get; private set; }
        public bool has_st0010 { get; private set; }
        public bool has_st0011 { get; private set; }
        public bool has_st0018 { get; private set; }
        public bool has_msu1 { get; private set; }
        public bool has_serial { get; private set; }

        public Collection<Mapping> mapping = new Collection<Mapping>();

        public void load(Mode cartridge_mode, string[] xml_list)
        {
            mode = cartridge_mode;
            region = Region.NTSC;
            ram_size = 0;
            spc7110_data_rom_offset = 0x100000;
            supergameboy_version = SuperGameBoyVersion.Version1;
            supergameboy_ram_size = 0;
            supergameboy_rtc_size = 0;

            has_bsx_slot = false;
            has_superfx = false;
            has_sa1 = false;
            has_srtc = false;
            has_sdd1 = false;
            has_spc7110 = false;
            has_spc7110rtc = false;
            has_cx4 = false;
            has_dsp1 = false;
            has_dsp2 = false;
            has_dsp3 = false;
            has_dsp4 = false;
            has_obc1 = false;
            has_st0010 = false;
            has_st0011 = false;
            has_st0018 = false;
            has_msu1 = false;
            has_serial = false;

            parse_xml(xml_list);

            if (ram_size > 0)
            {
                MappedRAM.cartram.map(Enumerable.Repeat<byte>(0xff, (int)ram_size).ToArray(), ram_size);
            }

            if (has_srtc || has_spc7110rtc)
            {
                MappedRAM.cartram.map(Enumerable.Repeat<byte>(0xff, 20).ToArray(), 20);
            }

            if (mode == Mode.Bsx)
            {
                MappedRAM.bsxram.map(Enumerable.Repeat<byte>(0xff, 32 * 1024).ToArray(), 32 * 1024);
                MappedRAM.bsxram.map(Enumerable.Repeat<byte>(0xff, 512 * 1024).ToArray(), 512 * 1024);
            }

            if (mode == Mode.SufamiTurbo)
            {
                if (!ReferenceEquals(MappedRAM.stArom.data(), null))
                {
                    MappedRAM.stAram.map(Enumerable.Repeat<byte>(0xff, 128 * 1024).ToArray(), 128 * 1024);
                }
                if (!ReferenceEquals(MappedRAM.stBrom.data(), null))
                {
                    MappedRAM.stBram.map(Enumerable.Repeat<byte>(0xff, 128 * 1024).ToArray(), 128 * 1024);
                }
            }

            if (mode == Mode.SuperGameBoy)
            {
                if (!ReferenceEquals(MappedRAM.gbrom.data(), null))
                {
                    if (Convert.ToBoolean(supergameboy_ram_size))
                    {
                        MappedRAM.gbram.map(Enumerable.Repeat<byte>(0xff, (int)supergameboy_ram_size).ToArray(), supergameboy_ram_size);
                    }
                    if (Convert.ToBoolean(supergameboy_rtc_size))
                    {
                        MappedRAM.gbrtc.map(Enumerable.Repeat<byte>(0x00, (int)supergameboy_rtc_size).ToArray(), supergameboy_rtc_size);
                    }
                }
            }

            MappedRAM.cartrom.write_protect(true);
            MappedRAM.cartram.write_protect(false);
            MappedRAM.cartrtc.write_protect(false);
            MappedRAM.bsxflash.write_protect(true);
            MappedRAM.bsxram.write_protect(false);
            MappedRAM.bsxpram.write_protect(false);
            MappedRAM.stArom.write_protect(true);
            MappedRAM.stAram.write_protect(false);
            MappedRAM.stBrom.write_protect(true);
            MappedRAM.stBram.write_protect(false);
            MappedRAM.gbrom.write_protect(true);
            MappedRAM.gbram.write_protect(false);
            MappedRAM.gbrtc.write_protect(false);

            uint checksum = Bit.ToUint32(~0);
            foreach (var n in MappedRAM.cartrom.data())
            {
                checksum = CRC32.adjust(checksum, n);
            }
            if (MappedRAM.bsxflash.size() != 0 && MappedRAM.bsxflash.size() != Bit.ToUint32(~0))
            {
                foreach (var n in MappedRAM.bsxflash.data())
                {
                    checksum = CRC32.adjust(checksum, n);
                }
            }
            if (MappedRAM.stArom.size() != 0 && MappedRAM.stArom.size() != Bit.ToUint32(~0))
            {
                foreach (var n in MappedRAM.stArom.data())
                {
                    checksum = CRC32.adjust(checksum, n);
                }
            }
            if (MappedRAM.stBrom.size() != 0 && MappedRAM.stBrom.size() != Bit.ToUint32(~0))
            {
                foreach (var n in MappedRAM.stBrom.data())
                {
                    checksum = CRC32.adjust(checksum, n);
                }
            }
            if (MappedRAM.gbrom.size() != 0 && MappedRAM.gbrom.size() != Bit.ToUint32(~0))
            {
                foreach (var n in MappedRAM.gbrom.data())
                {
                    checksum = CRC32.adjust(checksum, n);
                }
            }
            crc32 = ~checksum;

            //// TODO: verify hash
            //var sha = new SHA256Managed();
            //var shahash = sha.ComputeHash(MappedRAM.cartrom.data());

            //string hash = string.Empty;
            //foreach (var n in shahash)
            //{
            //    hash += n.ToString("X2");
            //}
            //sha256 = hash;

            Bus.bus.load_cart();
            System.system.serialize_init();
            loaded = true;
        }

        public void unload()
        {
            MappedRAM.cartrom.reset();
            MappedRAM.cartram.reset();
            MappedRAM.cartrtc.reset();
            MappedRAM.bsxflash.reset();
            MappedRAM.bsxram.reset();
            MappedRAM.bsxpram.reset();
            MappedRAM.stArom.reset();
            MappedRAM.stAram.reset();
            MappedRAM.stBrom.reset();
            MappedRAM.stBram.reset();
            MappedRAM.gbrom.reset();
            MappedRAM.gbram.reset();
            MappedRAM.gbrtc.reset();

            if (loaded == false)
            {
                return;
            }
            Bus.bus.unload_cart();
            loaded = false;
        }

        public void serialize(Serializer s)
        {
            if (MappedRAM.cartram.size() != 0 && MappedRAM.cartram.size() != Bit.ToUint32(~0))
            {
                s.array(MappedRAM.cartram.data(), MappedRAM.cartram.size(), "memory::cartram.data()");
            }

            if (MappedRAM.cartrtc.size() != 0 && MappedRAM.cartrtc.size() != Bit.ToUint32(~0))
            {
                s.array(MappedRAM.cartrtc.data(), MappedRAM.cartrtc.size(), "memory::cartrtc.data()");
            }

            if (MappedRAM.bsxram.size() != 0 && MappedRAM.bsxram.size() != Bit.ToUint32(~0))
            {
                s.array(MappedRAM.bsxram.data(), MappedRAM.bsxram.size(), "memory::bsxram.data()");
            }

            if (MappedRAM.bsxpram.size() != 0 && MappedRAM.bsxpram.size() != Bit.ToUint32(~0))
            {
                s.array(MappedRAM.bsxpram.data(), MappedRAM.bsxpram.size(), "memory::bsxpram.data()");
            }

            if (MappedRAM.stAram.size() != 0 && MappedRAM.stAram.size() != Bit.ToUint32(~0))
            {
                s.array(MappedRAM.stAram.data(), MappedRAM.stAram.size(), "memory::stAram.data()");
            }

            if (MappedRAM.stBram.size() != 0 && MappedRAM.stBram.size() != Bit.ToUint32(~0))
            {
                s.array(MappedRAM.stBram.data(), MappedRAM.stBram.size(), "memory::stBram.data()");
            }

            if (MappedRAM.gbram.size() != 0 && MappedRAM.gbram.size() != Bit.ToUint32(~0))
            {
                s.array(MappedRAM.gbram.data(), MappedRAM.gbram.size(), "memory::gbram.data()");
            }

            if (MappedRAM.gbrtc.size() != 0 && MappedRAM.gbrtc.size() != Bit.ToUint32(~0))
            {
                s.array(MappedRAM.gbrtc.data(), MappedRAM.gbrtc.size(), "MappedRAM.gbrtc.data()");
            }
        }

        public Cartridge()
        {
            loaded = false;
            unload();
        }

        private void parse_xml(string[] list)
        {
            mapping.Clear();
            parse_xml_cartridge(list[0]);

            if (mode == Mode.BsxSlotted)
            {
                parse_xml_bsx(list[1]);
            }
            else if (mode == Mode.Bsx)
            {
                parse_xml_bsx(list[1]);
            }
            else if (mode == Mode.SufamiTurbo)
            {
                parse_xml_sufami_turbo(list[1], Convert.ToBoolean(0));
                parse_xml_sufami_turbo(list[2], Convert.ToBoolean(1));
            }
            else if (mode == Mode.SuperGameBoy)
            {
                parse_xml_gameboy(list[1]);
            }
        }

        private void parse_xml_cartridge(string data)
        {
            XDocument document = XDocument.Parse(data);
            if (document.Root.IsEmpty)
            {
                return;
            }

            if (document.Elements("cartridge").Any())
            {
                if (document.Element("cartridge").Attributes("region").Any())
                {
                    region = (document.Element("cartridge").Attribute("region").Value == "NTSC") ? Region.NTSC : Region.PAL;
                }
            }

            foreach (var node in document.Descendants("rom"))
            {
                xml_parse_rom(node);
            }
            foreach (var node in document.Descendants("ram"))
            {
                xml_parse_ram(node);
            }
            foreach (var node in document.Descendants("superfx"))
            {
                xml_parse_superfx(node);
            }
            foreach (var node in document.Descendants("sa1"))
            {
                xml_parse_sa1(node);
            }
            foreach (var node in document.Descendants("bsx"))
            {
                xml_parse_bsx(node);
            }
            foreach (var node in document.Descendants("sufamiturbo"))
            {
                xml_parse_sufamiturbo(node);
            }
            foreach (var node in document.Descendants("supergameboy"))
            {
                xml_parse_supergameboy(node);
            }
            foreach (var node in document.Descendants("srtc"))
            {
                xml_parse_srtc(node);
            }
            foreach (var node in document.Descendants("sdd1"))
            {
                xml_parse_sdd1(node);
            }
            foreach (var node in document.Descendants("spc7110"))
            {
                xml_parse_spc7110(node);
            }
            foreach (var node in document.Descendants("cx4"))
            {
                xml_parse_cx4(node);
            }
            foreach (var node in document.Descendants("necdsp"))
            {
                xml_parse_necdsp(node);
            }
            foreach (var node in document.Descendants("obc1"))
            {
                xml_parse_obc1(node);
            }
            foreach (var node in document.Descendants("setadsp"))
            {
                xml_parse_setadsp(node);
            }
            foreach (var node in document.Descendants("setarisc"))
            {
                xml_parse_setarisc(node);
            }
            foreach (var node in document.Descendants("msu1"))
            {
                xml_parse_msu1(node);
            }
            foreach (var node in document.Descendants("serial"))
            {
                xml_parse_serial(node);
            }
        }

        private void parse_xml_bsx(string data)
        {
        }

        private void parse_xml_sufami_turbo(string data, bool slot)
        {
        }

        private void parse_xml_gameboy(string data)
        {
            XDocument document = XDocument.Parse(data);
            if (document.Root.IsEmpty)
            {
                return;
            }

            if (document.Elements("cartridge").Any())
            {
                if (document.Element("cartridge").Attributes("rtc").Any())
                {
                    supergameboy_rtc_size = (document.Element("cartridge").Attribute("rtc").Value == "true") ? 4U : 0U;
                }
            }
            if (document.Elements("cartridge").Any())
            {
                if (document.Element("cartridge").Elements("ram").Any())
                {
                    if (document.Element("cartridge").Element("ram").Attributes("size").Any())
                    {
                        supergameboy_ram_size = Convert.ToUInt32(document.Element("cartridge").Element("ram").Attribute("size").Value, 16);
                    }
                }
            }
        }

        private void xml_parse_rom(XElement root)
        {
            foreach (var leaf in root.Elements("map"))
            {
                Mapping m = new Mapping(MappedRAM.cartrom);
                if (leaf.Attributes("address").Any())
                {
                    xml_parse_address(m, leaf.Attribute("address").Value);
                }
                if (leaf.Attributes("mode").Any())
                {
                    xml_parse_mode(m, leaf.Attribute("mode").Value);
                }
                if (leaf.Attributes("offset").Any())
                {
                    m.offset = Convert.ToUInt32(leaf.Attribute("offset").Value, 16);
                }
                if (leaf.Attributes("size").Any())
                {
                    m.size = Convert.ToUInt32(leaf.Attribute("size").Value, 16);
                }
                mapping.Add(m);
            }
        }

        private void xml_parse_ram(XElement root)
        {
            if (root.Attributes("size").Any())
            {
                ram_size = Convert.ToUInt32(root.Attribute("size").Value, 16);
            }

            foreach (var leaf in root.Elements("map"))
            {
                Mapping m = new Mapping(MappedRAM.cartram);
                if (leaf.Attributes("address").Any())
                {
                    xml_parse_address(m, leaf.Attribute("address").Value);
                }
                if (leaf.Attributes("mode").Any())
                {
                    xml_parse_mode(m, leaf.Attribute("mode").Value);
                }
                if (leaf.Attributes("offset").Any())
                {
                    m.offset = Convert.ToUInt32(leaf.Attribute("offset").Value, 16);
                }
                if (leaf.Attributes("size").Any())
                {
                    m.size = Convert.ToUInt32(leaf.Attribute("size").Value, 16);
                }
                mapping.Add(m);
            }
        }

        private void xml_parse_superfx(XElement root)
        {
            has_superfx = true;

            foreach (var node in root.Elements("rom"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(SuperFXCPUROM.fxrom);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    if (leaf.Attributes("mode").Any())
                    {
                        xml_parse_mode(m, leaf.Attribute("mode").Value);
                    }
                    if (leaf.Attributes("offset").Any())
                    {
                        m.offset = Convert.ToUInt32(leaf.Attribute("offset").Value, 16);
                    }
                    if (leaf.Attributes("size").Any())
                    {
                        m.size = Convert.ToUInt32(leaf.Attribute("size").Value, 16);
                    }
                    mapping.Add(m);
                }
            }

            foreach (var node in root.Elements("ram"))
            {
                if (node.Attributes("size").Any())
                {
                    ram_size = Convert.ToUInt32(node.Attribute("size").Value, 16);
                }

                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(SuperFXCPURAM.fxram);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    if (leaf.Attributes("mode").Any())
                    {
                        xml_parse_mode(m, leaf.Attribute("mode").Value);
                    }
                    if (leaf.Attributes("offset").Any())
                    {
                        m.offset = Convert.ToUInt32(leaf.Attribute("offset").Value, 16);
                    }
                    if (leaf.Attributes("size").Any())
                    {
                        m.size = Convert.ToUInt32(leaf.Attribute("size").Value, 16);
                    }
                    mapping.Add(m);
                }
            }

            foreach (var node in root.Elements("mmio"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(SuperFX.superfx);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }
        }

        private void xml_parse_sa1(XElement root)
        {
            has_sa1 = true;

            foreach (var node in root.Elements("rom"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(VSPROM.vsprom);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    if (leaf.Attributes("mode").Any())
                    {
                        xml_parse_mode(m, leaf.Attribute("mode").Value);
                    }
                    if (leaf.Attributes("offset").Any())
                    {
                        m.offset = Convert.ToUInt32(leaf.Attribute("offset").Value, 16);
                    }
                    if (leaf.Attributes("size").Any())
                    {
                        m.size = Convert.ToUInt32(leaf.Attribute("size").Value, 16);
                    }
                    mapping.Add(m);
                }
            }

            foreach (var node in root.Elements("iram"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(CPUIRAM.cpuiram);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    if (leaf.Attributes("mode").Any())
                    {
                        xml_parse_mode(m, leaf.Attribute("mode").Value);
                    }
                    if (leaf.Attributes("offset").Any())
                    {
                        m.offset = Convert.ToUInt32(leaf.Attribute("offset").Value, 16);
                    }
                    if (leaf.Attributes("size").Any())
                    {
                        m.size = Convert.ToUInt32(leaf.Attribute("size").Value, 16);
                    }
                    mapping.Add(m);
                }
            }

            foreach (var node in root.Elements("bwram"))
            {
                if (node.Attributes("size").Any())
                {
                    ram_size = Convert.ToUInt32(node.Attribute("size").Value, 16);
                }

                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(CC1BWRAM.cc1bwram);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    if (leaf.Attributes("mode").Any())
                    {
                        xml_parse_mode(m, leaf.Attribute("mode").Value);
                    }
                    if (leaf.Attributes("offset").Any())
                    {
                        m.offset = Convert.ToUInt32(leaf.Attribute("offset").Value, 16);
                    }
                    if (leaf.Attributes("size").Any())
                    {
                        m.size = Convert.ToUInt32(leaf.Attribute("size").Value, 16);
                    }
                    mapping.Add(m);
                }
            }

            foreach (var node in root.Elements("mmio"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(SA1.sa1);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }
        }

        private void xml_parse_bsx(XElement root)
        {
            if (mode != Mode.BsxSlotted && mode != Mode.Bsx)
            {
                return;
            }

            foreach (var node in root.Elements("slot"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(MappedRAM.bsxflash);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    if (leaf.Attributes("mode").Any())
                    {
                        xml_parse_mode(m, leaf.Attribute("mode").Value);
                    }
                    if (leaf.Attributes("offset").Any())
                    {
                        m.offset = Convert.ToUInt32(leaf.Attribute("offset").Value, 16);
                    }
                    if (leaf.Attributes("size").Any())
                    {
                        m.size = Convert.ToUInt32(leaf.Attribute("size").Value, 16);
                    }
                    mapping.Add(m);
                }
            }

            foreach (var node in root.Elements("mmio"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(BSXCart.bsxcart);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }
        }

        private void xml_parse_sufamiturbo(XElement root)
        {
            if (mode != Mode.SufamiTurbo)
            {
                return;
            }

            foreach (var node in root.Elements("slot"))
            {
                bool slotid = Convert.ToBoolean(0);
                if (node.Attributes("id").Any())
                {
                    if (node.Attribute("id").Value == "A")
                    {
                        slotid = Convert.ToBoolean(0);
                    }
                    if (node.Attribute("id").Value == "B")
                    {
                        slotid = Convert.ToBoolean(1);
                    }
                }

                foreach (var slot in root.Elements("rom"))
                {
                    foreach (var leaf in slot.Elements("map"))
                    {
                        Mapping m = new Mapping(slotid == Convert.ToBoolean(0) ? MappedRAM.stArom : MappedRAM.stBrom);
                        if (leaf.Attributes("address").Any())
                        {
                            xml_parse_address(m, leaf.Attribute("address").Value);
                        }
                        if (leaf.Attributes("mode").Any())
                        {
                            xml_parse_mode(m, leaf.Attribute("mode").Value);
                        }
                        if (leaf.Attributes("offset").Any())
                        {
                            m.offset = Convert.ToUInt32(leaf.Attribute("offset").Value, 16);
                        }
                        if (leaf.Attributes("size").Any())
                        {
                            m.size = Convert.ToUInt32(leaf.Attribute("size").Value, 16);
                        }
                        if (m.memory.size() > 0)
                        {
                            mapping.Add(m);
                        }
                    }
                }

                foreach (var slot in root.Elements("ram"))
                {
                    foreach (var leaf in slot.Elements("map"))
                    {
                        Mapping m = new Mapping(slotid == Convert.ToBoolean(0) ? MappedRAM.stAram : MappedRAM.stBram);
                        if (leaf.Attributes("address").Any())
                        {
                            xml_parse_address(m, leaf.Attribute("address").Value);
                        }
                        if (leaf.Attributes("mode").Any())
                        {
                            xml_parse_mode(m, leaf.Attribute("mode").Value);
                        }
                        if (leaf.Attributes("offset").Any())
                        {
                            m.offset = Convert.ToUInt32(leaf.Attribute("offset").Value, 16);
                        }
                        if (leaf.Attributes("size").Any())
                        {
                            m.size = Convert.ToUInt32(leaf.Attribute("size").Value, 16);
                        }
                        if (m.memory.size() > 0)
                        {
                            mapping.Add(m);
                        }
                    }
                }
            }
        }

        private void xml_parse_supergameboy(XElement root)
        {
            if (mode != Mode.SuperGameBoy)
            {
                return;
            }

            if (root.Attributes("revision").Any())
            {
                if (root.Attribute("revision").Value == "1")
                {
                    supergameboy_version = SuperGameBoyVersion.Version1;
                }
                if (root.Attribute("revision").Value == "2")
                {
                    supergameboy_version = SuperGameBoyVersion.Version2;
                }
            }

            foreach (var node in root.Elements("mmio"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping((Memory)SuperGameBoy.supergameboy);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }
        }

        private void xml_parse_srtc(XElement root)
        {
            has_srtc = true;

            foreach (var node in root.Elements("mmio"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(SRTC.srtc);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }
        }

        private void xml_parse_sdd1(XElement root)
        {
            has_sdd1 = true;

            foreach (var node in root.Elements("mcu"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping((Memory)SDD1.sdd1);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }

            foreach (var node in root.Elements("mmio"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping((IMMIO)SDD1.sdd1);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }
        }

        private void xml_parse_spc7110(XElement root)
        {
            has_spc7110 = true;

            foreach (var node in root.Elements("dcu"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(SPC7110DCU.spc7110dcu);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }

            foreach (var node in root.Elements("mcu"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(SPC7110MCU.spc7110mcu);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    if (leaf.Attributes("offset").Any())
                    {
                        spc7110_data_rom_offset = Convert.ToUInt32(leaf.Attribute("offset").Value, 16);
                    }
                    mapping.Add(m);
                }
            }

            foreach (var node in root.Elements("mmio"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(SPC7110.spc7110);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }

            foreach (var node in root.Elements("ram"))
            {
                if (node.Attributes("size").Any())
                {
                    ram_size = Convert.ToUInt32(node.Attribute("size").Value, 16);
                }

                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(SPC7110RAM.spc7110ram);

                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    if (leaf.Attributes("mode").Any())
                    {
                        xml_parse_mode(m, leaf.Attribute("mode").Value);
                    }
                    if (leaf.Attributes("offset").Any())
                    {
                        m.offset = Convert.ToUInt32(leaf.Attribute("offset").Value, 16);
                    }
                    if (leaf.Attributes("size").Any())
                    {
                        m.size = Convert.ToUInt32(leaf.Attribute("size").Value, 16);
                    }
                    mapping.Add(m);
                }
            }

            foreach (var node in root.Elements("rtc"))
            {
                has_spc7110rtc = true;

                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(SPC7110.spc7110);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }
        }

        private void xml_parse_cx4(XElement root)
        {
            has_cx4 = true;

            foreach (var node in root.Elements("mmio"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(CX4.cx4);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }
        }

        private void xml_parse_necdsp(XElement root)
        {
            uint program = 0;

            if (root.Attributes("program").Any())
            {
                if (root.Attribute("program").Value == "DSP-1" || root.Attribute("program").Value == "DSP-1A" || root.Attribute("program").Value == "DSP-1B")
                {
                    program = 1;
                    has_dsp1 = true;
                }
                else if (root.Attribute("program").Value == "DSP-2")
                {
                    program = 2;
                    has_dsp2 = true;
                }
                else if (root.Attribute("program").Value == "DSP-3")
                {
                    program = 3;
                    has_dsp3 = true;
                }
                else if (root.Attribute("program").Value == "DSP-4")
                {
                    program = 4;
                    has_dsp4 = true;
                }
            }

            Memory[] dr = new Memory[5] { null, DSP1DR.dsp1dr, DSP2DR.dsp2dr, DSP3.dsp3, DSP4.dsp4 };
            Memory[] sr = new Memory[5] { null, DSP1SR.dsp1sr, DSP2SR.dsp2sr, DSP3.dsp3, DSP4.dsp4 };

            foreach (var node in root.Elements("dr"))
            {
                if (!ReferenceEquals(dr[program], null))
                {
                    foreach (var leaf in node.Elements("map"))
                    {
                        Mapping m = new Mapping(dr[program]);
                        if (leaf.Attributes("address").Any())
                        {
                            xml_parse_address(m, leaf.Attribute("address").Value);
                        }
                        mapping.Add(m);
                    }
                }
            }

            foreach (var node in root.Elements("sr"))
            {
                if (!ReferenceEquals(sr[program], null))
                {
                    foreach (var leaf in node.Elements("map"))
                    {
                        Mapping m = new Mapping(sr[program]);
                        if (leaf.Attributes("address").Any())
                        {
                            xml_parse_address(m, leaf.Attribute("address").Value);
                        }
                        mapping.Add(m);
                    }
                }
            }
        }

        private void xml_parse_obc1(XElement root)
        {
            has_obc1 = true;

            foreach (var node in root.Elements("mmio"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(OBC1.obc1);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }
        }

        private void xml_parse_setadsp(XElement root)
        {
            uint program = 0;

            if (root.Attributes("program").Any())
            {
                var content = root.Attribute("program").Value;
                if (content == "ST-0010")
                {
                    program = 1;
                    has_st0010 = true;
                }
                else if (content == "ST-0011")
                {
                    program = 2;
                    has_st0011 = true;
                }
            }

            Memory[] map = new Memory[3] { null, ST0010.st0010, null };

            foreach (var node in root.Elements("mmio"))
            {
                foreach (var leaf in node.Elements("map"))
                {
                    Mapping m = new Mapping(map[program]);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }
        }

        private void xml_parse_setarisc(XElement root)
        {
            uint program = 0;

            if (root.Attributes("program").Any())
            {
                if (root.Attribute("program").Value == "ST-0018")
                {
                    program = 1;
                    has_st0018 = true;
                }
            }

            IMMIO[] map = new IMMIO[2] { null, ST0018.st0018 };

            foreach (var node in root.Elements("mmio"))
            {
                if (!ReferenceEquals(map[program], null))
                {
                    foreach (var leaf in root.Elements("map"))
                    {
                        Mapping m = new Mapping(map[program]);
                        if (leaf.Attributes("address").Any())
                        {
                            xml_parse_address(m, leaf.Attribute("address").Value);
                        }
                        mapping.Add(m);
                    }
                }
            }
        }

        private void xml_parse_msu1(XElement root)
        {
            has_msu1 = true;

            foreach (var node in root.Elements("mmio"))
            {
                foreach (var leaf in root.Elements("map"))
                {
                    Mapping m = new Mapping(MSU1.msu1);
                    if (leaf.Attributes("address").Any())
                    {
                        xml_parse_address(m, leaf.Attribute("address").Value);
                    }
                    mapping.Add(m);
                }
            }
        }

        private void xml_parse_serial(XElement root)
        {
            has_serial = true;
        }

        private void xml_parse_address(Mapping m, string data)
        {
            var part = data.Split(new char[] { ':' });

            if (part.Length != 2)
            {
                return;
            }

            var subpart = part[0].Split(new char[] { '-' });
            if (subpart.Length == 1)
            {
                m.banklo = Convert.ToUInt32(subpart[0], 16);
                m.bankhi = m.banklo;
            }
            else if (subpart.Length == 2)
            {
                m.banklo = Convert.ToUInt32(subpart[0], 16);
                m.bankhi = Convert.ToUInt32(subpart[1], 16);
            }

            subpart = part[1].Split(new char[] { '-' });
            if (subpart.Length == 1)
            {
                m.addrlo = Convert.ToUInt32(subpart[0], 16);
                m.addrhi = m.addrlo;
            }
            else if (subpart.Length == 2)
            {
                m.addrlo = Convert.ToUInt32(subpart[0], 16);
                m.addrhi = Convert.ToUInt32(subpart[1], 16);
            }
        }

        private void xml_parse_mode(Mapping m, string data)
        {
            if (data == "direct")
            {
                m.mode = Bus.MapMode.Direct;
            }
            else if (data == "linear")
            {
                m.mode = Bus.MapMode.Linear;
            }
            else if (data == "shadow")
            {
                m.mode = Bus.MapMode.Shadow;
            }
        }
    }
}
