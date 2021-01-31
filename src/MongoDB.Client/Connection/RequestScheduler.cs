using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{

    internal partial class RequestScheduler //: IAsyncDisposable
    {
        private readonly MongoConnectionFactory _connectionFactory;
        private readonly List<MongoConnection> _connections;
        
        private readonly MongoClientSettings _settings;
        private static int _counter;
        private MongoConnection First;
        private MongoConnection Last;
        private MongoConnection Current;
        public RequestScheduler(MongoClientSettings settings, MongoConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _connections = new List<MongoConnection>();
            _settings = settings;
            _counter = 0;
        }
        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _counter);
        }
        internal async ValueTask InitAsync()
        {
            First = await CreateNewConnection();
            Last = await CreateNewConnection();
            var middle = await CreateNewConnection();
            middle.LeftConnection = Last;
            middle.RigthConnection = First;
            First.RigthConnection = middle;
            First.LeftConnection = Last;
            Last.LeftConnection = middle;
            Last.RigthConnection = First;
            Current = middle;
        }
        private ValueTask<MongoConnection> CreateNewConnection()
        {
            return _connectionFactory.Create(_settings);
        }
        internal ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token = default)
        {
            return Last.GetCursorAsync<T>(message, false, token);
        }

        internal ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token = default)
        {
            return Last.InsertAsync(message, false, token);
        }
        public ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken token)
        {
            return Last.DeleteAsync(message, false, token);
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
