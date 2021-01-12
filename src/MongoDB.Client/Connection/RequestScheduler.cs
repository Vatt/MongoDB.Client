using Microsoft.Extensions.ObjectPool;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{

    internal partial class RequestScheduler : IAsyncDisposable
    {
        private int MaxConnections => 100;// Environment.ProcessorCount * 2;
        private readonly MongoConnectionFactory _connectionFactory;
        private readonly List<MongoConnection> _connections;
        private readonly Channel<MongoReuqestBase> _channel;
        private readonly Channel<FindMongoRequest> _findChannel;
        private readonly ChannelWriter<MongoReuqestBase> _channelWriter;
        private readonly ChannelWriter<FindMongoRequest> _cursorChannel;
        private readonly object _initLock = new object();
        private static int _counter;
        private int _requestsInChannel = 0;
        public RequestScheduler(MongoConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            var options = new UnboundedChannelOptions();
            options.SingleWriter = true;
            options.SingleReader = false;
            options.AllowSynchronousContinuations = true;
            _channel = Channel.CreateUnbounded<MongoReuqestBase>(options);
            _findChannel = Channel.CreateUnbounded<FindMongoRequest>(options);
            _channelWriter = _channel.Writer;
            _cursorChannel = _findChannel.Writer;
            _connections = new List<MongoConnection>();
            _counter = 0;
        }
        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _counter);
        }
        internal async ValueTask InitAsync()
        {
            if (_connections.Count == 0)
            {
                for (int i = 0; i < MaxConnections; i++)
                {
                    _connections.Add(await CreateNewConnection());
                }
            }
        }
        private ValueTask<MongoConnection> CreateNewConnection()
        {
            return _connectionFactory.Create(_channel.Reader, _findChannel.Reader);
        }
        internal async ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token = default)
        {
            var request = FindMongoRequestPool.Get();
            var taskSrc = request.CompletionSource;
            request.Message = message;
            request.ParseAsync = CursorCallbackHolder<T>.CursorParseAsync;
            request.RequestNumber = message.Header.RequestNumber;
            await _cursorChannel.WriteAsync(request, token).ConfigureAwait(false);            
            var cursor = await taskSrc.GetValueTask().ConfigureAwait(false) as CursorResult<T>;
            FindMongoRequestPool.Return(request);
            return cursor;
        }

        internal async ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token = default)
        {
            var request = InsertMongoRequestPool.Get();
            var taskSource = request.CompletionSource;
            request.RequestNumber = message.Header.RequestNumber;
            request.Message = message;
            request.ParseAsync = InsertCallbackHolder<T>.InsertParseAsync; //TODO: Try FIXIT
            request.WriteAsync = InsertCallbackHolder<T>.WriteAsync<InsertMessage<T>>;
            await _channelWriter.WriteAsync(request, token).ConfigureAwait(false);
            var response = await taskSource.GetValueTask().ConfigureAwait(false) as InsertResult;
            InsertMongoRequestPool.Return(request);
            if (response is InsertResult result)
            {
                if (result.WriteErrors is null || result.WriteErrors.Count == 0)
                {
                    return;
                }

                ThrowHelper.InsertException(result.WriteErrors);
            }
            //TODO: FIXIT
            /*else if (response is BsonParseResult bson)
            {
                Debugger.Break();
            }
            */
        }
        public async ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken cancellationToken)
        {
            var request = DeleteMongoRequestPool.Get();//new DeleteMongoRequest(message, taskSource);
            var taskSource = request.CompletionSource;
            request.ParseAsync = DeleteCallbackHolder.DeleteParseAsync;
            request.Message = message;
            request.RequestNumber = message.Header.RequestNumber;
            await _channelWriter.WriteAsync(request, cancellationToken).ConfigureAwait(false);
            var deleteResult = await taskSource.GetValueTask().ConfigureAwait(false) as DeleteResult;
            DeleteMongoRequestPool.Return(request);
            return deleteResult;
        }

        public async ValueTask DisposeAsync()
        {
            _channelWriter.Complete();
            foreach (var connection in _connections)
            {
                await connection.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
