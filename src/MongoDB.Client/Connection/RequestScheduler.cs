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
            _counter = 10;
        }
        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _counter);
        }
        internal async ValueTask InitAsync()
        {
            var conn1 = await CreateNewConnection(); 
            var conn2 = await CreateNewConnection(); 
            var conn3 = await CreateNewConnection(); 
            var conn4 = await CreateNewConnection(); 
            var conn5 = await CreateNewConnection(); 
            var conn6 = await CreateNewConnection(); 
            var conn7 = await CreateNewConnection(); 
            var conn8 = await CreateNewConnection(); 
            var conn9 = await CreateNewConnection(); 
            var conn10 = await CreateNewConnection();
            var conn11 = await CreateNewConnection();
            var conn12 = await CreateNewConnection();
            var conn13 = await CreateNewConnection();
            var conn14 = await CreateNewConnection();
            var conn15 = await CreateNewConnection();
            var conn16 = await CreateNewConnection();
            var conn17 = await CreateNewConnection();
            var conn18 = await CreateNewConnection();
            var conn19 = await CreateNewConnection();
            var conn20 = await CreateNewConnection();

            conn1.LeftConnection = conn20;
            conn1.RigthConnection = conn2;
            conn2.LeftConnection = conn1;
            conn2.RigthConnection = conn3;
            conn3.LeftConnection = conn2;
            conn3.RigthConnection = conn4;
            conn4.LeftConnection = conn3;
            conn4.RigthConnection = conn5;
            conn5.LeftConnection = conn4;
            conn5.RigthConnection = conn6;
            conn6.LeftConnection = conn5;
            conn6.RigthConnection = conn7;
            conn7.LeftConnection = conn6;
            conn7.RigthConnection = conn8;
            conn8.LeftConnection = conn7;
            conn8.RigthConnection = conn8;
            conn9.LeftConnection = conn8;
            conn9.RigthConnection = conn9;
            conn10.LeftConnection = conn9;
            conn10.RigthConnection = conn11;
            conn11.LeftConnection = conn10;
            conn11.RigthConnection = conn12;
            conn12.LeftConnection = conn11;
            conn12.RigthConnection = conn13;
            conn13.LeftConnection = conn12;
            conn13.RigthConnection = conn14;
            conn14.LeftConnection = conn13;
            conn14.RigthConnection = conn15;
            conn15.LeftConnection = conn14;
            conn15.RigthConnection = conn16;
            conn16.LeftConnection = conn15;
            conn16.RigthConnection = conn17;
            conn17.LeftConnection = conn16;
            conn17.RigthConnection = conn18;
            conn18.LeftConnection = conn17;
            conn18.RigthConnection = conn19;
            conn19.LeftConnection = conn18;
            conn19.RigthConnection = conn20;
            conn20.LeftConnection = conn19;
            conn20.RigthConnection = conn1;
            _connections.Add(conn1);
            _connections.Add(conn2);
            _connections.Add(conn3);
            _connections.Add(conn4);
            _connections.Add(conn5);
            _connections.Add(conn6);
            _connections.Add(conn7);
            _connections.Add(conn8);
            _connections.Add(conn9);
            _connections.Add(conn10);
            _connections.Add(conn11);
            _connections.Add(conn12);
            _connections.Add(conn13);
            _connections.Add(conn14);
            _connections.Add(conn15);
            _connections.Add(conn16);
            _connections.Add(conn17);
            _connections.Add(conn18);
            _connections.Add(conn19);
            _connections.Add(conn20);
        }
        private MongoConnection GetConnection()
        {
            var rnd = new Random();
            return _connections[rnd.Next(0, 19)];
        }
        private ValueTask<MongoConnection> CreateNewConnection()
        {
            return _connectionFactory.Create(_settings);
        }
        internal ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token = default)
        {
            return GetConnection().GetCursorAsync<T>(message, false, token);
        }

        internal ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token = default)
        {
            return GetConnection().InsertAsync(message, false, token);
        }
        public ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken token)
        {
            return GetConnection().DeleteAsync(message, false, token);
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
