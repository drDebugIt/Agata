using System;

namespace Agata.Concurrency
{
    /// <summary>
    /// Describes an interface of thread pool.
    /// </summary>
    public interface IThreadPool
    {
        /// <summary>
        /// Schedules execution of specified action on this thread pool.  
        /// </summary>
        /// <param name="action">An action to execute.</param>
        void Schedule(Action action);
    }
}