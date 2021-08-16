using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Utils;

namespace MongoDB.Client
{
    public class MongoCollection<T> where T : IBsonSerializer<T>
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
            return new Cursor<T>(transaction, _scheduler, filter, Namespace);
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

        public ValueTask InsertAsync(TransactionHandler transaction, IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            return _scheduler.InsertAsync(transaction, items, Namespace, cancellationToken);
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

        private ValueTask<DeleteResult> DeleteAsync(TransactionHandler transaction, BsonDocument filter, int limit, CancellationToken cancellationToken = default)
        {
            return _scheduler.DeleteAsync(transaction, filter, limit, Namespace, cancellationToken);
        }

        internal ValueTask DropAsync(CancellationToken cancellationToken = default)
        {
            return DropAsync(TransactionHandler.CreateImplicit(_scheduler), cancellationToken);
        }

        internal ValueTask DropAsync(TransactionHandler transaction, CancellationToken cancellationToken = default)
        {
            return _scheduler.DropCollectionAsync(transaction, Namespace, cancellationToken);
        }

        internal ValueTask CreateAsync(CancellationToken cancellationToken = default)
        {
            return CreateAsync(TransactionHandler.CreateImplicit(_scheduler), cancellationToken);
        }

        internal ValueTask CreateAsync(TransactionHandler transaction, CancellationToken cancellationToken = default)
        {
            return _scheduler.CreateCollectionAsync(transaction, Namespace, cancellationToken);
        }
    }
}
