using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RadFramework.Libraries.Threading
{
    public class QueuedMultiThreadProcessor<TQueueTask> : MultiThreadProcessorBase
    {
        protected bool Disposed = false;

        protected ConcurrentQueue<TQueueTask> Queue { get; } = new ConcurrentQueue<TQueueTask>();

        private readonly int _size;
        private Action<TQueueTask> HandlerAction;

        protected SemaphoreSlim ProcessQueueSemaphore;

        private bool shuttingDown;
        
        private Timer processQueueTimer;

        public QueuedMultiThreadProcessor(int size, ThreadPriority priority, Action<TQueueTask> handlerAction, string threadDescription = null) : base(size, priority, null, threadDescription)
        {
            this.ProcessQueueSemaphore = new SemaphoreSlim(0, size);
            this.processQueueTimer = new Timer((o) => FlushPackages(), null, 0, 5000);
            _size = size;
            this.HandlerAction = handlerAction;
            StartThreads();
        }

        protected override void ThreadBody()
        {
            while (!this.Disposed)
            {
                TQueueTask request;

                while (this.Queue.TryDequeue(out request))
                {
                    try
                    {
                        InvokeHandlerAction(request);
                    }
                    catch
                    {
                    }
                }

                this.ProcessQueueSemaphore.Wait();
            }
        }

        protected virtual void InvokeHandlerAction(TQueueTask task)
        {
            this.HandlerAction(task);
        }
        
        public void Enqueue(TQueueTask request)
        {
            if (this.shuttingDown)
            {
                throw new InvalidOperationException();
            }

            this.Queue.Enqueue(request);

            FlushPackages();
        }

        private void FlushPackages()
        {
            if (this.ProcessQueueSemaphore.CurrentCount == this._size)
            {
                return;
            }

            try
            {
                this.ProcessQueueSemaphore.Release(1);
            }
            // since the if statement above and the release call the count increased.
            // shit happens.
            catch(SemaphoreFullException)
            {
                return;
            }

            EnsureThreadAddedOrAllThreadsRunning();
        }

        public override void Dispose()
        {
            this.shuttingDown = true;
            // dont loose the queue early
            while (!this.Queue.IsEmpty)
            {
                Thread.Sleep(250);
            }

            this.Disposed = true;

            base.Dispose();

            this.processQueueTimer.Dispose();
            
            this.ProcessQueueSemaphore.Dispose();
        }
    }
}
