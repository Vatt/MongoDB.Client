using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Connections;
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
        public RouterScheduler(MongoServiceConnection connection, MongoClientSettings settings, IMongoConnectionFactory connectionFactory, ILoggerFactory loggerFactory, IMongoConnectionInitializer connectionInitializer)
            : base(settings, connectionFactory, loggerFactory, null, connectionInitializer)
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

        public new async ValueTask DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);
            await _connection.DisposeAsync().ConfigureAwait(false);
        }
    }
    internal class ShardedScheduler : IMongoScheduler
    {
        private readonly MongoClientSettings _settings;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMongoConnectionInitializer _connectionInitializer;
        private readonly Func<EndPoint, CancellationToken, ValueTask<ConnectionContext>> _connectAsync;
        private List<RouterScheduler> _schedulers;
        private List<EndPoint> _badHosts;
        private int _schedulerCounter = 0;
        internal ShardedScheduler(
            MongoClientSettings settings,
            ILoggerFactory loggerFactory,
            IMongoConnectionInitializer connectionInitializer,
            Func<EndPoint, CancellationToken, ValueTask<ConnectionContext>> connectAsync)
        {
            _settings = settings;
            _loggerFactory = loggerFactory;
            _connectionInitializer = connectionInitializer;
            _connectAsync = connectAsync;
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
            var scheduler = GetScheduler();
            var requestNumber = scheduler.GetNextRequestNumber();
            var createCollectionHeader = new CreateCollectionHeader(collectionNamespace.CollectionName, collectionNamespace.DatabaseName, transaction.SessionId);
            var request = new CreateCollectionMessage(requestNumber, createCollectionHeader);
            return scheduler.CreateCollectionAsync(request, token);
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

        public ValueTask<UpdateResult> UpdateAsync(TransactionHandler transaction, BsonDocument filter, Update update, bool isMulty, CollectionNamespace collectionNamespace, UpdateOptions? options, CancellationToken token)
        {
            var scheduler = GetScheduler();
            var lastPing = scheduler.LastPing!;
            var requestNumber = scheduler.GetNextRequestNumber();
            var updateHeader = CreateUpdateHeader(collectionNamespace, transaction, lastPing.ClusterTime);

            var updateBody = options == null
                ? new UpdateBody(filter, update, isMulty)
                : new UpdateBody(filter, update, isMulty, options.IsUpsert, options.ArrayFilters, options.Collation);

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
            return DisposeAsyncCore();
        }

        private async ValueTask DisposeAsyncCore()
        {
            for (var i = 0; i < _schedulers.Count; i++)
            {
                await _schedulers[i].DisposeAsync().ConfigureAwait(false);
            }

            _schedulers.Clear();
        }

        public ValueTask DropCollectionAsync(TransactionHandler transaction, CollectionNamespace collectionNamespace, CancellationToken token)
        {
            var scheduler = GetScheduler();
            var requestNumber = scheduler.GetNextRequestNumber();
            var dropCollectionHeader = new DropCollectionHeader(collectionNamespace.CollectionName, collectionNamespace.DatabaseName, transaction.SessionId);
            var request = new DropCollectionMessage(requestNumber, dropCollectionHeader);
            return scheduler.DropCollectionAsync(request, token);
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
                    ConnectionContext? ctx = null;
                    MongoServiceConnection? serviceConnection = null;

                    try
                    {
                        ctx = await _connectAsync(endpoint, token).ConfigureAwait(false);
                        serviceConnection = new MongoServiceConnection(ctx);
                        await serviceConnection.Connect(_connectionInitializer, token).ConfigureAwait(false);
                        var scheduler = new RouterScheduler(serviceConnection, _settings with { ConnectionPoolMaxSize = maxConnections }, connectionFactory, _loggerFactory, _connectionInitializer);
                        await scheduler.MongoPing(token).ConfigureAwait(false);

                        if (scheduler.LastPing is null)
                        {
                            _badHosts.Add(endpoint);
                            await scheduler.DisposeAsync().ConfigureAwait(false);
                            serviceConnection = null;
                            continue;
                        }

                        await scheduler.StartAsync(token).ConfigureAwait(false);
                        _schedulers.Add(scheduler);
                        serviceConnection = null;
                    }
                    catch
                    {
                        if (serviceConnection is not null)
                        {
                            await serviceConnection.DisposeAsync().ConfigureAwait(false);
                        }
                        else if (ctx is not null)
                        {
                            await ctx.DisposeAsync().ConfigureAwait(false);
                        }

                        throw;
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
