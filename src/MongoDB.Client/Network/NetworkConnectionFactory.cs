using Microsoft.AspNetCore.Connections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
namespace MongoDB.Client.Network
{
    public class NetworkConnectionFactory : IConnectionFactory
    {
        public async ValueTask<ConnectionContext> ConnectAsync(EndPoint? endPoint,  CancellationToken cancellationToken = default)
        {
            Debug.Assert(endPoint != null, nameof(endPoint) + " != null");
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(endPoint, cancellationToken);
            if (!socket.Connected)
            {
                return null;
            }
            var ns = new NetworkStream(socket);
            return default;// System.Net.Connections.Connection.FromStream(ns, localEndPoint: socket.LocalEndPoint, remoteEndPoint: socket.RemoteEndPoint);
        }
    }
}
