using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Agata.Network.Tcp
{
    public class TcpServer
    {
        private readonly EndPoint _endPoint;
        private readonly TcpServerSettings _settings;
        private readonly SocketAsyncEventArgs _acceptArgs;
        private readonly Socket _socket;
        private readonly ConcurrentDictionary<long, TcpServerConnection> _connections;
        private long _tcpServerConnectionNumber;

        public static TcpServer Start(string ip, int port, TcpServerSettings settings = null)
        {
            return Start(IPAddress.Parse(ip), port, settings);
        }

        public static TcpServer Start(IPAddress ip, int port, TcpServerSettings settings = null)
        {
            var endPoint = new IPEndPoint(ip, port);
            return new TcpServer(endPoint, settings);
        }

        public static TcpServer Start(EndPoint endPoint, TcpServerSettings settings = null)
        {
            return new TcpServer(endPoint, settings);
        }
        
        private TcpServer(EndPoint endPoint, TcpServerSettings settings)
        {
            try
            {
                _endPoint = endPoint;
                _settings = settings ?? TcpServerSettings.Default;
                _acceptArgs = new SocketAsyncEventArgs();
                _acceptArgs.Completed += OnAcceptCompleted;
                _tcpServerConnectionNumber = 0;
                _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _socket.Bind(endPoint);
                _socket.Listen(_settings.Backlog);
                StartAccept(_socket, _acceptArgs);
            }
            catch
            {
                if (_socket != null)
                {
                    _socket.Close();
                    _socket.Dispose();
                }

                throw;
            }
        }

        private void StartAccept(Socket socket, SocketAsyncEventArgs e)
        {
            e.AcceptSocket = null;
            if (!socket.AcceptAsync(e))
            {
                ProcessAccept(socket, e);
            }
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(_socket, e);
        }

        private void ProcessAccept(Socket socket, SocketAsyncEventArgs e)
        {
            var error = e.SocketError;
            if (error == SocketError.Success)
            {
                Register(e.AcceptSocket);
            }
            else
            {
                OnError(error);
            }
            
            StartAccept(socket, e);
        }

        private void Register(Socket socket)
        {
            var connectionNumber = Interlocked.Increment(ref _tcpServerConnectionNumber);
            var connection = new TcpServerConnection(connectionNumber, socket);
            _connections.TryAdd(connectionNumber, connection);
        }
        
        private static void OnError(SocketError error)
        {
            throw new System.NotImplementedException();
        }

    }
}