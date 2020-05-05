using System;
using System.Threading;

namespace RadFramework.Libraries.Threading
{
    public class MultiThreadProcessorWithDispatchCapabilities : MultiThreadProcessorBase
    {
        private readonly int _dispatchTimeout;
        private readonly Action<Thread> _onDispatched;
        private readonly int _dispatchLimit;
        private readonly int _dispatchAbortionTimeout;
        private volatile int _dispatchCount;
        
        public MultiThreadProcessorWithDispatchCapabilities(
            int dispatchTimeout, 
            ThreadPriority priority, 
            Action handlerAction, 
            Action<Thread> onDispatched = null, 
            int dispatchLimit = 0,
            string threadDescription = null) 
            : base(Environment.ProcessorCount, priority, handlerAction, threadDescription)
        {
            _dispatchTimeout = dispatchTimeout;
            _onDispatched = onDispatched;
            _dispatchLimit = dispatchLimit;
            StartThreads();
        }
        
        public MultiThreadProcessorWithDispatchCapabilities(
            int size, 
            int dispatchTimeout, 
            ThreadPriority priority, 
            Action handlerAction, 
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
            StartThreads();
        }
        protected void InvokeHandlerAction()
        {
            var thread = StartHandlerThread();
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
                try
                {
                    InvokeHandlerAction();
                }
                catch
                {
                }

                if (!IsCurrentThreadAttached())
                {
                    return;
                }
            }
        }

        public bool Disposed { get; set; }

        private Thread StartHandlerThread()
        {
            Thread t = new Thread(() => base.ThreadBody());
            t.Priority = Priority;
            t.Start();
            return t;
        }

        public override void Dispose()
        {
            Disposed = true;
            base.Dispose();
        }
    }
}