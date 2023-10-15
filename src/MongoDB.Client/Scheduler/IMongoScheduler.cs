using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Filters;
using MongoDB.Client.Messages;

namespace MongoDB.Client.Scheduler
{
    internal interface IMongoScheduler : IAsyncDisposable
    {
        ValueTask StartAsync(CancellationToken token);
        ValueTask<FindResult<T>> FindAsync<T>(Filter filter, int limit, CollectionNamespace collectionNamespace, TransactionHandler transaction, CancellationToken token) where T : IBsonSerializer<T>;
        ValueTask<CursorResult<T>> GetMoreAsync<T>(MongoScheduler scheduler, long cursorId, CollectionNamespace collectionNamespace, TransactionHandler transaction, CancellationToken token) where T : IBsonSerializer<T>;
        ValueTask InsertAsync<T>(TransactionHandler transaction, IEnumerable<T> items, CollectionNamespace collectionNamespace, CancellationToken token) where T : IBsonSerializer<T>;
        ValueTask<DeleteResult> DeleteAsync(TransactionHandler transaction, Filter filter, int limit, CollectionNamespace collectionNamespace, CancellationToken token);
        ValueTask<UpdateResult> UpdateAsync(TransactionHandler transaction, Filter filter, Update update, bool isMulty, CollectionNamespace collectionNamespace, UpdateOptions? options, CancellationToken token);
        ValueTask DropCollectionAsync(TransactionHandler transaction, CollectionNamespace collectionNamespace, CancellationToken token);
        ValueTask CreateCollectionAsync(TransactionHandler transaction, CollectionNamespace collectionNamespace, CancellationToken token);
        ValueTask CommitTransactionAsync(TransactionHandler transactionHandler, CancellationToken cancellationToken);
        ValueTask AbortTransactionAsync(TransactionHandler transactionHandler, CancellationToken cancellationToken);
    }
}
