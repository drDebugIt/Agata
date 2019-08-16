namespace Agata.Concurrency
{
    /// <summary>
    /// Describes interface of awaiter.
    /// </summary>
    public interface IAwaiter
    {
        /// <summary>
        /// Schedules awaitable object to await until it ready.
        /// </summary>
        /// <param name="awaitable">An object to await.</param>
        /// <param name="onReady">A callback which called when awaitable object is ready.</param>
        void Await(IAwaitable awaitable, IAction onReady);
    }
}