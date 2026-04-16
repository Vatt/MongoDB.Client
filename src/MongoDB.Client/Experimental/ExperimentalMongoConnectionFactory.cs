using System.Net;
using System.Net.Connections;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Experimental
{
    internal class ExperimentalMongoConnectionFactory : IMongoConnectionFactory
    {
        internal static int CONNECTION_ID = 0;
        private readonly Func<EndPoint, CancellationToken, ValueTask<System.Net.Connections.Connection>> _connectAsync;
        private readonly EndPoint _endPoint;
        private readonly ILoggerFactory _loggerFactory;
        public ExperimentalMongoConnectionFactory(EndPoint endPoint, ILoggerFactory loggerFactory)
            : this(
                endPoint,
                loggerFactory,
                (endpoint, cancellationToken) => new SocketsConnectionFactory(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp).ConnectAsync(endpoint, cancellationToken: cancellationToken))
        {
        }

        internal ExperimentalMongoConnectionFactory(
            EndPoint endPoint,
            ILoggerFactory loggerFactory,
            Func<EndPoint, CancellationToken, ValueTask<System.Net.Connections.Connection>> connectAsync)
        {
            _endPoint = endPoint;
            _loggerFactory = loggerFactory;
            _connectAsync = connectAsync;
        }

        public async ValueTask<MongoConnection> CreateAsync(MongoClientSettings settings, IMongoConnectionInitializer initializer, ChannelReader<MongoRequest> reader, MongoScheduler requestScheduler, CancellationToken token)
        {
            System.Net.Connections.Connection? context = null;
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
