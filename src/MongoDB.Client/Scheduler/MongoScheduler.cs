using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Scheduler.Holders;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Scheduler
{

    internal partial class MongoScheduler : IAsyncDisposable
    {
        private readonly IMongoConnectionFactory _connectionFactory;
        private readonly ILogger<StandaloneScheduler> _logger;
        //TODO: fix this
        //private readonly List<MongoConnection> _connections; 
        private readonly List<MongoConnection> _connections;
        private readonly Channel<MongoRequest> _channel;
        private readonly ChannelWriter<MongoRequest> _channelWriter;
        private readonly MongoClientSettings _settings;
        private readonly int _maxConnections;
        private static int _counter;
        private SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        public MongoClusterTime? ClusterTime { get; protected set; }

        public MongoScheduler(MongoClientSettings settings, IMongoConnectionFactory connectionFactory, ILoggerFactory loggerFactory, MongoClusterTime? clusterTime)
        {
            _connectionFactory = connectionFactory;
            _logger = loggerFactory.CreateLogger<StandaloneScheduler>();
            var options = new BoundedChannelOptions(10);
            _channel = Channel.CreateBounded<MongoRequest>(options);
            _channelWriter = _channel.Writer;
            _connections = new List<MongoConnection>();
            _settings = settings;
            _counter = 0;
            _maxConnections = settings.ConnectionPoolMaxSize;
            ClusterTime = clusterTime;
        }

        public MongoScheduler(MongoClientSettings settings, IMongoConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
            : this(settings, connectionFactory, loggerFactory, null)
        {
        }


        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _counter);
        }


        public async ValueTask StartAsync(CancellationToken token)
        {
            if (_connections.Count == 0)
            {
                await _initLock.WaitAsync(token).ConfigureAwait(false);

                try
                {
                    if (_connections.Count == 0)
                    {
                        for (int i = 0; i < _maxConnections; i++)
                        {
                            var connection = await CreateNewConnection(token).ConfigureAwait(false);
                            _connections.Add(connection);
                        }
                    }

                }
                finally
                {
                    _initLock.Release();
                }
            }
        }


        private ValueTask<MongoConnection> CreateNewConnection(CancellationToken token)
        {
            return _connectionFactory.CreateAsync(_settings, _channel.Reader, this, token);
        }


        public async ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token) //where T : IBsonSerializer<T>
        {
            var request = MongoRequestPool.Get();
            var taskSrc = request.CompletionSource;
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = CursorCallbackHolder<T>.CursorParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.FindMessageWriter, message, token);
            };
            request.RequestNumber = message.Header.RequestNumber;
            if (_channelWriter.TryWrite(request) == false)
            {
                await _channelWriter.WriteAsync(request, token).ConfigureAwait(false);
            }
            var cursor = (CursorResult<T>)await taskSrc.GetValueTask().ConfigureAwait(false);
            MongoRequestPool.Return(request);
            return cursor;
        }


        public async ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token) //where T : IBsonSerializer<T>
        {
            var request = MongoRequestPool.Get();
            var taskSource = request.CompletionSource;
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = InsertCallbackHolder<T>.InsertParseAsync; //TODO: Try FIXIT
            request.WriteAsync = (protocol, token) =>
            {
                return InsertCallbackHolder<T>.WriteAsync(message, protocol, token);
            };
            if (_channelWriter.TryWrite(request) == false)
            {
                await _channelWriter.WriteAsync(request, token).ConfigureAwait(false);
            }
            var result = (InsertResult)await taskSource.GetValueTask().ConfigureAwait(false);
            MongoRequestPool.Return(request);

            if (result.WriteErrors is null || result.WriteErrors.Count == 0)
            {
                return;
            }

            ThrowHelper.InsertException(result.WriteErrors);
        }


        public async ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken token)
        {
            var request = MongoRequestPool.Get();//new DeleteMongoRequest(message, taskSource);
            var taskSource = request.CompletionSource;
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = DeleteCallbackHolder.DeleteParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.DeleteMessageWriter, message, token);
            };
            if (_channelWriter.TryWrite(request) == false)
            {
                await _channelWriter.WriteAsync(request, token).ConfigureAwait(false);
            }
            var deleteResult = (DeleteResult)await taskSource.GetValueTask().ConfigureAwait(false);
            MongoRequestPool.Return(request);
            return deleteResult!;
        }
        public async ValueTask<UpdateResult> UpdateAsync(UpdateMessage message, CancellationToken token)
        {
            var request = MongoRequestPool.Get();//new DeleteMongoRequest(message, taskSource);
            var taskSource = request.CompletionSource;
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = UpdateCallbackHolder.UpdateParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.UpdateMessageWriter, message, token);
            };
            if (_channelWriter.TryWrite(request) == false)
            {
                await _channelWriter.WriteAsync(request, token).ConfigureAwait(false);
            }
            var deleteResult = (UpdateResult)await taskSource.GetValueTask().ConfigureAwait(false);
            MongoRequestPool.Return(request);
            return deleteResult!;
        }
        public async ValueTask TransactionAsync(TransactionMessage message, CancellationToken token)
        {
            var request = MongoRequestPool.Get();
            var taskSource = request.CompletionSource;
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = TransactionCallbackHolder.TransactionParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.TransactionMessageWriter, message, token);
            };
            if (_channelWriter.TryWrite(request) == false)
            {
                await _channelWriter.WriteAsync(request, token).ConfigureAwait(false);
            }
            var transactionResult = (TransactionResult)await taskSource.GetValueTask().ConfigureAwait(false);
            MongoRequestPool.Return(request);
            if (transactionResult!.Ok != 1)
            {
                ThrowHelper.TransactionException(transactionResult!.ErrorMessage!, transactionResult!.Code!.Value, transactionResult!.CodeName!);
            }
        }

        public async ValueTask DropCollectionAsync(DropCollectionMessage message, CancellationToken token)
        {
            var taskSource = new ManualResetValueTaskSource<IParserResult>();
            var request = new MongoRequest(taskSource);
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = DropCollectionCallbackHolder.DropCollectionParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.DropCollectionMessageWriter, message, token);
            };
            if (_channelWriter.TryWrite(request) == false)
            {
                await _channelWriter.WriteAsync(request, token).ConfigureAwait(false);
            }
            var result = (DropCollectionResult)await taskSource.GetValueTask().ConfigureAwait(false);

            if (result.Ok != 1)
            {
                ThrowHelper.DropCollectionException(result.ErrorMessage!, result.Code, result.CodeName);
            }
        }


        public async ValueTask CreateCollectionAsync(CreateCollectionMessage message, CancellationToken token)
        {
            var taskSource = new ManualResetValueTaskSource<IParserResult>();
            var request = new MongoRequest(taskSource);
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = CreateCollectionCallbackHolder.CreateCollectionParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.CreateCollectionMessageWriter, message, token);
            };
            if (_channelWriter.TryWrite(request) == false)
            {
                await _channelWriter.WriteAsync(request, token).ConfigureAwait(false);
            }
            var result = (CreateCollectionResult)await taskSource.GetValueTask().ConfigureAwait(false);

            if (result.Ok != 1)
            {
                ThrowHelper.CreateCollectionException(result.ErrorMessage!, result.Code, result.CodeName!);
            }
        }

        public async Task ConnectionLost(MongoConnection connection)
        {
            try
            {
                await connection.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error on disposing connection");
            }
            _connections.Remove(connection);
            try
            {
                _connections.Add(await CreateNewConnection(default).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error on creating connection");
            }
        }

        public async ValueTask DisposeAsync()
        {
            _channelWriter.Complete();
            foreach (var connection in _connections)
            {
                await connection.DisposeAsync().ConfigureAwait(false);
            }
            _connections.Clear();
            _initLock.Dispose();
        }
    }
}
