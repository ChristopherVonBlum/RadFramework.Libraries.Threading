using System;

namespace RadFramework.Libraries.Threading
{
    public class StayInCurrentThreadSheduler : IThreadSheduler
    {
        public void Enqueue(Action task)
        {
            task();
        }

        public void Dispose()
        {
        }
    }
}