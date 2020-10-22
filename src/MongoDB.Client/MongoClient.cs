using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;

namespace MongoDB.Client
{
    public class MongoClient
    {
        private readonly EndPoint _endPoint;
        private MongoDBConnectionInfo _configMessage;
        private readonly Channel _channel;

        public MongoClient(EndPoint? endPoint = null)
        {

            _endPoint = endPoint ?? new DnsEndPoint("localhost", 27017);
            _channel = new Channel(_endPoint);
        }

        public ValueTask<MongoDBConnectionInfo> ConnectAsync(CancellationToken cancellationToken)
        {
            if (_configMessage is not null)
            {
                return new ValueTask<MongoDBConnectionInfo>(_configMessage);

            }

            return Slow(cancellationToken);

            async ValueTask<MongoDBConnectionInfo> Slow(CancellationToken cancellationToken)
            {
                _configMessage = await _channel.SendHelloAsync(cancellationToken);
                return _configMessage;
            }
        }

        //public ValueTask<T> SendAsync<T>(ReadOnlySpan<byte> message, CancellationToken cancellationToken)
        //{
        //    return _channel.SendAsync<T>(message, cancellationToken);
        //}
    }
}
