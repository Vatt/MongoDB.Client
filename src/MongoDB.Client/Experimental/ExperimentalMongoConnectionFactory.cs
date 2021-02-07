using Microsoft.Extensions.Logging;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using System;
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
            _networkFactory = new SocketsConnectionFactory(System.Net.Sockets.AddressFamily.InterNetwork,System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            _endPoint = endPoint;
            _loggerFactory = loggerFactory;
        }

        public async ValueTask<MongoConnection> CreateAsync(MongoClientSettings settings, ChannelReader<MongoRequest> reader, ChannelReader<MongoRequest> findReader)
        {
            var context = await _networkFactory.ConnectAsync(_endPoint).ConfigureAwait(false);
            if (context is null)
            {
                ThrowHelper.ConnectionException<SocketConnection>(_endPoint);
            }
            var id = Interlocked.Increment(ref CONNECTION_ID);
            var connection = new MongoConnection(id, settings, _loggerFactory.CreateLogger<MongoConnection>(), reader, findReader);
            await connection.StartAsyncExperimental(context).ConfigureAwait(false);
            return connection;
        }
    }
}
