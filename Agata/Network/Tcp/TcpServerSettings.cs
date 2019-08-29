namespace Agata.Network.Tcp
{
    /// <summary>
    /// Represents a set of TCP server settings.
    /// </summary>
    public sealed class TcpServerSettings
    {
        private TcpServerSettings(int backlog, bool keepAlive, bool noDelay, int receiveBufferSize)
        {
            Backlog = backlog;
            KeepAlive = keepAlive;
            NoDelay = noDelay;
            ReceiveBufferSize = receiveBufferSize;
        }

        /// <summary>
        /// A set of default TCP server settings.
        /// ------------------------------------------------
        /// Backlog:   64 connections
        /// KeepAlive: Enabled
        /// NoDelay:   Disabled
        /// </summary>
        public static readonly TcpServerSettings Default = new TcpServerSettings(
            64, true, false, 1024 * 8);

        /// <summary>
        /// The maximum length of the pending connections queue.
        /// </summary>
        public readonly int Backlog;

        /// <summary>
        /// Determines is TCP keep alive should be used.
        /// </summary>
        public readonly bool KeepAlive;

        /// <summary>
        /// Determines is Nagle algorithm should be disabled.
        /// https://en.wikipedia.org/wiki/Nagle%27s_algorithm
        /// </summary>
        public readonly bool NoDelay;

        /// <summary>
        /// Size of receive buffer.
        /// </summary>
        public readonly int ReceiveBufferSize;

        /// <summary>
        /// Sets maximum length of the pending connections queue.
        /// </summary>
        public TcpServerSettings SetBacklog(int value)
        {
            return new TcpServerSettings(value, KeepAlive, NoDelay, ReceiveBufferSize);
        }

        /// <summary>
        /// Enables TCP KeepAlive.
        /// </summary>
        public TcpServerSettings EnableKeepAlive()
        {
            return new TcpServerSettings(Backlog, true, NoDelay, ReceiveBufferSize);
        }

        /// <summary>
        /// Disables TCP KeepAlive.
        /// </summary>
        public TcpServerSettings DisableKeepAlive()
        {
            return new TcpServerSettings(Backlog, false, NoDelay, ReceiveBufferSize);
        }

        /// <summary>
        /// Disables Nagle algorithm. 
        /// </summary>
        public TcpServerSettings EnableNoDelay()
        {
            return new TcpServerSettings(Backlog, KeepAlive, true, ReceiveBufferSize);
        }

        /// <summary>
        /// Enables Nagle algorithm. 
        /// </summary>
        public TcpServerSettings DisableNoDelay()
        {
            return new TcpServerSettings(Backlog, KeepAlive, false, ReceiveBufferSize);
        }
    }
}