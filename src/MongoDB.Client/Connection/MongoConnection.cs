using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection
    {
        public int ConnectionId { get; }
        public int Threshold => 8;
        private System.Net.Connections.Connection _connection;
        private ILogger _logger;
        private readonly List<ParserCompletion> _completions;
        private ProtocolReader _protocolReader;
        private ProtocolWriter _protocolWriter;
        private readonly ChannelReader<ManualResetValueTaskSource<IParserResult>> _channelReader;
        private CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private Task? _listenerTask;
        private int _requestCounter = 0;
        internal MongoConnection(int connectionId, ILogger logger, ChannelReader<ManualResetValueTaskSource<IParserResult>> channelReader)
        {
            ConnectionId = connectionId;
            _completions = new List<ParserCompletion>(Threshold);
            _logger = logger;
            _channelReader = channelReader;
            
        }
        private int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _requestCounter);
        }
        public async ValueTask<QueryResult<TResp>> SendQueryAsync<TResp>(QueryMessage message, CancellationToken cancellationToken)
        {
            //if (_shutdownCts.IsCancellationRequested == false)
            //{
            //    if (_protocolWriter is not null)
            //    {
            //        var completion = _completionMap.GetOrAdd(message.RequestNumber,
            //            i => new ParserCompletion(new TaskCompletionSourceWithCancellation<IParserResult>(),
            //                response => ParseAsync<TResp>(response)));

            //        try
            //        {
            //            await _protocolWriter.WriteAsync(ProtocolWriters.QueryMessageWriter, message, cancellationToken).ConfigureAwait(false);
            //            var result = await completion.CompletionSource.WaitWithCancellationAsync(cancellationToken)
            //                .ConfigureAwait(false);

            //            if (result is QueryResult<TResp> queryResult)
            //            {
            //                return queryResult;
            //            }

            //            return default!;
            //        }
            //        finally
            //        {
            //            _completionMap.TryRemove(message.RequestNumber, out _);
            //        }
            //    }

            //    return ThrowHelper.ConnectionException<QueryResult<TResp>>(_endpoint);
            //}

            //return ThrowHelper.ObjectDisposedException<QueryResult<TResp>>(nameof(Channel));


            //async ValueTask<IParserResult> ParseAsync<T>(MongoResponseMessage mongoResponse)
            //{
            //    var reader = _reader!;
            //    switch (mongoResponse)
            //    {
            //        case ReplyMessage replyMessage:
            //            if (SerializersMap.TryGetSerializer<T>(out var replySerializer))
            //            {
            //                var bodyReader = new ReplyBodyReader<T>(replySerializer, replyMessage);
            //                var bodyResult = await reader.ReadAsync(bodyReader, _shutdownToken.Token)
            //                    .ConfigureAwait(false);
            //                reader.Advance();
            //                return bodyReader.Result;
            //            }

            //            return ThrowHelper.UnsupportedTypeException<QueryResult<T>>(typeof(T));
            //        default:
            //            return ThrowHelper.UnsupportedTypeException<QueryResult<T>>(typeof(T));
            //    }
            //}
            return default;
        }

    }
}