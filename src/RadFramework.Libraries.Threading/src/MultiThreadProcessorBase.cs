using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading;

namespace RadFramework.Libraries.Threading
{
    public abstract class MultiThreadProcessorBase : IDisposable
    {
        private readonly int _size;
        private readonly Action _threadBody;
        private readonly string _threadDescription;
        protected ThreadPriority Priority { get; }
        private ConcurrentDictionary<int, Thread> processingThreads;

        public MultiThreadProcessorBase(ThreadPriority priority, Action threadBody = null, string threadDescription = null) : this(Environment.ProcessorCount, priority, threadBody, threadDescription)
        {
            _threadBody = threadBody;
        }
        
        public MultiThreadProcessorBase(int size, ThreadPriority priority, Action threadBody = null, string threadDescription = null)
        {
            _threadDescription = threadDescription ?? this.GetType().FullName;
            this._size = size;
            _threadBody = threadBody;
            Priority = priority;
            this.processingThreads = new ConcurrentDictionary<int, Thread>();
            this.CreateThreads(size);
        }
        
        protected void StartThreads()
        {
            foreach (var processingThread in processingThreads)
            {
                processingThread.Value.Start();
            }
        }

        protected virtual void CreateThreads(int size)
        {
            for (int i = 0; i < size; i++)
            {
                CreateNewProcessingThread();
            }
        }

        private Thread CreateNewProcessingThread()
        {
            Thread newThread = new Thread(this.InvokeThreadBody);
            newThread.Priority = Priority;
            newThread.Name = _threadDescription;
            processingThreads[newThread.ManagedThreadId] = newThread;
            return newThread;
        }

        protected void EnsureThreadAddedOrAllThreadsRunning()
        {
            if (processingThreads.Count >= _size)
            {
                return;
            }

            CreateNewProcessingThread();
        }
        
        /// <summary>
        /// This wrapper ensures that the thread is only registered while it is actually running.
        /// After that the thread reference is released and ready for garbage collection.
        /// </summary>
        private void InvokeThreadBody()
        {
            try
            {
                ThreadBody();
            }
            catch
            {
                processingThreads.TryRemove(Thread.CurrentThread.ManagedThreadId, out var t);
                throw;
            }
            finally
            {
                processingThreads.TryRemove(Thread.CurrentThread.ManagedThreadId, out var t);
            }
        }
        
        protected virtual void ThreadBody()
        {
            _threadBody();
        }

        public void DispatchThread(Thread t)
        {
            lock (processingThreads)
            {
                processingThreads.TryRemove(t.ManagedThreadId, out Thread thread);
            }
        }

        public bool IsCurrentThreadAttached()
        {
            return processingThreads.ContainsKey(Thread.CurrentThread.ManagedThreadId);
        }
        
        public virtual void Dispose()
        {
            foreach (KeyValuePair<int, Thread> keyValuePair in this.processingThreads)
            {
                keyValuePair.Value.Join();
            }
        }
    }
}