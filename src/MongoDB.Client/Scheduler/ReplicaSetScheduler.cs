﻿using Microsoft.Extensions.Logging;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Experimental;
using MongoDB.Client.Messages;
using MongoDB.Client.Network;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Settings;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ReadPreference = MongoDB.Client.Settings.ReadPreference;

namespace MongoDB.Client.Scheduler
{
    internal sealed class ReplicaSetScheduler : IMongoScheduler
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly MongoClientSettings _settings;
        private readonly ILogger _logger;
        private readonly ImmutableArray<MongoScheduler> _shedulers;
        private readonly ImmutableArray<MongoScheduler> _serondaries;

        private MongoServiceConnection? _serviceConnection;

        private MongoScheduler? _primary;

        private MongoPingMessage? _lastPing;
        private int _schedulerCounter = 0;

        public MongoClusterTime ClusterTime => _lastPing?.ClusterTime!;

        public ReplicaSetScheduler(MongoClientSettings settings, ILoggerFactory loggerFactory)
        {
            _settings = settings;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ReplicaSetScheduler>();
            _shedulers = new();
            _serondaries = new();
        }


        private MongoScheduler GetSecondaryScheduler()
        {
            var counter = Interlocked.Increment(ref _schedulerCounter);
            var secondary = _serondaries;
            var schedulerId = counter % secondary.Length;
            var result = secondary[schedulerId];
            return result;
        }


        public async ValueTask StartAsync(CancellationToken token)
        {
            var endpoints = _settings.Endpoints;
            var maxConnections = _settings.ConnectionPoolMaxSize / endpoints.Length;
            maxConnections = maxConnections == 0 ? 1 : maxConnections;
            _serviceConnection = await CreateServiceConnection(token).ConfigureAwait(false);
            _lastPing = await _serviceConnection.MongoPing(token).ConfigureAwait(false);
            if (_lastPing.Primary is null)
            {
                ThrowHelper.PrimaryNullExceptions(); //TODO: fixit
            }
            for (var i = 0; i < _lastPing.Hosts.Count; i++)
            {
                var host = _lastPing.Hosts[i];
                IMongoConnectionFactory connectionFactory = _settings.ClientType == ClientType.Default ? new MongoConnectionFactory(host, _loggerFactory) : new ExperimentalMongoConnectionFactory(host, _loggerFactory);
                var scheduler = new MongoScheduler(_settings with { ConnectionPoolMaxSize = maxConnections }, connectionFactory, _loggerFactory, _lastPing.ClusterTime);
                _shedulers.Add(scheduler);
                if (host.Equals(_lastPing.Primary))
                {
                    _primary = scheduler;
                }
                else
                {
                    _serondaries.Add(scheduler);
                }
                await scheduler.StartAsync(token);
            }
        }

        private async Task<MongoServiceConnection> CreateServiceConnection(CancellationToken token)
        {
            var _connectionfactory = new NetworkConnectionFactory(_loggerFactory);
            for (int i = 0; i < _settings.Endpoints.Length; i++)
            {
                try
                {
                    var ctx = await _connectionfactory.ConnectAsync(_settings.Endpoints[i], token);
                    var serviceConnection = new MongoServiceConnection(ctx);
                    await serviceConnection.Connect(_settings, token).ConfigureAwait(false);
                    return serviceConnection;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return ThrowHelper.MongoInitExceptions<MongoServiceConnection>();
        }

        public ValueTask DropCollectionAsync(DropCollectionMessage message, CancellationToken token)
        {
            return _primary!.DropCollectionAsync(message, token);
        }

        public ValueTask CreateCollectionAsync(CreateCollectionMessage message, CancellationToken token)
        {
            return _primary!.CreateCollectionAsync(message, token);
        }


        public ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken cancellationToken)
        {
            return _primary!.DeleteAsync(message, cancellationToken);
        }


        public async ValueTask<FindResult<T>> FindAsync<T>(BsonDocument filter, int limit, CollectionNamespace collectionNamespace, TransactionHandler transaction, CancellationToken token)
        {
            var readPreferces = transaction.State == TransactionState.Implicit ? _settings.ReadPreference : ReadPreference.Primary;
            var scheduler = GetScheduler(readPreferces);
            var requestNum = scheduler.GetNextRequestNumber();
            var requestDocument = CreateFindRequest(filter, limit, collectionNamespace, transaction, _lastPing!.ClusterTime);
            if (ReferenceEquals(scheduler, _primary) == false)
            {
                requestDocument.ReadPreference = new Messages.ReadPreference(readPreferces);
            }
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
        {
            var requestNum = scheduler.GetNextRequestNumber();
            var requestDocument = CreateGetMoreRequest(cursorId, collectionNamespace, transaction, _lastPing!.ClusterTime);
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
                    return new FindRequest(null, null, default, cursorId, null, collectionNamespace.DatabaseName, transaction.SessionId, transaction.TxNumber);
                case TransactionState.Committed:
                    return ThrowEx<FindRequest>("Transaction already commited");
                case TransactionState.Aborted:
                    return ThrowEx<FindRequest>("Transaction already aborted");
                default:
                    return ThrowEx<FindRequest>("Invalid transaction state");
            }
        }

        public ValueTask InsertAsync<T>(TransactionHandler transaction, IEnumerable<T> items, CollectionNamespace collectionNamespace, CancellationToken token)
        {
            var scheduler = _primary!;
            var requestNumber = scheduler.GetNextRequestNumber();
            var insertHeader = CreateInsertHeader(collectionNamespace, transaction, _lastPing!.ClusterTime);
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

        public ValueTask<DeleteResult> DeleteAsync(TransactionHandler transaction, BsonDocument filter, int limit, CollectionNamespace collectionNamespace, CancellationToken token)
        {
            var scheduler = _primary!;
            var requestNumber = scheduler.GetNextRequestNumber();
            var deleteHeader = CreateDeleteHeader(collectionNamespace, transaction, _lastPing!.ClusterTime);

            var deleteBody = new DeleteBody(filter, limit);

            var request = new DeleteMessage(requestNumber, deleteHeader, deleteBody);
            return scheduler.DeleteAsync(request, token);
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

        public ValueTask DropCollectionAsync(TransactionHandler transaction, CollectionNamespace collectionNamespace, CancellationToken token)
        {
            var scheduler = _primary!;
            var requestNumber = scheduler.GetNextRequestNumber();
            var dropCollectionHeader = new DropCollectionHeader(collectionNamespace.CollectionName, collectionNamespace.DatabaseName, transaction.SessionId);
            var request = new DropCollectionMessage(requestNumber, dropCollectionHeader);
            return scheduler.DropCollectionAsync(request, token);
        }

        public ValueTask CreateCollectionAsync(TransactionHandler transaction, CollectionNamespace collectionNamespace, CancellationToken token)
        {
            var scheduler = _primary!;
            var requestNumber = scheduler.GetNextRequestNumber();
            var createCollectionHeader = new CreateCollectionHeader(collectionNamespace.CollectionName, collectionNamespace.DatabaseName, transaction.SessionId);
            var request = new CreateCollectionMessage(requestNumber, createCollectionHeader);
            return scheduler.CreateCollectionAsync(request, token);
        }

        private MongoScheduler GetScheduler(ReadPreference readPreference)
        {
            MongoScheduler? scheduler = null;
            switch (readPreference)
            {
                case ReadPreference.Primary:
                    scheduler = _primary;
                    break;
                case ReadPreference.PrimaryPreferred:
                    scheduler = _primary;
                    if (scheduler is null)
                    {
                        scheduler = GetSecondaryScheduler();
                    }
                    break;
                case ReadPreference.Secondary:
                    scheduler = GetSecondaryScheduler();
                    break;
                case ReadPreference.SecondaryPreferred:
                    scheduler = GetSecondaryScheduler();
                    if (scheduler is null)
                    {
                        scheduler = _primary;
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


        public ValueTask TransactionAsync(TransactionMessage message, CancellationToken token)
        {
            return _primary!.TransactionAsync(message, token);
        }


        public async ValueTask DisposeAsync()
        {
            foreach (var scheduler in _shedulers)
            {
                await scheduler.DisposeAsync();
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
