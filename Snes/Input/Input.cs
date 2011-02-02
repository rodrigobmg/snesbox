using System;
using Nall;

namespace Snes
{
    partial class Input
    {
        public static Input input = new Input();

        public enum Device : uint { None, Joypad, Multitap, Mouse, SuperScope, Justifier, Justifiers }
        public enum JoypadID : uint { B = 0, Y = 1, Select = 2, Start = 3, Up = 4, Down = 5, Left = 6, Right = 7, A = 8, X = 9, L = 10, R = 11 }
        public enum MouseID : uint { X = 0, Y = 1, Left = 2, Right = 3 }
        public enum SuperScopeID : uint { X = 0, Y = 1, Trigger = 2, Cursor = 3, Turbo = 4, Pause = 5 }
        public enum JustifierID : uint { X = 0, Y = 1, Trigger = 2, Start = 3 }

        public byte port_read(bool portnumber)
        {
            if (Cartridge.cartridge.has_serial && Convert.ToInt32(portnumber) == 1)
            {
                return (byte)((Convert.ToUInt32(Serial.serial.data2) << 1) | (Convert.ToUInt32(Serial.serial.data1) << 0));
            }

            Port p = port[Convert.ToInt32(portnumber)];

            switch (p.device)
            {
                case Device.Joypad:
                    {
                        if (CPU.cpu.joylatch() == Convert.ToBoolean(0))
                        {
                            if (p.counter0 >= 16)
                            {
                                return 1;
                            }
                            return (byte)System.system.Interface.input_poll(portnumber, p.device, 0, p.counter0++);
                        }
                        else
                        {
                            return (byte)System.system.Interface.input_poll(portnumber, p.device, 0, 0);
                        }
                    } //case Device::Joypad
                case Device.Multitap:
                    {
                        if (CPU.cpu.joylatch())
                        {
                            return 2; //when latch is high -- data2 = 1, data1 = 0
                        }

                        uint deviceidx, deviceindex0, deviceindex1;
                        byte mask = (byte)(Convert.ToInt32(portnumber) == 0 ? 0x40 : 0x80);

                        if (Convert.ToBoolean(CPU.cpu.pio() & mask))
                        {
                            deviceidx = p.counter0;
                            if (deviceidx >= 16)
                            {
                                return 3;
                            }
                            p.counter0++;

                            deviceindex0 = 0;  //controller 1
                            deviceindex1 = 1;  //controller 2
                        }
                        else
                        {
                            deviceidx = p.counter1;
                            if (deviceidx >= 16)
                            {
                                return 3;
                            }
                            p.counter1++;

                            deviceindex0 = 2;  //controller 3
                            deviceindex1 = 3;  //controller 4
                        }

                        return (byte)((System.system.Interface.input_poll(portnumber, p.device, deviceindex0, deviceidx) << 0)
                             | (System.system.Interface.input_poll(portnumber, p.device, deviceindex1, deviceidx) << 1));
                    } //case Device::Multitap
                case Device.Mouse:
                    {
                        if (p.counter0 >= 32)
                        {
                            return 1;
                        }

                        int position_x = System.system.Interface.input_poll(portnumber, p.device, 0, (uint)MouseID.X);  //-n = left, 0 = center, +n = right
                        int position_y = System.system.Interface.input_poll(portnumber, p.device, 0, (uint)MouseID.Y);  //-n = up,   0 = center, +n = right

                        bool direction_x = position_x < 0;  //0 = right, 1 = left
                        bool direction_y = position_y < 0;  //0 = down,  1 = up

                        if (position_x < 0)
                        {
                            position_x = -position_x;  //abs(position_x)
                        }
                        if (position_y < 0)
                        {
                            position_y = -position_y;  //abs(position_x)
                        }

                        position_x = Math.Min(127, position_x);  //range = 0 - 127
                        position_y = Math.Min(127, position_y);  //range = 0 - 127

                        switch (p.counter0++)
                        {
                            default:
                            case 0:
                                return 0;
                            case 1:
                                return 0;
                            case 2:
                                return 0;
                            case 3:
                                return 0;
                            case 4:
                                return 0;
                            case 5:
                                return 0;
                            case 6:
                                return 0;
                            case 7:
                                return 0;

                            case 8:
                                return (byte)System.system.Interface.input_poll(portnumber, p.device, 0, (uint)MouseID.Right);
                            case 9:
                                return (byte)System.system.Interface.input_poll(portnumber, p.device, 0, (uint)MouseID.Left);
                            case 10:
                                return 0;  //speed (0 = slow, 1 = normal, 2 = fast, 3 = unused)
                            case 11:
                                return 0;  // ||

                            case 12:
                                return 0;  //signature
                            case 13:
                                return 0;  // ||
                            case 14:
                                return 0;  // ||
                            case 15:
                                return 1;  // ||

                            case 16:
                                return (byte)(Convert.ToInt32(direction_y) & 1);
                            case 17:
                                return (byte)((position_y >> 6) & 1);
                            case 18:
                                return (byte)((position_y >> 5) & 1);
                            case 19:
                                return (byte)((position_y >> 4) & 1);
                            case 20:
                                return (byte)((position_y >> 3) & 1);
                            case 21:
                                return (byte)((position_y >> 2) & 1);
                            case 22:
                                return (byte)((position_y >> 1) & 1);
                            case 23:
                                return (byte)((position_y >> 0) & 1);

                            case 24:
                                return (byte)(Convert.ToInt32(direction_x) & 1);
                            case 25:
                                return (byte)((position_x >> 6) & 1);
                            case 26:
                                return (byte)((position_x >> 5) & 1);
                            case 27:
                                return (byte)((position_x >> 4) & 1);
                            case 28:
                                return (byte)((position_x >> 3) & 1);
                            case 29:
                                return (byte)((position_x >> 2) & 1);
                            case 30:
                                return (byte)((position_x >> 1) & 1);
                            case 31:
                                return (byte)((position_x >> 0) & 1);
                        }
                    } //case Device::Mouse
                case Device.SuperScope:
                    {
                        if (Convert.ToInt32(portnumber) == 0)
                        {
                            break;  //Super Scope in port 1 not supported ...
                        }
                        if (p.counter0 >= 8)
                        {
                            return 1;
                        }

                        if (p.counter0 == 0)
                        {
                            //turbo is a switch; toggle is edge sensitive
                            bool turbo = Convert.ToBoolean(System.system.Interface.input_poll(portnumber, p.device, 0, (uint)SuperScopeID.Turbo));
                            if (turbo && !p.superscope.turbolock)
                            {
                                p.superscope.turbo = !p.superscope.turbo;  //toggle state
                                p.superscope.turbolock = true;
                            }
                            else if (!turbo)
                            {
                                p.superscope.turbolock = false;
                            }

                            //trigger is a button
                            //if turbo is active, trigger is level sensitive, otherwise it is edge sensitive
                            p.superscope.trigger = false;
                            bool trigger = Convert.ToBoolean(System.system.Interface.input_poll(portnumber, p.device, 0, (uint)SuperScopeID.Trigger));
                            if (trigger && (p.superscope.turbo || !p.superscope.triggerlock))
                            {
                                p.superscope.trigger = true;
                                p.superscope.triggerlock = true;
                            }
                            else if (!trigger)
                            {
                                p.superscope.triggerlock = false;
                            }

                            //cursor is a button; it is always level sensitive
                            p.superscope.cursor = Convert.ToBoolean(System.system.Interface.input_poll(portnumber, p.device, 0, (uint)SuperScopeID.Cursor));

                            //pause is a button; it is always edge sensitive
                            p.superscope.pause = false;
                            bool pause = Convert.ToBoolean(System.system.Interface.input_poll(portnumber, p.device, 0, (uint)SuperScopeID.Pause));
                            if (pause && !p.superscope.pauselock)
                            {
                                p.superscope.pause = true;
                                p.superscope.pauselock = true;
                            }
                            else if (!pause)
                            {
                                p.superscope.pauselock = false;
                            }

                            p.superscope.offscreen =
                               p.superscope.x < 0 || p.superscope.x >= 256
                            || p.superscope.y < 0 || p.superscope.y >= (PPU.ppu.overscan() ? 240 : 225);
                        }

                        switch (p.counter0++)
                        {
                            case 0:
                                return Convert.ToByte(p.superscope.trigger);
                            case 1:
                                return Convert.ToByte(p.superscope.cursor);
                            case 2:
                                return Convert.ToByte(p.superscope.turbo);
                            case 3:
                                return Convert.ToByte(p.superscope.pause);
                            case 4:
                                return 0;
                            case 5:
                                return 0;
                            case 6:
                                return Convert.ToByte(p.superscope.offscreen);
                            case 7:
                                return 0;  //noise (1 = yes)
                            default:
                                return 0;
                        }
                    } //case Device::SuperScope
                case Device.Justifier:
                case Device.Justifiers:
                    {
                        if (Convert.ToInt32(portnumber) == 0)
                        {
                            break;  //Justifier in port 1 not supported ...
                        }
                        if (p.counter0 >= 32)
                        {
                            return 1;
                        }

                        if (p.counter0 == 0)
                        {
                            p.justifier.trigger1 = Convert.ToBoolean(System.system.Interface.input_poll(portnumber, p.device, 0, (uint)JustifierID.Trigger));
                            p.justifier.start1 = Convert.ToBoolean(System.system.Interface.input_poll(portnumber, p.device, 0, (uint)JustifierID.Start));

                            if (p.device == Device.Justifiers)
                            {
                                p.justifier.trigger2 = Convert.ToBoolean(System.system.Interface.input_poll(portnumber, p.device, 1, (uint)JustifierID.Trigger));
                                p.justifier.start2 = Convert.ToBoolean(System.system.Interface.input_poll(portnumber, p.device, 1, (uint)JustifierID.Start));
                            }
                            else
                            {
                                p.justifier.x2 = -1;
                                p.justifier.y2 = -1;

                                p.justifier.trigger2 = false;
                                p.justifier.start2 = false;
                            }
                        }

                        switch (p.counter0++)
                        {
                            case 0:
                                return 0;
                            case 1:
                                return 0;
                            case 2:
                                return 0;
                            case 3:
                                return 0;
                            case 4:
                                return 0;
                            case 5:
                                return 0;
                            case 6:
                                return 0;
                            case 7:
                                return 0;
                            case 8:
                                return 0;
                            case 9:
                                return 0;
                            case 10:
                                return 0;
                            case 11:
                                return 0;

                            case 12:
                                return 1;  //signature
                            case 13:
                                return 1;  // ||
                            case 14:
                                return 1;  // ||
                            case 15:
                                return 0;  // ||

                            case 16:
                                return 0;
                            case 17:
                                return 1;
                            case 18:
                                return 0;
                            case 19:
                                return 1;
                            case 20:
                                return 0;
                            case 21:
                                return 1;
                            case 22:
                                return 0;
                            case 23:
                                return 1;

                            case 24:
                                return Convert.ToByte(p.justifier.trigger1);
                            case 25:
                                return Convert.ToByte(p.justifier.trigger2);
                            case 26:
                                return Convert.ToByte(p.justifier.start1);
                            case 27:
                                return Convert.ToByte(p.justifier.start2);
                            case 28:
                                return Convert.ToByte(p.justifier.active);

                            case 29:
                                return 0;
                            case 30:
                                return 0;
                            case 31:
                                return 0;
                            default:
                                return 0;
                        }
                    } //case Device::Justifier(s)
            } //switch(p.device)

            //no device connected
            return 0;
        }

        public void port_set_device(bool portnumber, Device device)
        {
            Port p = port[Convert.ToInt32(portnumber)];

            p.device = device;
            p.counter0 = 0;
            p.counter1 = 0;

            //set iobit to true if device is capable of latching PPU counters
            iobit = port[1].device == Device.SuperScope
                 || port[1].device == Device.Justifier
                 || port[1].device == Device.Justifiers;
            latchx = -1;
            latchy = -1;

            if (device == Device.SuperScope)
            {
                p.superscope.x = 256 / 2;
                p.superscope.y = 240 / 2;

                p.superscope.trigger = false;
                p.superscope.cursor = false;
                p.superscope.turbo = false;
                p.superscope.pause = false;
                p.superscope.offscreen = false;

                p.superscope.turbolock = false;
                p.superscope.triggerlock = false;
                p.superscope.pauselock = false;
            }
            else if (device == Device.Justifier)
            {
                p.justifier.active = Convert.ToBoolean(0);
                p.justifier.x1 = 256 / 2;
                p.justifier.y1 = 240 / 2;
                p.justifier.x2 = -1;
                p.justifier.y2 = -1;

                p.justifier.trigger1 = false;
                p.justifier.trigger2 = false;
                p.justifier.start1 = false;
                p.justifier.start2 = false;
            }
            else if (device == Device.Justifiers)
            {
                p.justifier.active = Convert.ToBoolean(0);
                p.justifier.x1 = 256 / 2 - 16;
                p.justifier.y1 = 240 / 2;
                p.justifier.x2 = 256 / 2 + 16;
                p.justifier.y2 = 240 / 2;

                p.justifier.trigger1 = false;
                p.justifier.trigger2 = false;
                p.justifier.start1 = false;
                p.justifier.start2 = false;
            }
        }

        public void init() { }

        public void poll()
        {
            port[0].counter0 = 0;
            port[0].counter1 = 0;
            port[1].counter0 = 0;
            port[1].counter1 = 0;

            port[1].justifier.active = !port[1].justifier.active;
        }

        public void update()
        {
            System.system.Interface.input_poll();
            Port p = port[1];

            switch (p.device)
            {
                case Device.SuperScope:
                    {
                        int x = System.system.Interface.input_poll(Convert.ToBoolean(1), p.device, 0, (uint)SuperScopeID.X);
                        int y = System.system.Interface.input_poll(Convert.ToBoolean(1), p.device, 0, (uint)SuperScopeID.Y);
                        x += p.superscope.x;
                        y += p.superscope.y;
                        p.superscope.x = Math.Max(-16, Math.Min(256 + 16, x));
                        p.superscope.y = Math.Max(-16, Math.Min(240 + 16, y));

                        latchx = (short)p.superscope.x;
                        latchy = (short)p.superscope.y;
                    }
                    break;
                case Device.Justifier:
                case Device.Justifiers:
                    {
                        int x1 = System.system.Interface.input_poll(Convert.ToBoolean(1), p.device, 0, (uint)JustifierID.X);
                        int y1 = System.system.Interface.input_poll(Convert.ToBoolean(1), p.device, 0, (uint)JustifierID.Y);
                        x1 += p.justifier.x1;
                        y1 += p.justifier.y1;
                        p.justifier.x1 = Math.Max(-16, Math.Min(256 + 16, x1));
                        p.justifier.y1 = Math.Max(-16, Math.Min(240 + 16, y1));

                        int x2 = System.system.Interface.input_poll(Convert.ToBoolean(1), p.device, 1, (uint)JustifierID.X);
                        int y2 = System.system.Interface.input_poll(Convert.ToBoolean(1), p.device, 1, (uint)JustifierID.Y);
                        x2 += p.justifier.x2;
                        y2 += p.justifier.y2;
                        p.justifier.x2 = Math.Max(-16, Math.Min(256 + 16, x2));
                        p.justifier.y2 = Math.Max(-16, Math.Min(240 + 16, y2));

                        if (Convert.ToInt32(p.justifier.active) == 0)
                        {
                            latchx = (short)p.justifier.x1;
                            latchy = (short)p.justifier.y1;
                        }
                        else
                        {
                            latchx = (short)(p.device == Device.Justifiers ? p.justifier.x2 : -1);
                            latchy = (short)(p.device == Device.Justifiers ? p.justifier.y2 : -1);
                        }
                    }
                    break;
            }

            if (latchy < 0 || latchy >= (PPU.ppu.overscan() ? 240 : 225) || latchx < 0 || latchx >= 256)
            {
                //cursor is offscreen, set to invalid position so counters are not latched
                latchx = ~0;
                latchy = ~0;
            }
            else
            {
                //cursor is onscreen
                latchx += 40;  //offset trigger position to simulate hardware latching delay
                latchx <<= 2;  //dot -> clock conversion
                latchx += 2;  //align trigger on half-dot ala interrupts (speed optimization for sCPU::add_clocks)
            }
        }

        //light guns (Super Scope, Justifier(s)) strobe IOBit whenever the CRT
        //beam cannon is detected. this needs to be tested at the cycle level
        //(hence inlining here for speed) to avoid 'dead space' during DRAM refresh.
        //iobit is updated during port_set_device(),
        //latchx, latchy are updated during update() (once per frame)

        public void tick()
        {     //only test if Super Scope or Justifier is connected
            if (iobit && CPU.cpu.PPUCounter.vcounter() == latchy && CPU.cpu.PPUCounter.hcounter() == latchx)
            {
                PPU.ppu.latch_counters();
            }
        }

        private bool iobit;
        public short latchx, latchy;

        private Port[] port = new Port[2];
        public Port[] Ports
        {
            get { return port; }
        }

        public Input()
        {
            Utility.InstantiateArrayElements(port);
        }
    }
}
