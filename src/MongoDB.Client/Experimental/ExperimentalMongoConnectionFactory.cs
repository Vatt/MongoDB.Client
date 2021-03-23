using Microsoft.Extensions.Logging;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;
using System.Net;
using System.Net.Connections;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Experimental
{
    internal class ExperimentalMongoConnectionFactory : IMongoConnectionFactory
    {
        internal static int CONNECTION_ID = 0;
        private readonly SocketsConnectionFactory _networkFactory;
        private readonly EndPoint _endPoint;
        private readonly ILoggerFactory _loggerFactory;
        public ExperimentalMongoConnectionFactory(EndPoint endPoint, ILoggerFactory loggerFactory)
        {
            _networkFactory = new SocketsConnectionFactory(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            _endPoint = endPoint;
            _loggerFactory = loggerFactory;
        }

        public async ValueTask<MongoConnection> CreateAsync(MongoClientSettings settings, ChannelReader<MongoRequest> reader, ChannelReader<MongoRequest> findReader, MongoScheduler requestScheduler, CancellationToken token)
        {
            var context = await _networkFactory.ConnectAsync(_endPoint, cancellationToken: token).ConfigureAwait(false);
            if (context is null)
            {
                ThrowHelper.ConnectionException<SocketConnection>(_endPoint);
            }
            var id = Interlocked.Increment(ref CONNECTION_ID);
            var connection = new MongoConnection(id, settings, _loggerFactory.CreateLogger<MongoConnection>(), reader, findReader, requestScheduler);
            var connectionInfo = await connection.StartAsyncExperimental(context, token).ConfigureAwait(false);
            return connection;
        }
    }
}
