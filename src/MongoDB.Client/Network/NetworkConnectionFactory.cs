using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Client.Network.Transport.Sockets;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
namespace MongoDB.Client.Network
{
    public class NetworkConnectionFactory
    {
        IConnectionFactory _factory;

        public NetworkConnectionFactory(ILoggerFactory loggerFactory)
        {
            _factory = new SocketConnectionFactory(Options.Create(new SocketTransportOptions()), loggerFactory);
        }

        public ValueTask<ConnectionContext> ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken = default)
        {
            Debug.Assert(endPoint != null, nameof(endPoint) + " != null");
            return _factory.ConnectAsync(endPoint, cancellationToken);
        }
    }
}
