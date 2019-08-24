namespace Agata.Network.Tcp
{
    /// <summary>
    /// Represents a set of TCP server settings.
    /// </summary>
    public sealed class TcpServerSettings
    {
        private TcpServerSettings(int backlog)
        {
            Backlog = backlog;
        }

        /// <summary>
        /// A set of default TCP server settings.
        /// </summary>
        public static readonly TcpServerSettings Default = new TcpServerSettings(64); 

        /// <summary>
        /// The maximum length of the pending connections queue.
        /// </summary>
        public readonly int Backlog;

        /// <summary>
        /// Overrides maximum length of the pending connections queue.
        /// </summary>
        public TcpServerSettings OverrideBacklog(int value)
        {
            return new TcpServerSettings(value);
        }
    }
}