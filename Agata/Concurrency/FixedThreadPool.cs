using System;
using System.Threading;
using Helios.Concurrency;

namespace Agata.Concurrency
{
    /// <summary>
    /// Represents a thread pool which bases on Helios dedicated thread pool.
    /// This thread pool should be used as preferred thread pool.
    /// https://github.com/helios-io/DedicatedThreadPool 
    /// </summary>
    public class FixedThreadPool : IThreadPool
    {
        private readonly DedicatedThreadPool _threadPool;

        /// <summary>
        /// Creates a new thread pool.
        /// </summary>
        /// <param name="name">A name of this thread pool.</param>
        /// <param name="threadsNumber">A number of thread in this thread pool. Default value is a count of processors.</param>
        /// <param name="threadsPriority">A priority of threads in this thread pool. Default value is </param>
        public FixedThreadPool(
            string name = null,
            int? threadsNumber = null, 
            ThreadPriority? threadsPriority = null)
        {
            var settings = new DedicatedThreadPoolSettings(
                threadsNumber ?? Environment.ProcessorCount,
                threadsPriority,
                string.IsNullOrWhiteSpace(name) ? "HeliosThreadPool-" + Guid.NewGuid() : name);
            
            _threadPool = new DedicatedThreadPool(settings);
        }

        public void Schedule(Action action)
        {
            Schedule(ThreadPoolSystemActionWrapper.Wrap(action));
        }

        public void Schedule(IAction action)
        {
            _threadPool.QueueUserWorkItem(action);
        }
    }
}