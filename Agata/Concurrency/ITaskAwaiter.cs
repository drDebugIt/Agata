using System;
using System.Threading.Tasks;

namespace Agata.Concurrency
{
    /// <summary>
    /// Describes an interface of awaiter which wait when tasks will completed. 
    /// </summary>
    public interface ITaskAwaiter
    {
        /// <summary>
        /// Awaits task completion.
        /// </summary>
        /// <param name="task">A task to await.</param>
        /// <param name="onTaskCompleted">Action which should be performed when task will completed.</param>
        void Await(Task task, Action onTaskCompleted);
    }
}