using System;
using System.Collections.Concurrent;

namespace Agata.Concurrency
{
    internal sealed class ThreadPoolSystemActionWrapper : IAction
    {
        private static readonly ConcurrentStack<ThreadPoolSystemActionWrapper> Pool
            = new ConcurrentStack<ThreadPoolSystemActionWrapper>();

        private Action _action;

        public static IAction Wrap(Action action)
        {
            if (!Pool.TryPop(out var wrapper))
            {
                wrapper = new ThreadPoolSystemActionWrapper();
            }

            wrapper._action = action;
            return wrapper;
        }

        public static void Release(ThreadPoolSystemActionWrapper wrapper)
        {
            wrapper._action = null;
            if (Pool.Count >= 10000)
            {
                return;
            }
            
            Pool.Push(wrapper);
        }
        
        private ThreadPoolSystemActionWrapper()
        {
        }

        public void Invoke()
        {
            _action.Invoke();
        }
    }
}