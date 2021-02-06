using Microsoft.Extensions.Logging;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Network;
using MongoDB.Client.Network.Transport.Sockets.Internal;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal class MongoConnectionFactory
    {
        internal static int CONNECTION_ID = 0;
        private readonly NetworkConnectionFactory _networkFactory;
        private readonly EndPoint _endPoint;
        private readonly ILoggerFactory _loggerFactory;
        public MongoConnectionFactory(EndPoint endPoint, ILoggerFactory loggerFactory)
        {
            _networkFactory = new NetworkConnectionFactory(loggerFactory);
            _endPoint = endPoint;
            _loggerFactory = loggerFactory;
        }

        public async ValueTask<MongoConnection> Create(MongoClientSettings settings, ChannelReader<MongoRequest> reader, ChannelReader<MongoRequest> findReader)
        {
            var context = await _networkFactory.ConnectAsync(_endPoint).ConfigureAwait(false);
            if (context is null)
            {
                ThrowHelper.ConnectionException<SocketConnection>(_endPoint);
            }
            var id = Interlocked.Increment(ref CONNECTION_ID);
            var connection = new MongoConnection(id, settings, _loggerFactory.CreateLogger<MongoConnection>(), reader, findReader);
            await connection.StartAsync(context).ConfigureAwait(false);
            return connection;
        }
    }
}