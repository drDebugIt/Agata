namespace Agata.Concurrency
{
    /// <summary>
    /// Describes an interface of something that should be ready for next processing step.
    /// </summary>
    public interface IAwaitable
    {
        /// <summary>
        /// Determines is this object ready to next processing step.
        /// </summary>
        bool Ready { get; }
    }
}