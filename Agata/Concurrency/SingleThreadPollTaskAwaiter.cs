using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Agata.Logging;

namespace Agata.Concurrency
{
    /// <summary>
    /// Represents a task awaiter which waits task completion on single thread with poll strategy.
    /// </summary>
    public class SingleThreadPollTaskAwaiter : IAwaiter
    {
        private struct QueueEntry
        {
            public QueueEntry(IAwaitable awaitable, IAction callback)
            {
                Awaitable = awaitable;
                Callback = callback;
            }

            public readonly IAwaitable Awaitable;
            public readonly IAction Callback;
        }

        private static readonly ILogger Log = Logger.Create<SingleThreadPollTaskAwaiter>();

        private readonly BlockingCollection<QueueEntry> _queue;

        /// <summary>
        /// Initializes a new instance of tasks awaiter. 
        /// </summary>
        /// <param name="name">Thread name.</param>
        /// <param name="priority">Thread priority.</param>
        public SingleThreadPollTaskAwaiter(string name, ThreadPriority priority)
        {
            _queue = new BlockingCollection<QueueEntry>(new ConcurrentQueue<QueueEntry>());
            var thread = new Thread(Loop) {IsBackground = true, Name = name, Priority = priority};
            thread.Start(this);
        }

        /// <summary>
        /// Awaits awaitable object to completion.
        /// </summary>
        /// <param name="awaitable">An awaitable object to await.</param>
        /// <param name="onReady">Action which should be performed when awaitable object will completed.</param>
        public void Await(IAwaitable awaitable, IAction onReady)
        {
            var entry = new QueueEntry(awaitable, onReady);
            _queue.Add(entry);
        }

        private void Loop(object ignore)
        {
            var savedEntries = new List<QueueEntry>();
            while (true)
            {
                var entry = _queue.Take();
                if (!entry.Awaitable.Ready)
                {
                    savedEntries.Add(entry);
                }
                else
                {
                    CompleteEntry(entry);
                }

                while (_queue.TryTake(out entry))
                {
                    if (!entry.Awaitable.Ready)
                    {
                        savedEntries.Add(entry);
                        continue;
                    }

                    CompleteEntry(entry);
                }

                foreach (var savedEntry in savedEntries)
                {
                    _queue.Add(savedEntry);
                }

                savedEntries.Clear();

                Thread.Sleep(0);
            }
        }

        private static void CompleteEntry(QueueEntry entry)
        {
            try
            {
                entry.Callback.Invoke();
            }
            catch (Exception e)
            {
                Log.Error($"Error on complete task awaiting. Error: {e}");
            }
        }
    }
}