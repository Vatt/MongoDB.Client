﻿using MongoDB.Client.Bson.Document;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    public class MongoCollection<T>
    {
        private readonly IMongoScheduler _scheduler;

        internal MongoCollection(MongoDatabase database, string name, IMongoScheduler scheduler)
        {
            _scheduler = scheduler;
            Database = database;
            Namespace = new CollectionNamespace(database.Name, name);
        }

        public MongoDatabase Database { get; }

        public CollectionNamespace Namespace { get; }

        public Cursor<T> Find(BsonDocument filter)
        {
            return Find(TransactionHandler.CreateImplicit(_scheduler), filter);
        }

        public Cursor<T> Find(TransactionHandler transaction, BsonDocument filter)
        {
            return new Cursor<T>(transaction, _scheduler, filter, Namespace, _scheduler.SessionId);
        }

        public ValueTask InsertAsync(T item, CancellationToken cancellationToken = default)
        {
            return InsertAsync(TransactionHandler.CreateImplicit(_scheduler), item, cancellationToken);
        }

        public async ValueTask InsertAsync(TransactionHandler transaction, T item, CancellationToken cancellationToken = default)
        {
            var list = ListsPool<T>.Pool.Get();
            try
            {
                list.Add(item);
                await InsertAsync(transaction, list, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ListsPool<T>.Pool.Return(list);
            }
        }

        public ValueTask InsertAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            return InsertAsync(TransactionHandler.CreateImplicit(_scheduler), items, cancellationToken);
        }

        public async ValueTask InsertAsync(TransactionHandler transaction, IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            var requestNumber = _scheduler.GetNextRequestNumber();
            var insertHeader = CreateInsertHeader(transaction);
            var request = new InsertMessage<T>(requestNumber, insertHeader, items);
            await _scheduler.InsertAsync(request, cancellationToken).ConfigureAwait(false);
        }
        
        private InsertHeader CreateInsertHeader(TransactionHandler transaction)
        {
            switch (transaction.State)
            {
                case TransactionState.Starting:
                    transaction.State = TransactionState.InProgress;
                    return new InsertHeader(Namespace.CollectionName, true, Namespace.DatabaseName, _scheduler.SessionId, _scheduler.ClusterTime, transaction.TxNumber, true, false);
                case TransactionState.InProgress:
                    return new InsertHeader(Namespace.CollectionName, true, Namespace.DatabaseName, _scheduler.SessionId, _scheduler.ClusterTime, transaction.TxNumber, false);
                case TransactionState.Implicit:
                    return new InsertHeader(Namespace.CollectionName, true, Namespace.DatabaseName, _scheduler.SessionId, transaction.TxNumber);
                case TransactionState.Committed:
                    return ThrowEx<InsertHeader>("Transaction already commited");
                case TransactionState.Aborted:
                    return ThrowEx<InsertHeader>("Transaction already aborted");
                default:
                    return ThrowEx<InsertHeader>("Invalid transaction state");
            }
        }

        private static TMessage ThrowEx<TMessage>(string message)
        {
            throw new MongoException(message);
        }

        public ValueTask<DeleteResult> DeleteOneAsync(BsonDocument filter, CancellationToken cancellationToken = default)
        {
            return DeleteOneAsync(TransactionHandler.CreateImplicit(_scheduler), filter, cancellationToken);
        }

        public ValueTask<DeleteResult> DeleteOneAsync(TransactionHandler transaction, BsonDocument filter, CancellationToken cancellationToken = default)
        {
            return DeleteAsync(transaction, filter, 1, cancellationToken);
        }

        public ValueTask<DeleteResult> DeleteManyAsync(BsonDocument filter, CancellationToken cancellationToken = default)
        {
            return DeleteManyAsync(TransactionHandler.CreateImplicit(_scheduler), filter, cancellationToken);
        }

        public ValueTask<DeleteResult> DeleteManyAsync(TransactionHandler transaction, BsonDocument filter, CancellationToken cancellationToken = default)
        {
            return DeleteAsync(transaction, filter, 0, cancellationToken);
        }

        private async ValueTask<DeleteResult> DeleteAsync(TransactionHandler transaction, BsonDocument filter, int limit, CancellationToken cancellationToken = default)
        {
            var requestNumber = _scheduler.GetNextRequestNumber();
            var deleteHeader = CreateDeleteHeader(transaction);

            var deleteBody = new DeleteBody(filter, limit);

            var request = new DeleteMessage(requestNumber, deleteHeader, deleteBody);
            return await _scheduler.DeleteAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private DeleteHeader CreateDeleteHeader(TransactionHandler transaction)
        {
            switch (transaction.State)
            {
                case TransactionState.Starting:
                    transaction.State = TransactionState.InProgress;
                    return new DeleteHeader(Namespace.CollectionName, true, Namespace.DatabaseName, _scheduler.SessionId, _scheduler.ClusterTime, transaction.TxNumber, true, false);
                case TransactionState.InProgress:
                    return new DeleteHeader(Namespace.CollectionName, true, Namespace.DatabaseName, _scheduler.SessionId, _scheduler.ClusterTime, transaction.TxNumber, false);
                case TransactionState.Implicit:
                    return new DeleteHeader(Namespace.CollectionName, true, Namespace.DatabaseName, _scheduler.SessionId, transaction.TxNumber);
                case TransactionState.Committed:
                    return ThrowEx<DeleteHeader>("Transaction already commited");
                case TransactionState.Aborted:
                    return ThrowEx<DeleteHeader>("Transaction already aborted");
                default:
                    return ThrowEx<DeleteHeader>("Invalid transaction state");
            }
        }

        internal async ValueTask DropAsync(CancellationToken cancellationToken = default)
        {
            var requestNumber = _scheduler.GetNextRequestNumber();
            var dropCollectionHeader = new DropCollectionHeader(Namespace.CollectionName, Namespace.DatabaseName, _scheduler.SessionId);
            var request = new DropCollectionMessage(requestNumber, dropCollectionHeader);
            await _scheduler.DropCollectionAsync(request, cancellationToken).ConfigureAwait(false);
        }

        internal async ValueTask CreateAsync(CancellationToken cancellationToken = default)
        {
            var requestNumber = _scheduler.GetNextRequestNumber();
            var createCollectionHeader = new CreateCollectionHeader(Namespace.CollectionName, Namespace.DatabaseName, _scheduler.SessionId);
            var request = new CreateCollectionMessage(requestNumber, createCollectionHeader);
            await _scheduler.CreateCollectionAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}