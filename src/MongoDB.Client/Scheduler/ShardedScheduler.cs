using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Experimental;
using MongoDB.Client.Messages;
using MongoDB.Client.Network;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Scheduler
{
    sealed class RouterScheduler : MongoScheduler
    {
        private MongoServiceConnection _connection { get; }
        private MongoPingMessage? _lastPing;
        public MongoPingMessage? LastPing => _lastPing;
        public EndPoint EndPoint { get; }
        public RouterScheduler(MongoServiceConnection connection, MongoClientSettings settings, IMongoConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
            : base(settings, connectionFactory, loggerFactory, null)
        {
            _connection = connection;
            EndPoint = _connection.EndPoint;
            _lastPing = null;
        }
        public async ValueTask MongoPing(CancellationToken token = default)
        {
            try
            {
                _lastPing = await _connection.MongoPing(token).ConfigureAwait(false);
                ClusterTime = _lastPing.ClusterTime;
            }
            catch (Exception)
            {
                _lastPing = null;
            }

        }
    }
    internal class ShardedScheduler : IMongoScheduler
    {
        private readonly MongoClientSettings _settings;
        private readonly ILoggerFactory _loggerFactory;
        private List<RouterScheduler> _schedulers;
        private List<EndPoint> _badHosts;
        private int _schedulerCounter = 0;
        internal ShardedScheduler(MongoClientSettings settings, ILoggerFactory loggerFactory)
        {
            _settings = settings;
            _loggerFactory = loggerFactory;
            _schedulers = new();
            _badHosts = new();
        }
        public ValueTask AbortTransactionAsync(TransactionHandler transactionHandler, CancellationToken cancellationToken)
        {
            //var scheduler = GetScheduler();
            //var requestNumber = scheduler.GetNextRequestNumber();
            //var transactionRequest = new TransactionRequest(null, 1, "admin", transactionHandler.SessionId, _lastPing!.ClusterTime, transactionHandler.TxNumber, false);
            //var request = new TransactionMessage(requestNumber, transactionRequest);
            //return scheduler.TransactionAsync(request, cancellationToken);
            throw new NotImplementedException();
        }

        public ValueTask CommitTransactionAsync(TransactionHandler transactionHandler, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask CreateCollectionAsync(TransactionHandler transaction, CollectionNamespace collectionNamespace, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public ValueTask<DeleteResult> DeleteAsync(TransactionHandler transaction, BsonDocument filter, int limit, CollectionNamespace collectionNamespace, CancellationToken token)
        {
            var scheduler = GetScheduler();
            var lastPing = scheduler.LastPing!;
            var requestNumber = scheduler.GetNextRequestNumber();
            var deleteHeader = CreateDeleteHeader(collectionNamespace, transaction, lastPing.ClusterTime);

            var deleteBody = new DeleteBody(filter, limit);

            var request = new DeleteMessage(requestNumber, deleteHeader, deleteBody);
            return scheduler.DeleteAsync(request, token);
        }

        public ValueTask<UpdateResult> UpdateAsync(TransactionHandler transaction, BsonDocument filter, BsonDocument update, bool isMulty, CollectionNamespace collectionNamespace, CancellationToken token)
        {
            var scheduler = GetScheduler();
            var lastPing = scheduler.LastPing!;
            var requestNumber = scheduler.GetNextRequestNumber();
            var updateHeader = CreateUpdateHeader(collectionNamespace, transaction, lastPing.ClusterTime);

            var updateBody = new UpdateBody(filter, update,  isMulty);

            var request = new UpdateMessage(requestNumber, updateHeader, updateBody);
            return scheduler.UpdateAsync(request, token);
        }
        private UpdateHeader CreateUpdateHeader(CollectionNamespace collectionNamespace, TransactionHandler transaction, MongoClusterTime clusterTime)
        {
            switch (transaction.State)
            {
                case TransactionState.Starting:
                    transaction.State = TransactionState.InProgress;
                    return new UpdateHeader(collectionNamespace.CollectionName, true, collectionNamespace.DatabaseName, transaction.SessionId, clusterTime, transaction.TxNumber, true, false);
                case TransactionState.InProgress:
                    return new UpdateHeader(collectionNamespace.CollectionName, true, collectionNamespace.DatabaseName, transaction.SessionId, clusterTime, transaction.TxNumber, false);
                case TransactionState.Implicit:
                    return new UpdateHeader(collectionNamespace.CollectionName, true, collectionNamespace.DatabaseName, transaction.SessionId, transaction.TxNumber);
                case TransactionState.Committed:
                    return ThrowEx<UpdateHeader>("Transaction already commited");
                case TransactionState.Aborted:
                    return ThrowEx<UpdateHeader>("Transaction already aborted");
                default:
                    return ThrowEx<UpdateHeader>("Invalid transaction state");
            }
        }
        private DeleteHeader CreateDeleteHeader(CollectionNamespace collectionNamespace, TransactionHandler transaction, MongoClusterTime clusterTime)
        {
            switch (transaction.State)
            {
                case TransactionState.Starting:
                    transaction.State = TransactionState.InProgress;
                    return new DeleteHeader(collectionNamespace.CollectionName, true, collectionNamespace.DatabaseName, transaction.SessionId, clusterTime, transaction.TxNumber, true, false);
                case TransactionState.InProgress:
                    return new DeleteHeader(collectionNamespace.CollectionName, true, collectionNamespace.DatabaseName, transaction.SessionId, clusterTime, transaction.TxNumber, false);
                case TransactionState.Implicit:
                    return new DeleteHeader(collectionNamespace.CollectionName, true, collectionNamespace.DatabaseName, transaction.SessionId, transaction.TxNumber);
                case TransactionState.Committed:
                    return ThrowEx<DeleteHeader>("Transaction already commited");
                case TransactionState.Aborted:
                    return ThrowEx<DeleteHeader>("Transaction already aborted");
                default:
                    return ThrowEx<DeleteHeader>("Invalid transaction state");
            }
        }
        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public ValueTask DropCollectionAsync(TransactionHandler transaction, CollectionNamespace collectionNamespace, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<FindResult<T>> FindAsync<T>(BsonDocument filter, int limit, CollectionNamespace collectionNamespace, TransactionHandler transaction, CancellationToken token)
            where T : IBsonSerializer<T>
        {
            var scheduler = GetScheduler();
            var lastPing = scheduler.LastPing!;
            var requestNum = scheduler.GetNextRequestNumber();
            var requestDocument = CreateFindRequest(filter, limit, collectionNamespace, transaction, lastPing.ClusterTime);

            var request = new FindMessage(requestNum, requestDocument);
            var result = await scheduler.GetCursorAsync<T>(request, token).ConfigureAwait(false);
            return new FindResult<T>(result, scheduler);
        }
        private FindRequest CreateFindRequest(BsonDocument filter, int limit, CollectionNamespace collectionNamespace, TransactionHandler transaction, MongoClusterTime clusterTime)
        {
            switch (transaction.State)
            {
                case TransactionState.Starting:
                    transaction.State = TransactionState.InProgress;
                    return new FindRequest(collectionNamespace.CollectionName, filter, limit, default, null, collectionNamespace.DatabaseName, transaction.SessionId, clusterTime, transaction.TxNumber, true, false);
                case TransactionState.InProgress:
                    return new FindRequest(collectionNamespace.CollectionName, filter, limit, default, null, collectionNamespace.DatabaseName, transaction.SessionId, clusterTime, transaction.TxNumber, false);
                case TransactionState.Implicit:
                    return new FindRequest(collectionNamespace.CollectionName, filter, limit, default, null, collectionNamespace.DatabaseName, transaction.SessionId);
                case TransactionState.Committed:
                    return ThrowEx<FindRequest>("Transaction already commited");
                case TransactionState.Aborted:
                    return ThrowEx<FindRequest>("Transaction already aborted");
                default:
                    return ThrowEx<FindRequest>("Invalid transaction state");
            }
        }
        public ValueTask<CursorResult<T>> GetMoreAsync<T>(MongoScheduler scheduler, long cursorId, CollectionNamespace collectionNamespace, TransactionHandler transaction, CancellationToken token)
            where T : IBsonSerializer<T>
        {
            var info = GetScheduler();
            var lastPing = info.LastPing!;
            var requestNum = scheduler.GetNextRequestNumber();
            var requestDocument = CreateGetMoreRequest(cursorId, collectionNamespace, transaction, lastPing!.ClusterTime);
            var request = new FindMessage(requestNum, requestDocument);
            return scheduler.GetCursorAsync<T>(request, token);
        }
        private FindRequest CreateGetMoreRequest(long cursorId, CollectionNamespace collectionNamespace, TransactionHandler transaction, MongoClusterTime clusterTime)
        {
            switch (transaction.State)
            {
                case TransactionState.Starting:
                    transaction.State = TransactionState.InProgress;
                    return new FindRequest(null, null, default, cursorId, null, collectionNamespace.DatabaseName, transaction.SessionId, clusterTime, transaction.TxNumber, true, false);
                case TransactionState.InProgress:
                    return new FindRequest(null, null, default, cursorId, null, collectionNamespace.DatabaseName, transaction.SessionId, clusterTime, transaction.TxNumber, false);
                case TransactionState.Implicit:
                    //return new FindRequest(null, null, default, cursorId, null, collectionNamespace.DatabaseName, transaction.SessionId, transaction.TxNumber);
                    return new FindRequest(null, null, default, cursorId, collectionNamespace.CollectionName, collectionNamespace.DatabaseName, transaction.SessionId);
                case TransactionState.Committed:
                    return ThrowEx<FindRequest>("Transaction already commited");
                case TransactionState.Aborted:
                    return ThrowEx<FindRequest>("Transaction already aborted");
                default:
                    return ThrowEx<FindRequest>("Invalid transaction state");
            }
        }
        public ValueTask InsertAsync<T>(TransactionHandler transaction, IEnumerable<T> items, CollectionNamespace collectionNamespace, CancellationToken token)
            where T : IBsonSerializer<T>
        {
            var scheduler = GetScheduler();
            var lastPing = scheduler.LastPing!;
            var requestNumber = scheduler.GetNextRequestNumber();
            var insertHeader = CreateInsertHeader(collectionNamespace, transaction, lastPing.ClusterTime);
            var request = new InsertMessage<T>(requestNumber, insertHeader, items);
            return scheduler.InsertAsync(request, token);
        }
        private InsertHeader CreateInsertHeader(CollectionNamespace collectionNamespace, TransactionHandler transaction, MongoClusterTime clusterTime)
        {
            switch (transaction.State)
            {
                case TransactionState.Starting:
                    transaction.State = TransactionState.InProgress;
                    return new InsertHeader(collectionNamespace.CollectionName, true, collectionNamespace.DatabaseName, transaction.SessionId, clusterTime, transaction.TxNumber, true, false);
                case TransactionState.InProgress:
                    return new InsertHeader(collectionNamespace.CollectionName, true, collectionNamespace.DatabaseName, transaction.SessionId, clusterTime, transaction.TxNumber, false);
                case TransactionState.Implicit:
                    return new InsertHeader(collectionNamespace.CollectionName, true, collectionNamespace.DatabaseName, transaction.SessionId, transaction.TxNumber);
                case TransactionState.Committed:
                    return ThrowEx<InsertHeader>("Transaction already commited");
                case TransactionState.Aborted:
                    return ThrowEx<InsertHeader>("Transaction already aborted");
                default:
                    return ThrowEx<InsertHeader>("Invalid transaction state");
            }
        }

        public async ValueTask StartAsync(CancellationToken token)
        {
            var connectionfactory = new NetworkConnectionFactory(_loggerFactory);
            var endpoints = _settings.Endpoints;
            var maxConnections = _settings.ConnectionPoolMaxSize / endpoints.Length;
            maxConnections = maxConnections == 0 ? 1 : maxConnections;
            for (int i = 0; i < _settings.Endpoints.Length; i++)
            {
                try
                {

                    var endpoint = _settings.Endpoints[i];
                    IMongoConnectionFactory connectionFactory = _settings.ClientType switch
                    {
                        ClientType.Default => new MongoConnectionFactory(endpoint, _loggerFactory),
                        ClientType.Experimental => new ExperimentalMongoConnectionFactory(endpoint, _loggerFactory),
                        _ => throw new MongoBadClientTypeException()
                    };
                    var ctx = await connectionfactory.ConnectAsync(endpoint, token).ConfigureAwait(false);
                    var serviceConnection = new MongoServiceConnection(ctx);
                    await serviceConnection.Connect(_settings, token).ConfigureAwait(false);
                    var scheduler = new RouterScheduler(serviceConnection, _settings with { ConnectionPoolMaxSize = maxConnections }, connectionFactory, _loggerFactory);
                    await scheduler.MongoPing(token).ConfigureAwait(false);

                    if (scheduler.LastPing is null)
                    {
                        _badHosts.Add(endpoint);
                        continue;
                    }
                    else
                    {
                        await scheduler.StartAsync(token).ConfigureAwait(false);
                        _schedulers.Add(scheduler);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        private async ValueTask UpdateRouters(CancellationToken token)
        {
            var schedulers = _schedulers;
            for (var i = 0; i < schedulers.Count; i++)
            {
                await schedulers[i].MongoPing(token).ConfigureAwait(false);
            }
        }
        private RouterScheduler GetScheduler()
        {
            var counter = Interlocked.Increment(ref _schedulerCounter);
            var schedulers = _schedulers;
            var schedulerId = counter % schedulers.Count;
            var result = schedulers[schedulerId];
            return result;
        }

        private static TMessage ThrowEx<TMessage>(string message)
        {
            throw new MongoException(message);
        }

        // TODO:
        [DoesNotReturn]
        private static void ThrowSchedulerNotFound()
        {
            throw new Exception("Scheduler Not Found");
        }
    }
}
