namespace Agata.Concurrency
{
    /// <summary>
    /// Call back of Future execution.
    /// </summary>
    /// <param name="result">A result of future execution.</param>
    /// <typeparam name="T">A type of future execution result value.</typeparam>
    public delegate void FutureCallback<T>(Try<T> result);
}