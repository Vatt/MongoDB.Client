using System.Net;
using Microsoft.AspNetCore.Connections;
using MongoDB.Client.Authentication;
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
    internal class MongoServiceConnection : IMongoConnection
    {
        private static MongoPingMesageReader MongoPingMessageReader = new MongoPingMesageReader();
        private static BsonDocument _pingDocument = new BsonDocument("isMaster", 1);
        private const string AdminDatabase = "admin.$cmd";
        private static readonly BsonDocument BuildInfo = new BsonDocument("buildInfo", 1);
        internal ConnectionInfo? ConnectionInfo;
        private readonly ProtocolReader _protocolReader;
        private readonly ProtocolWriter _protocolWriter;
        private readonly ConnectionContext _ctx;
        private CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private int _requestId = 0;
        public EndPoint EndPoint { get; }
        public MongoServiceConnection(ConnectionContext connection)
        {
            _ctx = connection;
            EndPoint = connection.RemoteEndPoint!;
            _ctx = connection;
            _protocolReader = connection.CreateReader();
            _protocolWriter = connection.CreateWriter();
        }

        private int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _requestId);
        }

        public async ValueTask<MongoPingMessage> MongoPing(CancellationToken token)
        {
            var message = new QueryMessage(GetNextRequestNumber(), AdminDatabase, _pingDocument);
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

        public async ValueTask Connect(ScramAuthenticator authenticator, MongoClientSettings settings, CancellationToken token)
        {
            ConnectionInfo = await DoConnectAsync(token).ConfigureAwait(false);

            async Task<ConnectionInfo> DoConnectAsync(CancellationToken token)
            {
                var initialDocument = InitHelper.CreateInitialCommand(settings);
                var saslStart = authenticator.AuthenticateIsMaster(initialDocument);

                var isMasterQueryResult = await SendQueryAsync<BsonDocument>(AdminDatabase, initialDocument, token).ConfigureAwait(false);
                var isMaster = isMasterQueryResult[0];
                var buildInfoQueryResult = await SendQueryAsync<BsonDocument>(AdminDatabase, BuildInfo, token).ConfigureAwait(false);
                var buildInfo = buildInfoQueryResult[0];

                await authenticator.AuthenticateAsync(this, isMaster, saslStart, token).ConfigureAwait(false);

                return new ConnectionInfo(isMaster, buildInfo);
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

        public async ValueTask<QueryResult<TResp>> SendQueryAsync<TResp>(string database, BsonDocument document, CancellationToken token)
            where TResp : IBsonSerializer<TResp>
        {
            if (_protocolWriter is null)
            {
                ThrowHelper.ThrowNotInitialized();
            }

            var message = new QueryMessage(GetNextRequestNumber(), database, document);

            await _protocolWriter.WriteAsync(ProtocolWriters.QueryMessageWriter, message, token).ConfigureAwait(false);
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
                where T : IBsonSerializer<T>
            {
                switch (mongoResponse)
                {
                    case ReplyMessage replyMessage:
                        var bodyReader = new ReplyBodyReader<T>(replyMessage);
                        var bodyResult = await reader.ReadAsync(bodyReader, default).ConfigureAwait(false);
                        reader.Advance();
                        return bodyResult.Message;
                    default:
                        return ThrowHelper.UnsupportedTypeException<QueryResult<T>>(typeof(T));
                }
            }
        }
        public async ValueTask DisposeAsync()
        {
            _shutdownCts.Cancel();
            await _protocolReader.DisposeAsync().ConfigureAwait(false);
            await _protocolWriter.DisposeAsync().ConfigureAwait(false);
            await _ctx.DisposeAsync().ConfigureAwait(false);
        }
    }
}
