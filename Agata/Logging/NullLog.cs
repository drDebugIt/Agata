namespace Agata.Logging
{
    internal sealed class NullLog : ILog
    {
        public static readonly NullLog Instance = new NullLog();
        
        private NullLog()
        {
        }

        public bool IsDebugEnabled => false;
        
        public void Debug(string message)
        {
        }

        public void Error(string message)
        {
        }
    }
}