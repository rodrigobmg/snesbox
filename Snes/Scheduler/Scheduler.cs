using System.Threading;
using Nall;

namespace Snes
{
    class Scheduler
    {
        public static Scheduler scheduler = new Scheduler();

        public enum SynchronizeMode : uint { None, CPU, All }
        public SynchronizeMode sync;

        public enum ExitReason : uint { UnknownEvent, FrameEvent, SynchronizeEvent, DebuggerEvent }
        public ExitReason exit_reason { get; private set; }

        public Thread host_thread; //program thread (used to exit emulation)
        public Thread thread; //active emulation thread (used to enter emulation)

        public void enter()
        {
            host_thread = Libco.Active();
            Libco.Switch(thread);
        }

        public void exit(ExitReason reason)
        {
            exit_reason = reason;
            thread = Libco.Active();
            Libco.Switch(host_thread);
        }

        public void init()
        {
            host_thread = Libco.Active();
            thread = CPU.cpu.Processor.thread;
            sync = SynchronizeMode.None;
        }

        public Scheduler()
        {
            host_thread = null;
            thread = null;
            exit_reason = ExitReason.UnknownEvent;
        }
    }
}
