using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Connections;
using System.Net;
using System.Threading.Channels;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Network.Transport.Sockets.Internal;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Connection
{
    internal class MongoConnectionFactory : IMongoConnectionFactory
    {
        internal static int CONNECTION_ID = 0;
        private readonly Func<EndPoint, CancellationToken, ValueTask<ConnectionContext>> _connectAsync;
        private readonly EndPoint _endPoint;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MongoConnectionFactory> _logger;
        public MongoConnectionFactory(EndPoint endPoint, ILoggerFactory loggerFactory)
            : this(endPoint, loggerFactory, new Network.NetworkConnectionFactory(loggerFactory).ConnectAsync)
        {
        }

        internal MongoConnectionFactory(
            EndPoint endPoint,
            ILoggerFactory loggerFactory,
            Func<EndPoint, CancellationToken, ValueTask<ConnectionContext>> connectAsync)
        {
            _endPoint = endPoint;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<MongoConnectionFactory>();
            _connectAsync = connectAsync;
        }

        public async ValueTask<MongoConnection> CreateAsync(MongoClientSettings settings, IMongoConnectionInitializer initializer, ChannelReader<MongoRequest> reader, MongoScheduler requestScheduler, CancellationToken token)
        {
            ConnectionContext? context = null;
            MongoConnection? connection = null;

            try
            {
                context = await _connectAsync(_endPoint, token).ConfigureAwait(false);
                if (context is null)
                {
                    ThrowHelper.ConnectionException<SocketConnection>(_endPoint);
                }

                var id = Interlocked.Increment(ref CONNECTION_ID);
                connection = new MongoConnection(id, settings, _loggerFactory.CreateLogger<MongoConnection>(), reader, requestScheduler);
                var protocolReader = context.CreateReader();
                var protocolWriter = context.CreateWriter();
                connection.TakeTransportOwnership(context);
                context = null;
                await connection.StartAsync(initializer, protocolReader, protocolWriter, token).ConfigureAwait(false);
                _logger.LogInformation("Created new connection: {id}", id);
                return connection;
            }
            catch
            {
                if (connection is not null)
                {
                    await connection.DisposeAsync().ConfigureAwait(false);
                }

                if (context is not null)
                {
                    await context.DisposeAsync().ConfigureAwait(false);
                }

                throw;
            }
        }
    }
}
