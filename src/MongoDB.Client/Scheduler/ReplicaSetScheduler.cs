using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Experimental;
using MongoDB.Client.Messages;
using MongoDB.Client.Network;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ReadPreference = MongoDB.Client.Settings.ReadPreference;

namespace MongoDB.Client.Scheduler
{
    internal sealed class ReplicaSetScheduler : IMongoScheduler
    {
        private readonly NetworkConnectionFactory _networkfactory;
        private readonly ILoggerFactory _loggerFactory;
        private MongoServiceConnection _service;
        private MongoClientSettings _settings;
        private List<IMongoScheduler> _shedulers;
        private IMongoScheduler _master;
        private List<IMongoScheduler> _slaves;
        private ILogger _logger;
        private MongoPingMessage _lastPing;
        private int _schedulerCounter = 0;
        private int _requestCounter = 0;

        public MongoClusterTime ClusterTime => _lastPing?.ClusterTime!;


        public ReplicaSetScheduler(MongoClientSettings settings, ILoggerFactory loggerFactory)
        {
            _settings = settings;
            _logger = loggerFactory.CreateLogger<ReplicaSetScheduler>();
            _networkfactory = new NetworkConnectionFactory(loggerFactory);
            _loggerFactory = loggerFactory;
            _shedulers = new();
            _slaves = new();
        }


        private IMongoScheduler GetSlaveScheduler()
        {
            Interlocked.Increment(ref _schedulerCounter);
            var schedulerId = _schedulerCounter % _slaves.Count;
            var result = _slaves[schedulerId];
            return result;
        }


        public async ValueTask StartAsync()
        {
            var endpoints = _settings.Endpoints;
            var maxConnections = _settings.ConnectionPoolMaxSize / endpoints.Length;
            maxConnections = maxConnections == 0 ? 1 : maxConnections;
            for (int i = 0; i < _settings.Endpoints.Length; i++)
            {
                ConnectionContext? ctx;
                try
                {
                    ctx = await _networkfactory.ConnectAsync(_settings.Endpoints[i]);
                }
                catch (Exception ex)
                {
                    continue;
                }
                _service = new MongoServiceConnection(ctx);
                break;
            }
            if (_service is null)
            {
                ThrowHelper.MongoInitExceptions();
            }
            await _service.Connect(_settings, default).ConfigureAwait(false);
            _lastPing = await _service.MongoPing().ConfigureAwait(false);
            if (_lastPing.Primary is null)
            {
                ThrowHelper.PrimaryNullExceptions(); //TODO: fixit
            }
            for (var i = 0; i < _lastPing.Hosts.Count; i++)
            {
                var host = _lastPing.Hosts[i];
                IMongoConnectionFactory connectionFactory = _settings.ClientType == ClientType.Default ? new MongoConnectionFactory(host, _loggerFactory) : new ExperimentalMongoConnectionFactory(host, _loggerFactory);
                var scheduler = new StandaloneScheduler(_settings with { ConnectionPoolMaxSize = maxConnections }, connectionFactory, _loggerFactory, _lastPing.ClusterTime);
                _shedulers.Add(scheduler);
                if (host.Equals(_lastPing.Primary))
                {
                    _master = scheduler;
                }
                else
                {
                    _slaves.Add(scheduler);
                }
                await scheduler.StartAsync();
            }
        }


        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _requestCounter);
        }

        public Task ConnectionLost(MongoConnection connection)
        {
            throw new NotImplementedException();
        }

        public ValueTask CreateCollectionAsync(CreateCollectionMessage message, CancellationToken cancellationToken)
        {
            return _master.CreateCollectionAsync(message, cancellationToken);
        }

        public ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken cancellationToken)
        {
            return _master.DeleteAsync(message, cancellationToken);
        }

        private DeleteHeader CreateDeleteHeader(TransactionHandler transaction)
        {
            switch (transaction.State)
            {
                case TransactionState.Starting:
                    transaction.State = TransactionState.InProgress;
                    return new DeleteHeader(Namespace.CollectionName, true, Namespace.DatabaseName, transaction.SessionId, _scheduler.ClusterTime, transaction.TxNumber, true, false);
                case TransactionState.InProgress:
                    return new DeleteHeader(Namespace.CollectionName, true, Namespace.DatabaseName, transaction.SessionId, _scheduler.ClusterTime, transaction.TxNumber, false);
                case TransactionState.Implicit:
                    return new DeleteHeader(Namespace.CollectionName, true, Namespace.DatabaseName, transaction.SessionId, transaction.TxNumber);
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

        public ValueTask DropCollectionAsync(DropCollectionMessage message, CancellationToken cancellationToken)
        {
            return _master.DropCollectionAsync(message, cancellationToken);
        }

        public async ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token)
        {
            var scheduler = GetScheduler(message);
            var result = await scheduler.GetCursorAsync<T>(message, token).ConfigureAwait(false);
            return result;
        }

        private IMongoScheduler GetScheduler(FindMessage message)
        {
            var readPreference = message.Document.TxnNumber is null ? _settings.ReadPreference : ReadPreference.Primary;
            IMongoScheduler ? scheduler = null;
            switch (readPreference)
            {
                case ReadPreference.Primary:
                    scheduler = _master;
                    break;
                case ReadPreference.PrimaryPreferred:
                    scheduler = _master;
                    if (scheduler is null)
                    {
                        scheduler = GetSlaveScheduler();
                        message.Document.ReadPreference = new Messages.ReadPreference(_settings.ReadPreference);
                    }
                    break;
                case ReadPreference.Secondary:
                    scheduler = GetSlaveScheduler();
                    message.Document.ReadPreference = new Messages.ReadPreference(_settings.ReadPreference);
                    break;
                case ReadPreference.SecondaryPreferred:
                    scheduler = GetSlaveScheduler();
                    if (scheduler is null)
                    {
                        scheduler = _master;
                    }
                    else
                    {
                        message.Document.ReadPreference = new Messages.ReadPreference(_settings.ReadPreference);
                    }
                    break;
                case ReadPreference.Nearest:
                default:
                    ReadPreferenceNotSupported(_settings.ReadPreference);
                    break;
            }
            if (scheduler is null)
            {
                ThrowSchedulerNotFound();
            }
            return scheduler;
        }

        public ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token)
        {
            return _master.InsertAsync(message, token);
        }

        private InsertHeader CreateInsertHeader(TransactionHandler transaction)
        {
            switch (transaction.State)
            {
                case TransactionState.Starting:
                    transaction.State = TransactionState.InProgress;
                    return new InsertHeader(Namespace.CollectionName, true, Namespace.DatabaseName, transaction.SessionId, _scheduler.ClusterTime, transaction.TxNumber, true, false);
                case TransactionState.InProgress:
                    return new InsertHeader(Namespace.CollectionName, true, Namespace.DatabaseName, transaction.SessionId, _scheduler.ClusterTime, transaction.TxNumber, false);
                case TransactionState.Implicit:
                    return new InsertHeader(Namespace.CollectionName, true, Namespace.DatabaseName, transaction.SessionId, transaction.TxNumber);
                case TransactionState.Committed:
                    return ThrowEx<InsertHeader>("Transaction already commited");
                case TransactionState.Aborted:
                    return ThrowEx<InsertHeader>("Transaction already aborted");
                default:
                    return ThrowEx<InsertHeader>("Invalid transaction state");
            }
        }


        public ValueTask TransactionAsync(TransactionMessage message, CancellationToken token)
        {
            return _master.TransactionAsync(message, token);
        }

        private FindRequest CreateGetMoreRequest(long cursorId, TransactionHandler transaction)
        {
            switch (transaction.State)
            {
                case TransactionState.Starting:
                    transaction.State = TransactionState.InProgress;
                    return new FindRequest(null, null, default, cursorId, null, _collectionNamespace.DatabaseName, transaction.SessionId, _scheduler.ClusterTime, transaction.TxNumber, true, false);
                case TransactionState.InProgress:
                    return new FindRequest(null, null, default, cursorId, null, _collectionNamespace.DatabaseName, transaction.SessionId, _scheduler.ClusterTime, transaction.TxNumber, false);
                case TransactionState.Implicit:
                    return new FindRequest(null, null, default, cursorId, null, _collectionNamespace.DatabaseName, transaction.SessionId, transaction.TxNumber);
                case TransactionState.Committed:
                    return ThrowEx<FindRequest>("Transaction already commited");
                case TransactionState.Aborted:
                    return ThrowEx<FindRequest>("Transaction already aborted");
                default:
                    return ThrowEx<FindRequest>("Invalid transaction state");
            }
        }

        private FindRequest CreateFindRequest(BsonDocument filter, TransactionHandler transaction)
        {
            switch (transaction.State)
            {
                case TransactionState.Starting:
                    transaction.State = TransactionState.InProgress;
                    return new FindRequest(_collectionNamespace.CollectionName, filter, _limit, default, null, _collectionNamespace.DatabaseName, transaction.SessionId, _scheduler.ClusterTime, transaction.TxNumber, true, false);
                case TransactionState.InProgress:
                    return new FindRequest(_collectionNamespace.CollectionName, filter, _limit, default, null, _collectionNamespace.DatabaseName, transaction.SessionId, _scheduler.ClusterTime, transaction.TxNumber, false);
                case TransactionState.Implicit:
                    return new FindRequest(_collectionNamespace.CollectionName, filter, _limit, default, null, _collectionNamespace.DatabaseName, transaction.SessionId);
                case TransactionState.Committed:
                    return ThrowEx<FindRequest>("Transaction already commited");
                case TransactionState.Aborted:
                    return ThrowEx<FindRequest>("Transaction already aborted");
                default:
                    return ThrowEx<FindRequest>("Invalid transaction state");
            }
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

        // TODO:
        [DoesNotReturn]
        private static void ReadPreferenceNotSupported(ReadPreference readPreference)
        {
            throw new Exception(readPreference.ToString() + " is not supported");
        }
    }
}
