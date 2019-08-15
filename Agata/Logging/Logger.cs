namespace Agata.Logging
{
    public static class Logger
    {
        public static ILogger Create<T>()
        {
            return NullLogger.Instance;
        }
    }
}