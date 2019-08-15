using System;
using System.Runtime.Serialization;

namespace Agata.Concurrency
{
    /// <summary>
    /// Represents errors that occur during future execution.
    /// </summary>
    public class FutureException : Exception
    {
        /// <summary>
        /// Initializes a new instance of future exception.
        /// </summary>
        public FutureException()
        {
        }

        /// <summary>
        /// Initializes a new instance of future exception.
        /// </summary>
        protected FutureException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of future exception.
        /// </summary>
        public FutureException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of future exception.
        /// </summary>
        public FutureException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}