using System;

namespace Snes
{
    public class InputStateEventArgs : EventArgs
    {
        public Port Port { get; private set; }
        public Device Device { get; private set; }
        public uint Index { get; private set; }
        public uint Id { get; private set; }
        public short State { get; set; }

        public InputStateEventArgs(Port port, Device device, uint index, uint id)
        {
            Port = port;
            Device = device;
            Index = index;
            Id = id;
        }
    }
}
