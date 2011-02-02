using System;
using System.Collections.ObjectModel;
using System.Linq;
using Snes;

namespace SnesBox
{
    class Snes
    {
        public static readonly int VersionMajor;
        public static readonly int VersionMinor;
        static Collection<uint> audio_buffer = new Collection<uint>();

        static Snes()
        {
            VersionMajor = (int)LibSnes.snes_library_revision_major();
            VersionMinor = (int)LibSnes.snes_library_revision_minor();
        }

        Cartridge _cartridge;
        public event VideoUpdatedEventHandler VideoUpdated;
        public event AudioUpdatedEventHandler AudioUpdated;

        public Snes()
        {
            _cartridge = null;

            LibSnes.snes_init();
            LibSnes.snes_audio_sample += new LibSnes.SnesAudioSample(LibSnes_snes_audio_sample);
            LibSnes.snes_video_refresh += new LibSnes.SnesVideoRefresh(LibSnes_snes_video_refresh);
        }

        void LibSnes_snes_video_refresh(ArraySegment<ushort> data, uint width, uint height)
        {
            VideoUpdated(this, new VideoUpdatedEventArgs(data, (int)width, (int)height));
            AudioUpdated(this, new AudioUpdatedEventArgs(audio_buffer.ToArray(), audio_buffer.Count));
            audio_buffer.Clear();
        }

        void LibSnes_snes_audio_sample(ushort left, ushort right)
        {
            audio_buffer.Add((uint)((right << 16) | left));
        }

        public Cartridge Cartridge
        {
            get
            {
                if (_cartridge != null)
                {
                    _cartridge.Refresh();
                }

                return _cartridge;
            }
        }

        public void SetControllerPortDevice(int port, LibSnes.SnesDevice device)
        {
            if (port < 1 || port > 2)
            {
                throw new ArgumentOutOfRangeException("port");
            }

            LibSnes.snes_set_controller_port_device(port == 2 ? true : false, (uint)device);
        }

        public void RunToFrame()
        {
            if (_cartridge == null)
            {
                throw new InvalidOperationException("No rom loaded.");
            }

            LibSnes.snes_run();
        }

        public void PowerCycle()
        {
            LibSnes.snes_power();
        }

        public void Reset()
        {
            LibSnes.snes_reset();
        }

        public void LoadCartridge(Cartridge cartridge)
        {
            if (_cartridge != null)
            {
                _cartridge.Refresh();
                LibSnes.snes_unload_cartridge();
            }

            _cartridge = cartridge;
            cartridge.Load(this);
        }
    }
}
