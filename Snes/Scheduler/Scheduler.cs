using System.Collections;

namespace Snes
{
    class Scheduler
    {
        public static Scheduler scheduler = new Scheduler();

        public enum SynchronizeMode : uint { None, CPU, All }
        public SynchronizeMode sync;

        public enum ExitReason : uint { UnknownEvent, FrameEvent, SynchronizeEvent, DebuggerEvent }
        public ExitReason exit_reason { get; private set; }

        public IEnumerator thread; //active emulation thread (used to enter emulation)

        public void enter()
        {
            while (true)
            {
                thread.MoveNext();
                //TODO: Set a static ExitReason and find a way to yield break?
                if (thread.Current is ExitReason)
                {
                    exit_reason = (ExitReason)thread.Current;
                    break;
                }
                thread = (IEnumerator)thread.Current;
            }
        }

        public void init()
        {
            thread = CPU.cpu.Processor.thread;
            sync = SynchronizeMode.None;
        }

        public Scheduler()
        {
            thread = null;
            exit_reason = ExitReason.UnknownEvent;
        }
    }
}
