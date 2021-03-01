using Microsoft.Extensions.Logging;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Scheduler
{

    internal partial class StandaloneScheduler : IMongoScheduler, IAsyncDisposable
    {
        private readonly IMongoConnectionFactory _connectionFactory;
        private readonly ILogger<StandaloneScheduler> _logger;
        private readonly List<MongoConnection> _connections;
        private readonly Channel<MongoRequest> _channel;
        private readonly Channel<MongoRequest> _findChannel;
        private readonly ChannelWriter<MongoRequest> _channelWriter;
        private readonly ChannelWriter<MongoRequest> _cursorChannel;
        private readonly MongoClientSettings _settings;
        private static int _counter;
        public StandaloneScheduler(MongoClientSettings settings, IMongoConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
        {
            _connectionFactory = connectionFactory;
            _logger = loggerFactory.CreateLogger<StandaloneScheduler>();
            var options = new UnboundedChannelOptions();
            options.SingleWriter = true;
            options.SingleReader = false;
            options.AllowSynchronousContinuations = true;
            _channel = Channel.CreateUnbounded<MongoRequest>(options);
            _findChannel = Channel.CreateUnbounded<MongoRequest>(options);
            _channelWriter = _channel.Writer;
            _cursorChannel = _findChannel.Writer;
            _connections = new List<MongoConnection>();
            _settings = settings;
            _counter = 0;
        }


        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _counter);
        }


        public async ValueTask InitAsync()
        {
            if (_connections.Count == 0)
            {
                for (int i = 0; i < _settings.ConnectionPoolMaxSize; i++)
                {
                    _connections.Add(await CreateNewConnection());
                }
            }
        }


        private ValueTask<MongoConnection> CreateNewConnection()
        {
            return _connectionFactory.CreateAsync(_settings, _channel.Reader, _findChannel.Reader, this);
        }


        public async ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token = default)
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
            await _cursorChannel.WriteAsync(request);
            var cursor =(CursorResult<T>) await taskSrc.GetValueTask().ConfigureAwait(false);
            MongoRequestPool.Return(request);
            return cursor;
        }


        public async ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token = default)
        {
            var request = MongoRequestPool.Get();
            var taskSource = request.CompletionSource;
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = InsertCallbackHolder<T>.InsertParseAsync; //TODO: Try FIXIT
            request.WriteAsync = (protocol, token) =>
            {
                return InsertCallbackHolder<T>.WriteAsync(message, protocol, token);
            };
            await _channelWriter.WriteAsync(request, token);
            var response = await taskSource.GetValueTask().ConfigureAwait(false) as InsertResult;
            MongoRequestPool.Return(request);
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
            var request = MongoRequestPool.Get();//new DeleteMongoRequest(message, taskSource);
            var taskSource = request.CompletionSource;
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = DeleteCallbackHolder.DeleteParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.DeleteMessageWriter, message, token);
            };
            await _channelWriter.WriteAsync(request);
            var deleteResult = await taskSource.GetValueTask().ConfigureAwait(false) as DeleteResult;
            MongoRequestPool.Return(request);
            return deleteResult!;
        }


        public async ValueTask DropCollectionAsync(DropCollectionMessage message, CancellationToken cancellationToken)
        {
            var taskSource = new ManualResetValueTaskSource<IParserResult>();
            var request = new MongoRequest(taskSource);
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = DropCollectionCallbackHolder.DropCollectionParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.DropCollectionMessageWriter, message, token);
            };
            await _channelWriter.WriteAsync(request);
            var result = await taskSource.GetValueTask().ConfigureAwait(false);
            if (result is DropCollectionResult dropCollectionResult)
            {
                if (dropCollectionResult.Ok != 1)
                {
                    ThrowHelper.DropCollectionException(dropCollectionResult.ErrorMessage!);
                }
            }
        }


        public async ValueTask CreateCollectionAsync(CreateCollectionMessage message, CancellationToken cancellationToken)
        {
            var taskSource = new ManualResetValueTaskSource<IParserResult>();
            var request = new MongoRequest(taskSource);
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = CreateCollectionCallbackHolder.CreateCollectionParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.CreateCollectionMessageWriter, message, token);
            };
            await _channelWriter.WriteAsync(request);
            var result = await taskSource.GetValueTask().ConfigureAwait(false);
            if (result is CreateCollectionResult CreateCollectionResult)
            {
                if (CreateCollectionResult.Ok != 1)
                {
                    ThrowHelper.CreateCollectionException(CreateCollectionResult.ErrorMessage!, CreateCollectionResult.Code, CreateCollectionResult.CodeName!);
                }
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
                _connections.Add(await CreateNewConnection());
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
        }
    }
}
