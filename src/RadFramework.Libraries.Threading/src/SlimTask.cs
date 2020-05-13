using System;
using System.Threading;

namespace RadFramework.Libraries.Threading
{
    public class SlimTask<TIn, TOut>
    {
        private readonly Func<TIn, TOut> task;
        private ManualResetEvent Awaiter { get; set; }
        public Thread Thread { get; private set; }
        
        public TOut Result { get; private set; }
        
        public SlimTask(Func<TIn, TOut> task)
        {
            this.task = task;
            Awaiter = new ManualResetEvent(false);
            Thread = new Thread(o => ProcessInternal(o));
        }
        
        public SlimTask<TIn, TOut> Start()
        {
            Thread.Start();
            return this;
        }

        public TOut Await()
        {
            Awaiter.WaitOne();
            return Result;
        }
        
        void ProcessInternal(object o)
        {
            var result = task((TIn)o);
            Awaiter.Set();
            Result = result;
        }
    }

    public class SlimTask<TOut>
    {
        private readonly Func<TOut> task;
        private ManualResetEvent Awaiter { get; set; }
        public Thread Thread { get; private set; }

        public TOut Result { get; private set; }
        
        public SlimTask(Func<TOut> task)
        {
            this.task = task;
            Awaiter = new ManualResetEvent(false);
            Thread = new Thread(o => ProcessInternal(o));
        }
        
        public SlimTask(TOut task)
        {
            this.Result = task;
        }

        public SlimTask<TOut> Start()
        {
            Thread.Start();
            return this;
        }

        public TOut Await()
        {
            Awaiter.WaitOne();
            return Result;

        }
        
        void ProcessInternal(object o)
        {
            Result = task();
            Awaiter.Set();
        }
    }

    public class SlimTask
    {
        private readonly Action task;
        private ManualResetEvent Awaiter { get; set; }
        public Thread Thread { get; private set; }

        public SlimTask(Action task)
        {
            this.task = task;
            Awaiter = new ManualResetEvent(false);
            Thread = new Thread(o => ProcessInternal());
        }

        public SlimTask Start()
        {
            Thread.Start();
            return this;
        }

        public void Await()
        {
            Awaiter.WaitOne();
        }
        
        void ProcessInternal()
        {
            task();
            Awaiter.Set();
        }
    }
}