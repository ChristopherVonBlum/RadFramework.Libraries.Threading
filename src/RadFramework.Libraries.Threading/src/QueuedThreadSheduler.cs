using System;
using System.Collections.Generic;
using System.Threading;

namespace RadFramework.Libraries.Threading
{
    public class QueuedThreadSheduler : QueuedMultiThreadProcessor<Action>, IThreadSheduler
    {
        public QueuedThreadSheduler(
            int size,
            ThreadPriority priority,
            string threadDescription) 
            : base(
                size,
                priority,
                (a) => a(),
                threadDescription)
        {
        }
    }
}