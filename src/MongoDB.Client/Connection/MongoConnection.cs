using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal struct RequestCompletion
    {
        public RequestCompletion(ManualResetValueTaskSource<IParserResult> completionSource, Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>> parseAsync)
        {
            CompletionSource = completionSource;
            ParseAsync = parseAsync;
        }

        public ManualResetValueTaskSource<IParserResult> CompletionSource { get; }
        public Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>> ParseAsync { get; }
    }
    internal sealed partial class MongoConnection : IAsyncDisposable
    {

        public int ConnectionId { get; }
        public int Threshold { get; }
        public int RequestsInWork => _completions.Count;
        private ConnectionContext _connection;
        private ILogger _logger;
        private ConcurrentDictionary<int, RequestCompletion> _completions;
        private ProtocolReader _protocolReader;
        private ProtocolWriter _protocolWriter;
        private CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private Task _protocolListenerTask;

        private readonly ConcurrentQueue<ManualResetValueTaskSource<IParserResult>> _queue = new();
        private readonly MongoClientSettings _settings;
        internal MongoConnection LeftConnection;
        internal MongoConnection RigthConnection;
        internal MongoConnection(int connectionId, MongoClientSettings settings, ILogger logger)
        {
            ConnectionId = connectionId;
            _completions = new ConcurrentDictionary<int, RequestCompletion>();
            _logger = logger;
            _settings = settings;
            Threshold = settings.MultiplexingTreshold;
        }

        public async ValueTask DisposeAsync()
        {
            _shutdownCts.Cancel();
            await _protocolWriter.DisposeAsync().ConfigureAwait(false);
            await _protocolListenerTask.ConfigureAwait(false);
            await _protocolReader.DisposeAsync().ConfigureAwait(false);
            await _connection.DisposeAsync().ConfigureAwait(false);
        }
        private ManualResetValueTaskSource<IParserResult> GetTaskSrc()
        {
            ManualResetValueTaskSource<IParserResult> src;
            if (!_queue.TryDequeue(out src))
            {
                src = new ManualResetValueTaskSource<IParserResult>();
            }
            return src;
        }
        public async ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, bool force, CancellationToken token = default)
        {
            if (force == false && _completions.Count >= Threshold)
            {
                if(LeftConnection.RequestsInWork < RequestsInWork)
                {
                    return await LeftConnection.GetCursorAsync<T>(message, true, token).ConfigureAwait(false);
                }
                if(RigthConnection.RequestsInWork < RequestsInWork)
                {
                    return await RigthConnection.GetCursorAsync<T>(message, true, token).ConfigureAwait(false);
                }
            }

            ManualResetValueTaskSource<IParserResult> src = GetTaskSrc();
            _completions.GetOrAdd(message.Header.RequestNumber, CursorCallbackHolder<T>.CreateCompletion(src));
            try
            {
                await _protocolWriter.WriteAsync(ProtocolWriters.FindMessageWriter, message, token).ConfigureAwait(false);
                var result = await src.GetValueTask().ConfigureAwait(false);
                if (result is CursorResult<T> cursor)
                {
                    return cursor;
                }
                _completions.TryRemove(message.Header.RequestNumber, out _);
                return default;
            }
            finally
            {
                src.Reset();
                _queue.Enqueue(src);
                _completions.TryRemove(message.Header.RequestNumber, out _);
            }

        }
        public async ValueTask InsertAsync<T>(InsertMessage<T> message, bool force, CancellationToken token = default)
        {
            if (force == false && _completions.Count >= Threshold)
            {
                if (LeftConnection.RequestsInWork < RequestsInWork)
                {
                    await LeftConnection.InsertAsync(message, true, token).ConfigureAwait(false);
                }
                if (RigthConnection.RequestsInWork < RequestsInWork)
                {
                    await RigthConnection.InsertAsync(message, true, token).ConfigureAwait(false);
                }
            }
            ManualResetValueTaskSource<IParserResult> src = GetTaskSrc();
            _completions.GetOrAdd(message.Header.RequestNumber, InsertCallbackHolder<T>.CreateCompletion(src));
            try
            {
                await _protocolWriter.WriteAsync(InsertCallbackHolder<T>.InsertMessageWriter, message, token).ConfigureAwait(false);
                var result = await src.GetValueTask().ConfigureAwait(false);
                if (result is InsertResult insertResult)
                {
                    if (insertResult.WriteErrors is null)
                    {
                        return;
                    }
                    ThrowHelper.InsertException(insertResult.WriteErrors);
                }
            }
            finally
            {
                _completions.TryRemove(message.Header.RequestNumber, out _);
                src.Reset();
                _queue.Enqueue(src);
            }
        }
        public async ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, bool force, CancellationToken token)
        {
            if (force == false && _completions.Count >= Threshold)
            {
                if (LeftConnection.RequestsInWork < RequestsInWork)
                {
                    return await LeftConnection.DeleteAsync(message, true, token).ConfigureAwait(false);
                }
                if (RigthConnection.RequestsInWork < RequestsInWork)
                {
                    return await RigthConnection.DeleteAsync(message, true, token).ConfigureAwait(false);
                }
            }
            var src = GetTaskSrc();
            _completions.GetOrAdd(message.Header.RequestNumber, DeleteCallbackHolder.CreateCompletion(src));
            try
            {
                await _protocolWriter.WriteAsync(ProtocolWriters.DeleteMessageWriter, message, token).ConfigureAwait(false);
                var result = await src.GetValueTask().ConfigureAwait(false);
                if (result is DeleteResult deleteResult)
                {
                    return deleteResult;
                }
                return default;
            }
            finally
            {
                _completions.TryRemove(message.Header.RequestNumber, out _);
                src.Reset();
                _queue.Enqueue(src);
            }

        }
    }
}