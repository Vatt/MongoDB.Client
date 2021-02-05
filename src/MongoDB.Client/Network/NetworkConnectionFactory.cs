using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Connections;
namespace MongoDB.Client.Network
{
    public class NetworkConnectionFactory
    {
        SocketsConnectionFactory _factory;
        public NetworkConnectionFactory(ILoggerFactory loggerFactory)
        {
            _factory = new System.Net.Connections.SocketsConnectionFactory(System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
        }
        public ValueTask<System.Net.Connections.Connection?> ConnectAsync(EndPoint? endPoint, CancellationToken cancellationToken = default)
        {
            Debug.Assert(endPoint != null, nameof(endPoint) + " != null");
            return _factory.ConnectAsync(endPoint);
        }
    }
}
