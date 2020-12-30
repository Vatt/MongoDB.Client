using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Protocol.Readers;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection
    {
        public int ConnectionId { get; }
        public int Threshold => 8;
        private ConnectionContext _connection;
        private ILogger _logger;
        private ConcurrentDictionary<long, MongoReuqestBase> _completions;
        private ProtocolReader _protocolReader;
        private ProtocolWriter _protocolWriter;
        private readonly ChannelReader<MongoReuqestBase> _channelReader;
        private CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private Task? _protocolListenerTask;
        private Task? _channelListenerTask;
        private readonly ConcurrentQueue<ManualResetValueTaskSource<IParserResult>> _queue = new();
        internal MongoConnection(int connectionId, ILogger logger, ChannelReader<MongoReuqestBase> channelReader)
        {
            ConnectionId = connectionId;
            _completions = new ConcurrentDictionary<long, MongoReuqestBase>();
            _logger = logger;
            _channelReader = channelReader;
        }

        public async ValueTask<QueryResult<TResp>> SendQueryAsync<TResp>(QueryMessage message, CancellationToken cancellationToken)
        {
            //if (_shutdownCts.IsCancellationRequested == false)
            //{
            //    if (_protocolWriter is not null)
            //    {
            //        var request = new QueryMongoRequest(message, new ManualResetValueTaskSource<IParserResult>());
            //        request.ParseAsync = ParseAsync<TResp>;
            //        try
            //        {
            //            await _protocolWriter.WriteAsync(ProtocolWriters.QueryMessageWriter, message, cancellationToken).ConfigureAwait(false);
            //            var result = await request.CompletionSource.WaitWithCancellationAsync(cancellationToken).ConfigureAwait(false);

            //            if (result is QueryResult<TResp> queryResult)
            //            {
            //                return queryResult;
            //            }

            //            return default!;
            //        }
            //        finally
            //        {
            //            _completions.TryRemove(message.RequestNumber, out _);
            //        }
            //    }

            //    return ThrowHelper.ConnectionException<QueryResult<TResp>>(default /*_endpoint*/);
            //}

            //return ThrowHelper.ObjectDisposedException<QueryResult<TResp>>(nameof(MongoConnection));
            ManualResetValueTaskSource<IParserResult> taskSource;
            if (_queue.TryDequeue(out taskSource) == false)
            {
                taskSource = new ManualResetValueTaskSource<IParserResult>();
            }

            var completion = new QueryMongoRequest(message, taskSource);
            completion.ParseAsync = ParseAsync<TResp>;
            _completions.GetOrAdd(completion.Message.RequestNumber, completion);
            try
            {
                await _protocolWriter.WriteAsync(ProtocolWriters.QueryMessageWriter, message, cancellationToken).ConfigureAwait(false);
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
                        //if (SerializersMap.TryGetSerializer<T>(out var replySerializer))
                        //{
                        //    var bodyReader = new ReplyBodyReader<T>(new BsonDocumentSerializer() as IGenericBsonSerializer<T>, replyMessage);
                        //    var bodyResult = await reader.ReadAsync(bodyReader, default)
                        //        .ConfigureAwait(false);
                        //    reader.Advance();
                        //    return bodyResult.Message;
                        //}
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