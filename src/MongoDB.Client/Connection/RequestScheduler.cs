using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{

    internal partial class RequestScheduler //: IAsyncDisposable
    {
        private readonly MongoConnectionFactory _connectionFactory;
        private readonly Channel<MongoConnection> _connections;        
        private readonly MongoClientSettings _settings;
        private static int _counter;
        private int _connectionsCount;
        public RequestScheduler(MongoClientSettings settings, MongoConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _connections = Channel.CreateUnbounded<MongoConnection>(new UnboundedChannelOptions { AllowSynchronousContinuations = false, SingleReader = false, SingleWriter = false });


            _settings = settings;
            _counter = 10;
        }
        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _counter);
        }
        internal async ValueTask InitAsync()
        {
            for(int i = 0; i< _settings.ConnectionPoolMinSize; i++)
            {
                _connections.Writer.TryWrite(await CreateNewConnection());
            }
        }
        private async ValueTask<MongoConnection> GetConnection()
        {
            if(_connections.Reader.TryRead(out var connection))
            {
                return connection;
            }
            if(_connectionsCount <= _settings.ConnectionPoolMaxSize)
            {
                Interlocked.Increment(ref _connectionsCount);
                var newConnection = await CreateNewConnection();
                if(!_connections.Writer.TryWrite(newConnection))
                {
                    await _connections.Writer.WriteAsync(newConnection);
                }
                return newConnection;
            }
            return await _connections.Reader.ReadAsync();

        }
        private async ValueTask<MongoConnection> CreateNewConnection()
        {
            var connection = await _connectionFactory.Create(_settings);
            Console.WriteLine($"Connection {connection.ConnectionId} created");
            return connection;
        }
        internal async ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token = default)
        {
            var connection = await GetConnection();
            var result = await connection.GetCursorAsync<T>(message, token);
            _connections.Writer.TryWrite(connection);
            return result;
        }

        internal async ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token = default)
        {
            var connection = await GetConnection();
            await connection.InsertAsync(message, token);
            _connections.Writer.TryWrite(connection);
        }
        public async ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken token)
        {
            var connection = await GetConnection();
            var result = await connection.DeleteAsync(message, token);
            _connections.Writer.TryWrite(connection);
            return result;
        }

        //public async ValueTask DropCollectionAsync(DropCollectionMessage message, CancellationToken cancellationToken)
        //{
        //    var taskSource = new ManualResetValueTaskSource<IParserResult>();
        //    var request = new MongoRequest(taskSource);
        //    request.RequestNumber = message.Header.RequestNumber;
        //    request.ParseAsync = DropCollectionCallbackHolder.DropCollectionParseAsync;
        //    request.WriteAsync = (protocol, token) =>
        //    {
        //        return protocol.WriteAsync(ProtocolWriters.DropCollectionMessageWriter, message, token);
        //    };
        //    await _channelWriter.WriteAsync(request);
        //    var result = await taskSource.GetValueTask().ConfigureAwait(false);
        //    if (result is DropCollectionResult dropCollectionResult)
        //    {
        //        if (dropCollectionResult.Ok != 1)
        //        {
        //            ThrowHelper.DropCollectionException(dropCollectionResult.ErrorMessage!);
        //        }
        //    }
        //}

        //public async ValueTask CreateCollectionAsync(CreateCollectionMessage message, CancellationToken cancellationToken)
        //{
        //    var taskSource = new ManualResetValueTaskSource<IParserResult>();
        //    var request = new MongoRequest(taskSource);
        //    request.RequestNumber = message.Header.RequestNumber;
        //    request.ParseAsync = CreateCollectionCallbackHolder.CreateCollectionParseAsync;
        //    request.WriteAsync = (protocol, token) =>
        //    {
        //        return protocol.WriteAsync(ProtocolWriters.CreateCollectionMessageWriter, message, token);
        //    };
        //    await _channelWriter.WriteAsync(request);
        //    var result = await taskSource.GetValueTask().ConfigureAwait(false);
        //    if (result is CreateCollectionResult CreateCollectionResult)
        //    {
        //        if (CreateCollectionResult.Ok != 1)
        //        {
        //            ThrowHelper.CreateCollectionException(CreateCollectionResult.ErrorMessage!, CreateCollectionResult.Code, CreateCollectionResult.CodeName!);
        //        }
        //    }
        //}

        //public async ValueTask DisposeAsync()
        //{
        //    _channelWriter.Complete();
        //    foreach (var connection in _connections)
        //    {
        //        await connection.DisposeAsync().ConfigureAwait(false);
        //    }
        //}
    }
}
