using System;
using System.Threading;

namespace RadFramework.Libraries.Threading
{
    public class QueuedThreadShedulerWithDispatchCapabilities : QueuedMultiThreadProcessorWithDispatchCapabilities<Action>, IThreadSheduler
    {
        public QueuedThreadShedulerWithDispatchCapabilities(
            int dispatchTimeout,
            ThreadPriority priority,
            Action<Thread> onDispatched = null,
            int dispatchLimit = 0,
            int dispatchAbortionTimeout = 0,
            string threadDescription = null) 
            : base(dispatchTimeout, priority, a => a(), onDispatched, dispatchLimit, dispatchAbortionTimeout, threadDescription)
        {
        }

        public QueuedThreadShedulerWithDispatchCapabilities(
            int size,
            int dispatchTimeout,
            ThreadPriority priority,
            Action<Thread> onDispatched = null,
            int dispatchLimit = 0,
            int dispatchAbortionTimeout = 0,
            string threadDescription = null) 
            : base(size, dispatchTimeout, priority, a => a(), onDispatched, dispatchLimit, dispatchAbortionTimeout, threadDescription)
        {
        }
    }
}