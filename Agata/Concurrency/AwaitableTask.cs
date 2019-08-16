using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Agata.Concurrency
{
    /// <summary>
    /// Bridge between Task and awaitable unit.
    /// </summary>
    internal class AwaitableTask<T> : IAwaitable, IAction
    {
        private const int PoolSize = 10000;
        private static readonly ConcurrentStack<AwaitableTask<T>> Pool = new ConcurrentStack<AwaitableTask<T>>();

        private Task<T> _task;
        private Promise<T> _promise;

        private AwaitableTask()
        {
        }

        public static AwaitableTask<T> Acquire(Task<T> task, Promise<T> promise)
        {
            if (!Pool.TryPop(out var awaitableTask))
            {
                awaitableTask = new AwaitableTask<T>();
            }

            awaitableTask._task = task;
            awaitableTask._promise = promise;

            return awaitableTask;
        }

        private static void Release(AwaitableTask<T> awaitableTask)
        {
            awaitableTask._task = null;
            awaitableTask._promise = null;

            if (Pool.Count < PoolSize)
            {
                Pool.Push(awaitableTask);
            }
        }

        public bool Ready => _task.IsCompleted || _task.IsFaulted || _task.IsCanceled;

        public void Invoke()
        {
            try
            {
                if (_task.IsFaulted || _task.IsCanceled)
                {
                    _promise.Fail(_task.Exception);
                    return;
                }

                _promise.Success(_task.Result);
            }
            catch (Exception error)
            {
                _promise.Fail(error);
            }
            finally
            {
                Release(this);
            }
        }
    }
}