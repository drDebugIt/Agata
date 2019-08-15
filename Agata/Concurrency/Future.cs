using System;
using System.Threading.Tasks;

namespace Agata.Concurrency
{
    /// <summary>
    /// Provides a set of methods for start Futures.
    /// </summary>
    public static class Future
    {
        /// <summary>
        /// Schedules action execution on specified thread pool. 
        /// </summary>
        /// <param name="threadPool">A thread pool for service action.</param>
        /// <param name="action">Action fot execute.</param>
        /// <typeparam name="T">Type of schedules action result.</typeparam>
        /// <returns>A future of action executing.</returns>
        public static Future<T> Start<T>(IThreadPool threadPool, Func<T> action)
        {
            var promise = new Promise<T>();
            var tpAction = new Action(() =>
            {
                try
                {
                    var result = action();
                    promise.Success(result);
                }
                catch (Exception error)
                {
                    promise.Fail(error);
                }
            });

            threadPool.Schedule(tpAction);

            return promise.Future;
        }

        /// <summary>
        /// Creates future from specified .Net task.
        /// Completion of this task will await with specified awaiter.
        /// </summary>
        /// <param name="awaiter">A strategy of await task completion.</param>
        /// <param name="task">A task which should be transformed to future.</param>
        /// <typeparam name="T">Type of task result.</typeparam>
        /// <returns>A future which wraps task computation results.</returns>
        public static Future<T> From<T>(ITaskAwaiter awaiter, Task<T> task)
        {
            var promise = new Promise<T>();
            awaiter.Await(task, () =>
            {
                try
                {
                    if (task.IsCompleted)
                    {
                        promise.Success(task.Result);
                        return;
                    }

                    promise.Fail(task.Exception);
                }
                catch (Exception error)
                {
                    promise.Fail(error);
                }
            });
            return promise.Future;
        }
    }
}