using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection
    {
        private ConnectionInfo? _connectionInfo;
        private readonly BsonDocument _initialDocument = InitHelper.CreateInitialCommand();
        internal async ValueTask StartAsync(ConnectionContext connection, CancellationToken cancellationToken = default)
        {
            _connection = connection;
            _protocolReader = _connection.CreateReader();
            _protocolWriter = _connection.CreateWriter();
            _protocolListenerTask = StartProtocolListenerAsync();
            _connectionInfo = await DoConnectAsync(cancellationToken).ConfigureAwait(false);
            _channelListenerTask = StartChannelListerAsync();
            async Task<ConnectionInfo> DoConnectAsync(CancellationToken token)
            {
                var connectRequest = CreateQueryRequest(_initialDocument, GetNextRequestNumber());
                var configMessage = await SendQueryAsync<BsonDocument>(connectRequest, token).ConfigureAwait(false);
                var buildInfoRequest = CreateQueryRequest(new BsonDocument("buildInfo", 1), GetNextRequestNumber());
                var hell = await SendQueryAsync<BsonDocument>(buildInfoRequest, token).ConfigureAwait(false);
                return new ConnectionInfo(configMessage[0], hell[0]);
            }
        }
        private QueryMessage CreateQueryRequest(string database, BsonDocument document, int number)
        {
            return new QueryMessage(number, database, document);
        }
        private QueryMessage CreateQueryRequest(BsonDocument document, int number)
        {
            var doc = CreateWrapperDocument(document);
            return CreateQueryRequest("admin.$cmd", doc, number);
        }

        private static BsonDocument CreateWrapperDocument(BsonDocument document)
        {
            BsonDocument? readPreferenceDocument = null;
            var doc = new BsonDocument
                {
                    {"$query", document},
                    {"$readPreference", readPreferenceDocument, readPreferenceDocument != null}
                };

            if (doc.Count == 1)
            {
                return doc["$query"].AsBsonDocument;
            }
            else
            {
                return doc;
            }
        }
    }
}