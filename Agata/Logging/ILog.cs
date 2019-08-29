namespace Agata.Logging
{
    /// <summary>
    /// Provides a set of methods for logging purposes. 
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Determines is debug logging enabled.
        /// </summary>
        bool IsDebugEnabled { get; }

        /// <summary>
        /// Logs specified message with debug severity.
        /// </summary>
        void Debug(string message);

        /// <summary>
        /// Logs specified message with warning severity.
        /// </summary>
        void Warning(string message);

        /// <summary>
        /// Logs specified message with error severity.
        /// </summary>
        void Error(string message);
    }
    
    
}