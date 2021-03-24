using Microsoft.AspNetCore.Connections;
using MongoDB.Client.Authentication;
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
    public sealed partial class MongoConnection : IMongoConnection
    {
        private const string AdminDatabase = "admin.$cmd";
        private static readonly BsonDocument BuildInfo = new BsonDocument("buildInfo", 1);

        internal ConnectionInfo ConnectionInfo;


        internal ValueTask<ConnectionInfo> StartAsync(ScramAuthenticator authenticator, ConnectionContext connection, CancellationToken cancellationToken = default)
        {
            return StartAsync(authenticator, connection.CreateReader(), connection.CreateWriter(), cancellationToken);
        }


        internal async ValueTask<ConnectionInfo> StartAsync(ScramAuthenticator authenticator, ProtocolReader reader, ProtocolWriter writer, CancellationToken cancellationToken)
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
                var initialDocument = InitHelper.CreateInitialCommand(_settings);
                var saslStart = authenticator.AuthenticateIsMaster(initialDocument);

                var isMasterQueryResult = await SendQueryAsync<BsonDocument>(AdminDatabase, initialDocument, token).ConfigureAwait(false);
                var isMaster = isMasterQueryResult[0];
                var buildInfoQueryResult = await SendQueryAsync<BsonDocument>(AdminDatabase, BuildInfo, token).ConfigureAwait(false);
                var buildInfo = buildInfoQueryResult[0];

                await authenticator.AuthenticateAsync(this, isMaster, saslStart, token).ConfigureAwait(false);

                return new ConnectionInfo(isMaster, buildInfo);
            }
        }


        private QueryMessage CreateQueryRequest(string database, BsonDocument document)
        {
            return new QueryMessage(GetNextRequestNumber(), database, document);
        }


        private QueryMessage CreateQueryRequest(BsonDocument document)
        {
            return CreateQueryRequest(AdminDatabase, document);
        }

        public async ValueTask<QueryResult<TResp>> SendQueryAsync<TResp>(string database, BsonDocument document, CancellationToken cancellationToken)
        {
            if (_protocolWriter is null)
            {
                ThrowHelper.ThrowNotInitialized();
            }

            var message = new QueryMessage(GetNextRequestNumber(), database, document);

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
