using System;
using System.Net.Sockets;
using Agata.Collections;
using Agata.Concurrency.Actors;
using Agata.Logging;

namespace Agata.Network.Tcp
{
    internal class TcpServerConnection
    {
        private static readonly ILog Log = Logging.Log.For<TcpServerConnection>();

        private readonly TcpServer _server;
        private readonly long _number;
        private readonly Socket _socket;
        private readonly ActorRef<TcpPeer> _peer;
        private readonly FixedArrayPool<byte> _buffers;
        private readonly SocketAsyncEventArgs _receive;
        private volatile bool _connected;

        internal static TcpServerConnection Accept(
            TcpServer server,
            long number,
            Socket socket,
            ActorRef<TcpPeer> peer,
            FixedArrayPool<byte> buffers)
        {
            return new TcpServerConnection(server, number, socket, peer, buffers);
        }

        private TcpServerConnection(
            TcpServer server,
            long number,
            Socket socket,
            ActorRef<TcpPeer> peer,
            FixedArrayPool<byte> buffers)
        {
            _server = server;
            _number = number;
            _socket = socket;
            _peer = peer;
            _buffers = buffers;
            _connected = true;

            _receive = new SocketAsyncEventArgs();
            _receive.Completed += OnReceiveCompleted;

            StartReceive(socket, _receive);
        }

        private void StartReceive(Socket socket, SocketAsyncEventArgs e)
        {
            if (!_connected)
            {
                return;
            }

            var buffer = _buffers.Get();
            e.SetBuffer(buffer, 0, buffer.Length);
            if (!socket.ReceiveAsync(e))
            {
                ProcessReceive(socket, e);
            }
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(_socket, e);
        }

        private void ProcessReceive(Socket socket, SocketAsyncEventArgs e)
        {
            if (!_connected)
            {
                return;
            }

            var receivedDataSize = e.BytesTransferred;
            var receivedData = e.Buffer;
            if (receivedDataSize > 0)
            {
                _peer.Schedule(peer => OnDataReceived(peer, receivedData, receivedDataSize));
            }

            if (e.SocketError == SocketError.Success && receivedDataSize > 0)
            {
                StartReceive(socket, e);
                return;
            }

            Disconnect(e.SocketError);
        }

        private void OnDataReceived(TcpPeer peer, byte[] buffer, int size)
        {
            if (!_connected)
            {
                return;
            }

            try
            {
                peer.OnReceive(buffer, 0, size);
            }
            finally
            {
                _buffers.Return(buffer);
            }
        }

        private void Disconnect(SocketError socketError)
        {
            if (!_connected)
            {
                return;
            }

            _connected = false;

            if (!TcpServer.IsDisconnectError(socketError))
            {
                var error =
                    "Close socket with unexpected error (" +
                    $"endpoint={_server.EndPoint}," +
                    $"connection_number={_number}): " +
                    $"{socketError}";

                Log.Error(error);
            }
            else if (Log.IsDebugEnabled)
            {
                var msg =
                    "Close socket (" +
                    $"endpoint={_server.EndPoint}," +
                    $"connection_number={_number})";

                Log.Debug(msg);
            }

            _peer.Kill(peer => peer.OnDisconnect());
            _server.RemoveConnection(_number);
            _receive.Completed -= OnReceiveCompleted;

            ShutdownSocket();
            DisposeSocket();
        }

        private void ShutdownSocket()
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                var error =
                    "Unexpected error on shutdown socket (" +
                    $"endpoint={_server.EndPoint}," +
                    $"connection_number={_number}): " +
                    $"{e.Message}" +
                    $"{Environment.NewLine}" +
                    $"{e.StackTrace}";

                Log.Error(error);
            }
        }

        private void DisposeSocket()
        {
            try
            {
                _socket.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception e)
            {
                var error =
                    "Unexpected error on dispose socket (" +
                    $"endpoint={_server.EndPoint}," +
                    $"connection_number={_number}): " +
                    $"{e.Message}" +
                    $"{Environment.NewLine}" +
                    $"{e.StackTrace}";

                Log.Error(error);
            }
        }
    }
}