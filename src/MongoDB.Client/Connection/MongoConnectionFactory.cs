using System;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Messages;
using MongoDB.Client.Network;

namespace MongoDB.Client.Connection
{
    public class MongoConnectionFactory
    {
        private static int CONNECTION_ID = 0;
        private readonly NetworkConnectionFactory _networkFactory;
        private readonly EndPoint _endPoint;
        private readonly ILoggerFactory _loggerFactory;
        private ConnectionScheduler _scheduler;
        public MongoConnectionFactory(EndPoint endPoint, ILoggerFactory loggerFactory)
        {
            _networkFactory = new NetworkConnectionFactory();
            _endPoint = endPoint;
            _loggerFactory = loggerFactory;
        }

        public async ValueTask<MongoConnection> Create(ChannelReader<ManualResetValueTaskSource<IParserResult>> reader)
        {
            var connection = await _networkFactory.ConnectAsync(_endPoint);
            if (connection is null)
            {
                ThrowHelper.ConnectionException<System.Net.Connections.Connection>(_endPoint);
            }
            var id = Interlocked.Increment(ref CONNECTION_ID);
            return new MongoConnection(id, _loggerFactory.CreateLogger(String.Empty), reader);
        }
    }
}