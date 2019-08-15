using System;
using static Agata.Ensure;

namespace Agata.Concurrency
{
    /// <summary>
    /// Represents a place where future continuation should be executed.
    /// </summary>
    public sealed class Execute
    {
        /// <summary>
        /// Execute future continuation on last used thread.
        /// </summary>
        public static readonly Execute OnLastThread = new Execute(null);

        /// <summary>
        /// Execute future continuation on specified thread pool.
        /// </summary>
        /// <param name="threadPool">Thread pool for service future continuation.</param>
        /// <returns>An execute unit.</returns>
        public static Execute On(IThreadPool threadPool)
        {
            return new Execute(NotNull(threadPool, nameof(threadPool)));
        }
        
        private readonly IThreadPool _threadPool;

        private Execute(IThreadPool threadPool)
        {
            _threadPool = threadPool;
        }

        internal void Action(Action action)
        {
            if (_threadPool == null)
            {
                action();
                return;
            }
            
            _threadPool.Schedule(action);
        }
    }
}