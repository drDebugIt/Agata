using System;

namespace Agata
{
    /// <summary>
    /// Represents a computation that may either result in an exception, or return a successfully computed value. 
    /// </summary>
    /// <typeparam name="T">Type of computed value.</typeparam>
    public struct Try<T>
    {
        private readonly T _value;
        private readonly Exception _error;

        /// <summary>
        /// Creates a result of successful computation. 
        /// </summary>
        /// <param name="value">Computation result value.</param>
        /// <returns>A result of successful computation.</returns>
        public static Try<T> Success(T value)
        {
            return new Try<T>(value, null);
        }

        /// <summary>
        /// Creates a result of failed computation. 
        /// </summary>
        /// <param name="error">Computation error.</param>
        /// <returns>A result of failed computation.</returns>
        public static Try<T> Fail(Exception error)
        {
            return new Try<T>(default(T), error);
        }

        private Try(T value, Exception error)
        {
            _value = value;
            _error = error;
        }

        /// <summary>
        /// Determines is computation result was successful.
        /// </summary>
        public bool IsSuccess => !IsFail;

        /// <summary>
        /// Determines is computation result was failed.
        /// </summary>
        public bool IsFail => _error != null;

        /// <summary>
        /// A result of successful computation.
        /// If this computation is failed throws an exception. 
        /// </summary>
        public T Value
        {
            get
            {
                if (IsFail)
                {
                    throw Error;
                }

                return _value;
            }
        }

        /// <summary>
        /// An error of failed computation result. 
        /// </summary>
        public Exception Error
        {
            get
            {
                if (IsSuccess)
                {
                    throw new InvalidOperationException("Can't get value from Success.");
                }

                return _error;
            }
        }
    }
}