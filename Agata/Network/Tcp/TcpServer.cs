using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Agata.Collections;
using Agata.Concurrency;
using Agata.Concurrency.Actors;
using Agata.Logging;

namespace Agata.Network.Tcp
{
    /// <summary>
    /// Represents a TCP server.
    /// </summary>
    public class TcpServer
    {
        private static readonly ILog Log = Logging.Log.For<TcpServer>();

        private readonly ActorSystem _actorSystem;
        private readonly Socket _socket;
        private readonly FixedArrayPool<byte> _receiveBufferPool;
        private readonly ConcurrentDictionary<long, TcpServerConnection> _connections;
        private long _tcpServerConnectionNumber;

        public static TcpServer Start<T>(
            string ip,
            int port,
            IThreadPool threadPool,
            Func<T> peerFactory,
            TcpServerSettings settings = null)
            where T : TcpPeer
        {
            return Start(IPAddress.Parse(ip), port, threadPool, peerFactory, settings);
        }

        public static TcpServer Start<T>(
            IPAddress ip,
            int port,
            IThreadPool threadPool,
            Func<T> peerFactory,
            TcpServerSettings settings = null)
            where T : TcpPeer
        {
            var endPoint = new IPEndPoint(ip, port);
            return new TcpServer(endPoint, threadPool, peerFactory, settings);
        }

        public static TcpServer Start<T>(
            EndPoint endPoint,
            IThreadPool threadPool,
            Func<T> peerFactory,
            TcpServerSettings settings = null)
            where T : TcpPeer
        {
            return new TcpServer(endPoint, threadPool, peerFactory, settings);
        }

        private TcpServer(
            EndPoint endPoint,
            IThreadPool threadPool,
            Func<TcpPeer> peerFactory,
            TcpServerSettings settings)
        {
            try
            {
                EndPoint = endPoint;
                Settings = settings ?? TcpServerSettings.Default;
                var acceptArgs = new SocketAsyncEventArgs();
                acceptArgs.Completed += OnAcceptCompleted;
                _connections = new ConcurrentDictionary<long, TcpServerConnection>();
                _actorSystem = new ActorSystem(EndPoint.ToString(), threadPool);
                _actorSystem.RegisterFactoryOf(peerFactory);
                _tcpServerConnectionNumber = 0;
                _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _socket.Bind(endPoint);
                _socket.Listen(Settings.Backlog);
                _receiveBufferPool = new FixedArrayPool<byte>(Settings.ReceiveBufferSize, 10000);
                StartAccept(_socket, acceptArgs);
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

        /// <summary>
        /// Endpoint of this TCP server. 
        /// </summary>
        public readonly EndPoint EndPoint;

        /// <summary>
        /// Setting of this TCP server.
        /// </summary>
        public readonly TcpServerSettings Settings;

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
                AcceptConnection(e.AcceptSocket);
            }
            else
            {
                OnError(error);
            }

            StartAccept(socket, e);
        }

        private void AcceptConnection(Socket socket)
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, Settings.KeepAlive);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, Settings.NoDelay);

            var connectionNumber = Interlocked.Increment(ref _tcpServerConnectionNumber);
            var peerName = $"{EndPoint.ToString().ToLowerInvariant()}-{connectionNumber}";
            var peer = _actorSystem.ActorOf<TcpPeer>(peerName);
            var connection = TcpServerConnection.Accept(this, connectionNumber, socket, peer, _receiveBufferPool);
            _connections.TryAdd(connectionNumber, connection);
        }

        internal void RemoveConnection(long number)
        {
            _connections.TryRemove(number, out _);
        }

        private void OnError(SocketError socketError)
        {
            if (IsDisconnectError(socketError))
            {
                return;
            }

            var error = $"TCP server unexpected error (endpoint={EndPoint}): {socketError}";
            Log.Error(error);
        }

        internal static bool IsDisconnectError(SocketError socketError)
        {
            return socketError == SocketError.ConnectionAborted ||
                   socketError == SocketError.ConnectionRefused ||
                   socketError == SocketError.ConnectionReset ||
                   socketError == SocketError.OperationAborted ||
                   socketError == SocketError.Shutdown;
        }
    }
}