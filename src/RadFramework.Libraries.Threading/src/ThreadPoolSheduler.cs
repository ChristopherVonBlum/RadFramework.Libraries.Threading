using System;
using System.Threading;

namespace RadFramework.Libraries.Threading
{
    public class ThreadPoolSheduler : IThreadSheduler
    {
        public void Dispose()
        {
        }

        public void Enqueue(Action task)
        {
            ThreadPool.QueueUserWorkItem(state => task());
        }
    }
}