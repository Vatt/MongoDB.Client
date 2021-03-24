using Microsoft.Extensions.Logging;
using MongoDB.Client.Authentication;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Network;
using MongoDB.Client.Network.Transport.Sockets.Internal;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal class MongoConnectionFactory : IMongoConnectionFactory
    {
        internal static int CONNECTION_ID = 0;
        private readonly NetworkConnectionFactory _networkFactory;
        private readonly EndPoint _endPoint;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MongoConnectionFactory> _logger;
        public MongoConnectionFactory(EndPoint endPoint, ILoggerFactory loggerFactory)
        {
            _networkFactory = new NetworkConnectionFactory(loggerFactory);
            _endPoint = endPoint;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<MongoConnectionFactory>();
        }

        public async ValueTask<MongoConnection> CreateAsync(MongoClientSettings settings, ScramAuthenticator authenticator, ChannelReader<MongoRequest> reader, ChannelReader<MongoRequest> findReader, MongoScheduler requestScheduler, CancellationToken token)
        {
            var context = await _networkFactory.ConnectAsync(_endPoint, token).ConfigureAwait(false);
            if (context is null)
            {
                ThrowHelper.ConnectionException<SocketConnection>(_endPoint);
            }
            var id = Interlocked.Increment(ref CONNECTION_ID);
            var connection = new MongoConnection(id, settings, _loggerFactory.CreateLogger<MongoConnection>(), reader, findReader, requestScheduler);
            await connection.StartAsync(authenticator, context, token).ConfigureAwait(false);
            _logger.LogInformation("Created new connection: {id}", id);
            return connection;
        }
    }
}
