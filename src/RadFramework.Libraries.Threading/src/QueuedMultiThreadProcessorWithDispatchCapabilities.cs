using System;
using System.Threading;

namespace RadFramework.Libraries.Threading
{
    public class QueuedMultiThreadProcessorWithDispatchCapabilities<TQueueTask> : QueuedMultiThreadProcessor<TQueueTask>
    {
        private readonly int _dispatchTimeout;
        private readonly Action<Thread> _onDispatched;
        private readonly int _dispatchLimit;
        private readonly int _dispatchAbortionTimeout;
        private volatile int _dispatchCount;
        
        public QueuedMultiThreadProcessorWithDispatchCapabilities(
            int dispatchTimeout, 
            ThreadPriority priority, 
            Action<TQueueTask> handlerAction, 
            Action<Thread> onDispatched = null, 
            int dispatchLimit = 0,
            int dispatchAbortionTimeout = 0,
            string threadDescription = null) 
            : base(Environment.ProcessorCount, priority, handlerAction, threadDescription)
        {
            _dispatchTimeout = dispatchTimeout;
            _onDispatched = onDispatched;
            _dispatchLimit = dispatchLimit;
            _dispatchAbortionTimeout = dispatchAbortionTimeout;
        }
        
        public QueuedMultiThreadProcessorWithDispatchCapabilities(
            int size, 
            int dispatchTimeout, 
            ThreadPriority priority, 
            Action<TQueueTask> handlerAction, 
            Action<Thread> onDispatched = null, 
            int dispatchLimit = 0,
            int dispatchAbortionTimeout = 0,
            string threadDescription = null) 
            : base(size, priority, handlerAction, threadDescription)
        {
            _dispatchTimeout = dispatchTimeout;
            _onDispatched = onDispatched;
            _dispatchLimit = dispatchLimit;
            _dispatchAbortionTimeout = dispatchAbortionTimeout;
        }

        protected override void InvokeHandlerAction(TQueueTask task)
        {
            var thread = StartHandlerThread(task);
            thread.Join(_dispatchTimeout);
            if (thread.ThreadState == ThreadState.Running && TryDispatchThread(Thread.CurrentThread))
            {
                _onDispatched?.Invoke(thread);
                
                if (this._dispatchAbortionTimeout != 0)
                {
                    thread.Join(_dispatchAbortionTimeout);
                    
                    if (thread.ThreadState == ThreadState.Running)
                    {
                        try
                        {
                            thread.Abort();
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    thread.Join();
                }
                
                _dispatchCount--;
                return;
            }

            thread.Join();
        }

        private bool TryDispatchThread(Thread currentThread)
        {
            if (_dispatchCount < _dispatchLimit)
            {
                _dispatchCount++;
                DispatchThread(currentThread);
                return true;
            }

            return false;
        }

        protected override void ThreadBody()
        {
            while (!this.Disposed && IsCurrentThreadAttached())
            {
                TQueueTask request;

                if (IsCurrentThreadAttached())
                    while (this.Queue.TryDequeue(out request))
                    {
                        try
                        {
                            InvokeHandlerAction(request);
                        }
                        catch
                        {
                        }

                        if (!IsCurrentThreadAttached())
                        {
                            return;
                        }
                    }
                else return;

                this.ProcessQueueSemaphore?.Wait();
            }
        }
        
        private Thread StartHandlerThread(TQueueTask task)
        {
            Thread t = new Thread(() => base.InvokeHandlerAction(task));
            t.Priority = Priority;
            t.Start();
            return t;
        }
    }
}