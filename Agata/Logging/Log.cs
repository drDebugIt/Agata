namespace Agata.Logging
{
    public static class Log
    {
        public static ILog For<T>()
        {
            return NullLog.Instance;
        }
    }
}