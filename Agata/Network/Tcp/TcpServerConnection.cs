using System.Net.Sockets;

namespace Agata.Network.Tcp
{
    internal class TcpServerConnection
    {
        private readonly long _number;
        private readonly Socket _socket;

        internal TcpServerConnection(long number, Socket socket)
        {
            _number = number;
            _socket = socket;
        }
        
        
    }
}