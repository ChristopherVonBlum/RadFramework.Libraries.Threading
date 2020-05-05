using System;
using System.Linq;
using System.Threading;

namespace RadFramework.Libraries.Threading
{
    public class MultiThreadProcessor : MultiThreadProcessorBase
    {
        public MultiThreadProcessor(ThreadPriority priority, Action threadBody = null, string threadDescription = null) : base(priority, threadBody)
        {
            StartThreads();
        }

        public MultiThreadProcessor(int size, ThreadPriority priority, Action threadBody = null, string threadDescription = null) : base(size, priority, threadBody)
        {
            StartThreads();
        }
    }
}