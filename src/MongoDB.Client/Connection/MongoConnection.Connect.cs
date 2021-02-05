using Microsoft.AspNetCore.Connections;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Protocol.Readers;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection
    {
        private ConnectionInfo? _connectionInfo;
        internal async ValueTask StartAsync(System.Net.Connections.Connection connection, CancellationToken cancellationToken = default)
        {
            _connection = connection;
            _protocolReader = _connection.CreateReader();
            _protocolWriter = _connection.CreateWriter();
            _protocolListenerTask = StartProtocolListenerAsync();
            _connectionInfo = await DoConnectAsync(cancellationToken).ConfigureAwait(false);
            _channelListenerTask = StartChannelListerAsync();
            _channelFindListenerTask = StartFindChannelListerAsync();
            async Task<ConnectionInfo> DoConnectAsync(CancellationToken token)
            {
                var _initialDocument = InitHelper.CreateInitialCommand(_settings);
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
        public async ValueTask<QueryResult<TResp>> SendQueryAsync<TResp>(QueryMessage message, CancellationToken cancellationToken)
        {
            ManualResetValueTaskSource<IParserResult> taskSource;
            if (_queue.TryDequeue(out taskSource) == false)
            {
                taskSource = new ManualResetValueTaskSource<IParserResult>();
            }

            var completion = new MongoRequest(taskSource);
            completion.RequestNumber = message.RequestNumber;
            completion.ParseAsync = ParseAsync<TResp>;
            _completions.GetOrAdd(completion.RequestNumber, completion);
            try
            {
                await _protocolWriter.WriteUnsafeAsync(ProtocolWriters.QueryMessageWriter, message, cancellationToken).ConfigureAwait(false);
                var response = await new ValueTask<IParserResult>(completion.CompletionSource, completion.CompletionSource.Version).ConfigureAwait(false);

                if (response is QueryResult<TResp> queryResult)
                {
                    return queryResult;
                }

                return default!;
            }
            finally
            {
                _completions.TryRemove(message.RequestNumber, out _);
                taskSource.Reset();
                _queue.Enqueue(taskSource);
            }

            async ValueTask<IParserResult> ParseAsync<T>(ProtocolReader reader, MongoResponseMessage mongoResponse)
            {
                switch (mongoResponse)
                {
                    case ReplyMessage replyMessage:
                        var bodyReader = new ReplyBodyReader<T>(new BsonDocumentSerializer() as IGenericBsonSerializer<T>, replyMessage);
                        var bodyResult = await reader.ReadAsync(bodyReader, default).ConfigureAwait(false);
                        reader.Advance();
                        return bodyResult.Message;

                        return ThrowHelper.UnsupportedTypeException<QueryResult<T>>(typeof(T));
                    default:
                        return ThrowHelper.UnsupportedTypeException<QueryResult<T>>(typeof(T));
                }
            }
            return default;
        }
    }
}