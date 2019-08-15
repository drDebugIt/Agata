using System;

namespace Agata.Concurrency
{
    /// <summary>
    /// Represents a promise of something that will be calculated in future.
    /// </summary>
    /// <typeparam name="T">A type of calculation result.</typeparam>
    public sealed class Promise<T>
    {
        private readonly Future<T> _future = new Future<T>();

        /// <summary>
        /// A future of this promise.
        /// </summary>
        public Future<T> Future => _future;
        
        /// <summary>
        /// Finishes this promise with success result.
        /// </summary>
        /// <param name="result">A result of calculation.</param>
        public void Success(T result)
        {
            _future.Success(result);
        }

        /// <summary>
        /// Finishes this promise with error result.
        /// </summary>
        /// <param name="error">An error of calculation.</param>
        public void Fail(Exception error)
        {
            _future.Fail(error);
        }
    }
}