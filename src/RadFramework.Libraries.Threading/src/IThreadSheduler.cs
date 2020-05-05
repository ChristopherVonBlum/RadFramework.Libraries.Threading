using System;
using System.Collections.Generic;

namespace RadFramework.Libraries.Threading
{
    public interface IThreadSheduler : IDisposable
    {
        void Enqueue(Action task);
    }
}