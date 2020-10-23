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
        private MongoDBConnectionInfo? _configMessage;
        private BsonDocument? _hell;
        private readonly Channel _channel;

        private static ReadOnlyMemory<byte> Config = new byte[] { 42, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 212, 7, 0, 0, 4, 0, 0, 0, 97, 100, 109, 105, 110, 46, 36, 99, 109, 100, 0, 0, 0, 0, 0, 255, 255, 255, 255, 3, 1, 0, 0, 16, 105, 115, 77, 97, 115, 116, 101, 114, 0, 1, 0, 0, 0, 3, 99, 108, 105, 101, 110, 116, 0, 214, 0, 0, 0, 3, 100, 114, 105, 118, 101, 114, 0, 56, 0, 0, 0, 2, 110, 97, 109, 101, 0, 20, 0, 0, 0, 109, 111, 110, 103, 111, 45, 99, 115, 104, 97, 114, 112, 45, 100, 114, 105, 118, 101, 114, 0, 2, 118, 101, 114, 115, 105, 111, 110, 0, 8, 0, 0, 0, 48, 46, 48, 46, 48, 46, 48, 0, 0, 3, 111, 115, 0, 111, 0, 0, 0, 2, 116, 121, 112, 101, 0, 8, 0, 0, 0, 87, 105, 110, 100, 111, 119, 115, 0, 2, 110, 97, 109, 101, 0, 29, 0, 0, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 87, 105, 110, 100, 111, 119, 115, 32, 49, 48, 46, 48, 46, 49, 57, 48, 52, 49, 0, 2, 97, 114, 99, 104, 105, 116, 101, 99, 116, 117, 114, 101, 0, 7, 0, 0, 0, 120, 56, 54, 95, 54, 52, 0, 2, 118, 101, 114, 115, 105, 111, 110, 0, 11, 0, 0, 0, 49, 48, 46, 48, 46, 49, 57, 48, 52, 49, 0, 0, 2, 112, 108, 97, 116, 102, 111, 114, 109, 0, 16, 0, 0, 0, 46, 78, 69, 84, 32, 67, 111, 114, 101, 32, 51, 46, 49, 46, 57, 0, 0, 4, 99, 111, 109, 112, 114, 101, 115, 115, 105, 111, 110, 0, 5, 0, 0, 0, 0, 0 };
        private static ReadOnlyMemory<byte> Hell = new byte[] { 59, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 212, 7, 0, 0, 4, 0, 0, 0, 97, 100, 109, 105, 110, 46, 36, 99, 109, 100, 0, 0, 0, 0, 0, 255, 255, 255, 255, 20, 0, 0, 0, 16, 98, 117, 105, 108, 100, 73, 110, 102, 111, 0, 1, 0, 0, 0, 0 };

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
                await _channel.ConnectAsync(cancellationToken).ConfigureAwait(false);
                _configMessage = await _channel.SendAsync<MongoDBConnectionInfo>(Config, cancellationToken);
                _hell = await _channel.SendAsync<BsonDocument>(Hell, cancellationToken);
                return _configMessage;
            }
        }

        //public ValueTask<T> SendAsync<T>(ReadOnlySpan<byte> message, CancellationToken cancellationToken)
        //{
        //    return _channel.SendAsync<T>(message, cancellationToken);
        //}
    }
}
