namespace Agata.Network.Tcp
{
    /// <summary>
    /// Represents a base class for TCP peer.
    /// </summary>
    public abstract class TcpPeer
    {
        /// <summary>
        /// Calls when this peer receives some data.
        /// </summary>
        /// <param name="reusableBuffer">Received data buffer. Reference of on this data should not be kept outside this method scope because it reusable.</param>
        /// <param name="offset">Offset in this data in buffer.</param>
        /// <param name="size">Total size of data in buffer.</param>
        public abstract void OnReceive(byte[] reusableBuffer, int offset, int size);

        /// <summary>
        /// Calls when underlying connection closed.
        /// Override this for dispose and free used resources.
        /// </summary>
        public virtual void OnDisconnect()
        {
        }
    }
}