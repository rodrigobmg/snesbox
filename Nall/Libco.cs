﻿using System.Collections.Generic;
using System.Threading;

namespace Nall
{
    public static class Libco
    {
        private static Thread _active;
        private static Dictionary<Thread, ManualResetEventSlim> _threads = new Dictionary<Thread, ManualResetEventSlim>();
        private static readonly object _locker = new object();

        public static Thread Active()
        {
            if (ReferenceEquals(_active, null))
            {
                _active = Thread.CurrentThread;
            }
            return _active;
        }

        public static Thread Create(string name, int size, ThreadStart entrypoint)
        {
            if (ReferenceEquals(_active, null))
            {
                _active = Thread.CurrentThread;
                _threads.Add(_active, new ManualResetEventSlim(false));
            }

            size += 256; /* allocate additional space for storage */
            size &= ~15; /* align stack to 16-byte boundary */
            var thread = new Thread(entrypoint, size) { Name = name };
            _threads.Add(thread, new ManualResetEventSlim(false));
            return thread;
        }

        public static void Delete(Thread thread)
        {
            thread.Abort();
            thread = null;
        }

        public static void Switch(Thread thread)
        {
            var previous = _active;
            _active = thread;

            _threads[previous].Reset();
            if (_active.ThreadState == ThreadState.Unstarted)
            {
                _active.Start();
            }
            else
            {
                _threads[_active].Set();
            }

            _threads[previous].Wait();
        }
    }
}
