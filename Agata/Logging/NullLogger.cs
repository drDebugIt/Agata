namespace Agata.Logging
{
    internal sealed class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new NullLogger();
        
        private NullLogger()
        {
        }
        
        public void Error(string message)
        {
        }
    }
}