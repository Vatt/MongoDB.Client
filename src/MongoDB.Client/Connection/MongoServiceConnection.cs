using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Connection
{
    internal class MongoServiceConnection
    {
        private static MongoPingMesageReader MongoPingMessageReader = new MongoPingMesageReader();
        private static BsonDocument _pingDocument = new BsonDocument("isMaster", 1);
        internal ConnectionInfo ConnectionInfo;
        private ProtocolReader _protocolReader;
        private ProtocolWriter _protocolWriter;
        private CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private int _requestId = 0;
        public MongoServiceConnection(ConnectionContext connection)
        {
            _protocolReader = connection.CreateReader();
            _protocolWriter = connection.CreateWriter();
        }

        private int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _requestId);
        }
        public async ValueTask<MongoPingMessage> MongoPing(CancellationToken token)
        {
            var message = new QueryMessage(GetNextRequestNumber(), "admin.$cmd", _pingDocument);
            //var test = await SendQueryAsync<BsonDocument>(message, _shutdownCts.Token).ConfigureAwait(false);
            if (_protocolWriter is null)
            {
                ThrowHelper.ThrowNotInitialized();
            }
            await _protocolWriter.WriteAsync(ProtocolWriters.QueryMessageWriter, message, token).ConfigureAwait(false);
            var header = await ReadAsyncPrivate(_protocolReader, ProtocolReaders.MessageHeaderReader, token).ConfigureAwait(false);
            if (header.Opcode != Opcode.Reply)
            {
                //TODO: DO SOME
            }
            var replyResult = await ReadAsyncPrivate(_protocolReader, ProtocolReaders.ReplyMessageReader, token).ConfigureAwait(false);
            var bodyResult = await ReadAsyncPrivate(_protocolReader, MongoPingMessageReader, token).ConfigureAwait(false);
            return bodyResult;
        }
        public async ValueTask Connect(MongoClientSettings settings, CancellationToken token)
        {
            ConnectionInfo = await DoConnectAsync(token).ConfigureAwait(false);
            async Task<ConnectionInfo> DoConnectAsync(CancellationToken token)
            {
                var _initialDocument = InitHelper.CreateInitialCommand(settings);
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
            if (readPreferenceDocument is null)
            {
                return document;
            }
            var doc = new BsonDocument
                {
                    {"$query", document},
                    {"$readPreference", readPreferenceDocument}
                };

            return doc;
        }
        public async ValueTask<QueryResult<TResp>?> SendQueryAsync<TResp>(QueryMessage message, CancellationToken token)
        {
            if (_protocolWriter is null)
            {
                ThrowHelper.ThrowNotInitialized();
            }
            await _protocolWriter.WriteUnsafeAsync(ProtocolWriters.QueryMessageWriter, message, token).ConfigureAwait(false);
            var header = await ReadAsyncPrivate(_protocolReader, ProtocolReaders.MessageHeaderReader, _shutdownCts.Token).ConfigureAwait(false);
            if (header.Opcode != Opcode.Reply)
            {
                //TODO: DO SOME
            }
            var replyResult = await ReadAsyncPrivate(_protocolReader, ProtocolReaders.ReplyMessageReader, _shutdownCts.Token).ConfigureAwait(false);
            MongoResponseMessage replyMessage = new ReplyMessage(header, replyResult);
            var result = await ParseAsync<TResp>(_protocolReader, replyMessage);
            return result as QueryResult<TResp>;
            async ValueTask<IParserResult> ParseAsync<T>(ProtocolReader reader, MongoResponseMessage mongoResponse)
            {
                switch (mongoResponse)
                {
                    case ReplyMessage replyMessage:
                        var bodyReader = new ReplyBodyReader<T>((IGenericBsonSerializer<T>)new BsonDocumentSerializer(), replyMessage);
                        var bodyResult = await reader.ReadAsync(bodyReader, default).ConfigureAwait(false);
                        reader.Advance();
                        return bodyResult.Message;
                    default:
                        return ThrowHelper.UnsupportedTypeException<QueryResult<T>>(typeof(T));
                }
            }
        }
        private static async ValueTask<T> ReadAsyncPrivate<T>(ProtocolReader protocolReader, IMessageReader<T> reader, CancellationToken token)
        {
            var result = await protocolReader.ReadAsync(reader, token).ConfigureAwait(false);
            protocolReader.Advance();
            if (result.IsCanceled || result.IsCompleted)
            {
                //TODO: DO SOME
            }
            return result.Message;
        }
    }
}
