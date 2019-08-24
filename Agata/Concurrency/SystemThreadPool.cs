using System;
using System.Threading;

namespace Agata.Concurrency
{
    public class SystemThreadPool : IThreadPool
    {
        public static readonly SystemThreadPool Instance = new SystemThreadPool();
        
        private SystemThreadPool()
        {
        }

        public void Schedule(IAction action)
        {
            Schedule(action.Invoke);
        }

        public void Schedule(Action action)
        {
            ThreadPool.QueueUserWorkItem(_ => action());
        }
    }
}