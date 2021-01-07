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

    internal class RequestScheduler : IAsyncDisposable
    {
        private class FindRequestMessagePolicy : IPooledObjectPolicy<FindMongoRequest>
        {
            public FindMongoRequest Create()
            {
                return new FindMongoRequest(new ManualResetValueTaskSource<IParserResult>());
            }

            public bool Return(FindMongoRequest obj)
            {
                obj.CompletionSource.Reset();
                obj.RequestNumber = default;
                obj.Message = default;
                obj.ParseAsync = default;
                return true;
            }
        }
        private class InsertRequestMessagePolicy : IPooledObjectPolicy<InsertMongoRequest>
        {
            public InsertMongoRequest Create()
            {
                return new InsertMongoRequest(new ManualResetValueTaskSource<IParserResult>());
            }

            public bool Return(InsertMongoRequest obj)
            {
                obj.CompletionSource.Reset();
                obj.RequestNumber = default;
                obj.Message = default;
                obj.RequestNumber = default;
                obj.ParseAsync = default;
                obj.WriteAsync = default;
                return true;
            }
        }
        private class DeleteRequestMessagePolicy : IPooledObjectPolicy<DeleteMongoRequest>
        {
            public DeleteMongoRequest Create()
            {
                return new DeleteMongoRequest(new ManualResetValueTaskSource<IParserResult>());
            }

            public bool Return(DeleteMongoRequest obj)
            {
                obj.CompletionSource.Reset();
                obj.RequestNumber = default;
                obj.Message = default;
                obj.ParseAsync = default;
                return true;
            }
        }
        private static ObjectPool<FindMongoRequest> FindMongoRequestPool => _findMongoRequestPool ??= new DefaultObjectPool<FindMongoRequest>(new FindRequestMessagePolicy());
        [ThreadStatic]
        private static ObjectPool<FindMongoRequest>? _findMongoRequestPool;
        
        private static ObjectPool<InsertMongoRequest> InsertMongoRequestPool => _insertMongoRequestPool ??= new DefaultObjectPool<InsertMongoRequest>(new InsertRequestMessagePolicy());
        [ThreadStatic]
        private static ObjectPool<InsertMongoRequest>? _insertMongoRequestPool;

        private static ObjectPool<DeleteMongoRequest> DeleteMongoRequestPool => _deleteMongoRequestPool ??= new DefaultObjectPool<DeleteMongoRequest>(new DeleteRequestMessagePolicy());
        [ThreadStatic]
        private static ObjectPool<DeleteMongoRequest>? _deleteMongoRequestPool;
        private int MaxConnections => Environment.ProcessorCount;
        private readonly MongoConnectionFactory _connectionFactory;
        private readonly List<MongoConnection> _connections;
        private readonly Channel<MongoReuqestBase> _channel;
        private readonly ChannelWriter<MongoReuqestBase> _channelWriter;
        private static int _counter;
        private readonly ConcurrentQueue<ManualResetValueTaskSource<IParserResult>> _queue = new();
        

        public RequestScheduler(MongoConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            var options = new UnboundedChannelOptions();
            options.SingleWriter = true;
            options.SingleReader = false;
            options.AllowSynchronousContinuations = true;
            _channel = Channel.CreateUnbounded<MongoReuqestBase>(options);
            _channelWriter = _channel.Writer;
            _connections = new List<MongoConnection>();
            _counter = 0;
        }
        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _counter);
        }
        internal async ValueTask Init()
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
            return _connectionFactory.Create(_channel.Reader);
        }
        internal async ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token = default)
        {
            if (_connections.Count == 0)
            {
                await Init();
            }
            var request = FindMongoRequestPool.Get();
            var taskSrc = request.CompletionSource;
            request.Message = message;
            request.ParseAsync = CursorCallbackHolder<T>.CursorParseAsync;
            request.RequestNumber = message.Header.RequestNumber;
            await _channelWriter.WriteAsync(request, token).ConfigureAwait(false);
            var cursor = await taskSrc.GetValueTask().ConfigureAwait(false) as CursorResult<T>;
            FindMongoRequestPool.Return(request);
            return cursor;
        }

        internal async ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token = default)
        {
            if (_connections.Count == 0)
            {
                await Init();
            }
            var request = InsertMongoRequestPool.Get();
            var taskSource = request.CompletionSource;
            request.RequestNumber = message.Header.RequestNumber;
            request.Message = message;
            request.ParseAsync = InsertParserCallbackHolder<T>.InsertParseAsync; //TODO: Try FIXIT
            request.WriteAsync = InsertParserCallbackHolder<T>.WriteAsync<InsertMessage<T>>;
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
            if (_connections.Count == 0)
            {
                await Init();
            }
            var request = DeleteMongoRequestPool.Get();//new DeleteMongoRequest(message, taskSource);
            var taskSource = request.CompletionSource;
            request.ParseAsync = DeleteParserCallbackHolder.DeleteParseAsync;
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
            foreach(var connection in _connections)
            {
                await connection.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
