using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agata.Logging;

namespace Agata.Concurrency
{
    /// <summary>
    /// Represents a task awaiter which waits task completion on single thread with poll strategy.
    /// </summary>
    public class SingleThreadPollTaskAwaiter : ITaskAwaiter
    {
        private struct QueueEntry
        {
            public QueueEntry(Task task, Action action)
            {
                Task = task;
                Action = action;
            }

            public readonly Task Task;
            public readonly Action Action;
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
        /// Awaits task completion.
        /// </summary>
        /// <param name="task">A task to await.</param>
        /// <param name="onTaskCompleted">Action which should be performed when task will completed.</param>
        public void Await(Task task, Action onTaskCompleted)
        {
            var entry = new QueueEntry(task, onTaskCompleted);
            _queue.Add(entry);
        }

        private void Loop(object ignore)
        {
            var savedEntries = new List<QueueEntry>();
            while (true)
            {
                var entry = _queue.Take();
                var completed = entry.Task.IsCompleted || entry.Task.IsFaulted || entry.Task.IsCanceled;
                if (!completed)
                {
                    savedEntries.Add(entry);
                }
                else
                {
                    CompleteEntry(entry);
                }
                
                while (_queue.TryTake(out entry))
                {
                    completed = entry.Task.IsCompleted || entry.Task.IsFaulted || entry.Task.IsCanceled;
                    if (!completed)
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
                entry.Action();
            }
            catch (Exception e)
            {
                Log.Error($"Error on complete task awaiting. Error: {e}");
            }
        }
    }
}