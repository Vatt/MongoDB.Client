using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Protocol.Readers;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection : IMongoConnection
    {
        internal ConnectionInfo? ConnectionInfo;
        internal ValueTask<ConnectionInfo> StartAsync(IMongoConnectionInitializer initializer, ConnectionContext connection, CancellationToken cancellationToken = default)
        {
            return StartAsync(initializer, connection.CreateReader(), connection.CreateWriter(), connection, cancellationToken);
        }



        internal async ValueTask<ConnectionInfo> StartAsync(IMongoConnectionInitializer initializer, ProtocolReader reader, ProtocolWriter writer, CancellationToken cancellationToken)
        {
            await StartAsync(initializer, reader, writer, null, cancellationToken).ConfigureAwait(false);
            return ConnectionInfo!;
        }

        internal async ValueTask<ConnectionInfo> StartAsync(IMongoConnectionInitializer initializer, ProtocolReader reader, ProtocolWriter writer, IAsyncDisposable? owner, CancellationToken cancellationToken)
        {
            _protocolReader = reader;
            _protocolWriter = writer;
            if (owner is not null)
            {
                TakeTransportOwnership(owner);
            }
            _protocolListenerTask = StartProtocolListenerAsync();

            try
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdownCts.Token);
                ConnectionInfo = await initializer.InitializeAsync(this, linkedCts.Token).ConfigureAwait(false);
                _channelListenerTask = StartChannelListerAsync();
                return ConnectionInfo;
            }
            catch
            {
                try
                {
                    await DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error on disposing connection after initialization failure");
                }

                throw;
            }
        }

        public async ValueTask<QueryResult<TResp>> SendQueryAsync<TResp>(string database, BsonDocument document, CancellationToken cancellationToken)
            where TResp : IBsonSerializer<TResp>
        {
            if (_protocolWriter is null)
            {
                ThrowHelper.ThrowNotInitialized();
            }

            var message = new QueryMessage(GetNextRequestNumber(), database, document);

            MongoRequest completion;
            if (!_queue.TryDequeue(out completion!))
            {
                completion = new MongoRequest(new ManualResetValueTaskSource<IParserResult>());
            }

            completion.BeginOperation();
            completion.RequestNumber = message.RequestNumber;
            completion.ParseAsync = ParseAsync<TResp>;
            _completions.GetOrAdd(completion.RequestNumber, completion);
            try
            {
                //await _protocolWriter.WriteUnsafeAsync(ProtocolWriters.QueryMessageWriter, message, cancellationToken).ConfigureAwait(false);
                await _protocolWriter.WriteAsync(ProtocolWriters.QueryMessageWriter, message, cancellationToken).ConfigureAwait(false);
                var response = await completion.GetValueTask().ConfigureAwait(false);

                if (response is QueryResult<TResp> queryResult)
                {
                    return queryResult;
                }

                return ThrowHelper.InvalidReturnType<QueryResult<TResp>>(typeof(QueryResult<TResp>), response.GetType());
            }
            finally
            {
                _completions.TryRemove(message.RequestNumber, out _);
                completion.ResetForPool();
                _queue.Enqueue(completion);
            }

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
    }
}
