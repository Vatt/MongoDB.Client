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
        internal ConnectionInfo ConnectionInfo;


        internal ValueTask<ConnectionInfo> StartAsync(ConnectionContext connection, CancellationToken cancellationToken = default)
        {
            return StartAsync(connection.CreateReader(), connection.CreateWriter(), cancellationToken);
        }


        internal async ValueTask<ConnectionInfo> StartAsync(ProtocolReader reader, ProtocolWriter writer, CancellationToken cancellationToken)
        {
            _protocolReader = reader;
            _protocolWriter = writer;
            _protocolListenerTask = StartProtocolListenerAsync();
            ConnectionInfo = await DoConnectAsync(cancellationToken).ConfigureAwait(false);



            _channelListenerTask = StartChannelListerAsync();
            _channelFindListenerTask = StartFindChannelListerAsync();
            return ConnectionInfo;
            async Task<ConnectionInfo> DoConnectAsync(CancellationToken token)
            {
                var (initialDocument, saslStart) = InitHelper.CreateInitialCommand(_settings);
                var connectRequest = CreateQueryRequest(initialDocument, GetNextRequestNumber());
                var isMasterQueryResult = await SendQueryAsync<BsonDocument>(connectRequest, token).ConfigureAwait(false);
                var isMaster = isMasterQueryResult[0];
                var buildInfoRequest = CreateQueryRequest(new BsonDocument("buildInfo", 1), GetNextRequestNumber());
                var buildInfoQueryResult = await SendQueryAsync<BsonDocument>(buildInfoRequest, token).ConfigureAwait(false);
                var buildInfo = buildInfoQueryResult[0];

                if (isMaster.TryGet("speculativeAuthenticate", out var authDataElement))
                {
                    var authData = authDataElement.AsBsonDocument!;
                    var payloadBinaryData = (BsonBinaryData)authData["payload"].Value!;
                    var payload = (byte[]) payloadBinaryData.Value;

                    var conversationId = authData["conversationId"].AsInt;
                    var (saslDoc, serverSignature) = InitHelper.CreateSaslStart(_settings, payload, saslStart!, conversationId);
                    var result = await SendQueryAsync<BsonDocument>(CreateQueryRequest(saslDoc, GetNextRequestNumber()), token).ConfigureAwait(false);
                    var isOk = (double)result[0]["ok"].Value!;
                    if (isOk == 0)
                    {
                        ThrowHelper.MongoAuthentificationException(result[0]["errmsg"].ToString(), (int)result[0]["code"].Value!);
                    }
                }

                return new ConnectionInfo(isMaster, buildInfo);
            }
        }


        private QueryMessage CreateQueryRequest(string database, BsonDocument document, int number)
        {
            return new QueryMessage(number, database, document);
        }


        private QueryMessage CreateQueryRequest(BsonDocument document, int number)
        {
            return CreateQueryRequest("admin.$cmd", document, number);
        }


        public async ValueTask<QueryResult<TResp>> SendQueryAsync<TResp>(QueryMessage message, CancellationToken cancellationToken)
        {
            if (_protocolWriter is null)
            {
                ThrowHelper.ThrowNotInitialized();
            }
            ManualResetValueTaskSource<IParserResult> taskSource;
            if (_queue.TryDequeue(out var taskSrc))
            {
                taskSource = taskSrc;
            }
            else
            {
                taskSource = new ManualResetValueTaskSource<IParserResult>();
            }

            var completion = new MongoRequest(taskSource)
            {
                RequestNumber = message.RequestNumber,
                ParseAsync = ParseAsync<TResp>
            };
            _completions.GetOrAdd(completion.RequestNumber, completion);
            try
            {
                await _protocolWriter.WriteUnsafeAsync(ProtocolWriters.QueryMessageWriter, message, cancellationToken).ConfigureAwait(false);
                var response = await new ValueTask<IParserResult>(completion.CompletionSource, completion.CompletionSource.Version).ConfigureAwait(false);

                if (response is QueryResult<TResp> queryResult)
                {
                    return queryResult;
                }

                return ThrowHelper.InvalidReturnType<QueryResult<TResp>>(typeof(QueryResult<TResp>), response.GetType());
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
                        var bodyReader = new ReplyBodyReader<T>((IGenericBsonSerializer<T>)new BsonDocumentSerializer(), replyMessage);
                        var bodyResult = await reader.ReadAsync(bodyReader, default).ConfigureAwait(false);
                        reader.Advance();
                        return bodyResult.Message;
                    default:
                        return ThrowHelper.UnsupportedTypeException<QueryResult<T>>(typeof(T));
                }
            }
        }
    }
}
