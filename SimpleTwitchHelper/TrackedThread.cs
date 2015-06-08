using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace SimpleTwitchHelper
{
    /**
     *  @author jslicer
     */
    public class TrackedThread
    {
        private static readonly IList<Thread> _threadList = new List<Thread>();
        private readonly ParameterizedThreadStart _start1;
        private readonly ThreadStart _start2;

        public TrackedThread(ParameterizedThreadStart start)
        {
            _start1 = start;
            Thread = new Thread(StartThreadParameterized);
            lock (_threadList)
            {
                _threadList.Add(Thread);
            }
        }

        public TrackedThread(ThreadStart start)
        {
            _start2 = start;
            Thread = new Thread(StartThread);
            lock (_threadList)
            {
                _threadList.Add(Thread);
            }
        }

        public TrackedThread(ParameterizedThreadStart start, int maxStackSize)
        {
            _start1 = start;
            Thread = new Thread(StartThreadParameterized, maxStackSize);
            lock (_threadList)
            {
                _threadList.Add(Thread);
            }
        }

        public TrackedThread(ThreadStart start, int maxStackSize)
        {
            _start2 = start;
            Thread = new Thread(StartThread, maxStackSize);
            lock (_threadList)
            {
                _threadList.Add(Thread);
            }
        }

        public static int Count
        {
            get
            {
                lock (_threadList)
                {
                    return _threadList.Count;
                }
            }
        }

        public static IEnumerable<Thread> ThreadList
        {
            get
            {
                lock (_threadList)
                {
                    return new ReadOnlyCollection<Thread>(_threadList);
                }
            }
        }

        public Thread Thread { get; }

        private void StartThreadParameterized(object obj)
        {
            try
            {
                _start1(obj);
            }
            finally
            {
                lock (_threadList)
                {
                    _threadList.Remove(Thread);
                }
            }
        }

        private void StartThread()
        {
            try
            {
                _start2();
            }
            finally
            {
                lock (_threadList)
                {
                    _threadList.Remove(Thread);
                }
            }
        }
    }
}