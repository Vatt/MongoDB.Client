using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Messages;

namespace MongoDB.Client.Scheduler
{
    internal interface IMongoScheduler : IAsyncDisposable
    {
        ValueTask StartAsync(CancellationToken token);
        ValueTask<FindResult<T>> FindAsync<T>(BsonDocument filter, int limit, CollectionNamespace collectionNamespace, TransactionHandler transaction, CancellationToken token);
        ValueTask<CursorResult<T>> GetMoreAsync<T>(MongoScheduler scheduler, long cursorId, CollectionNamespace collectionNamespace, TransactionHandler transaction, CancellationToken token);
        ValueTask InsertAsync<T>(TransactionHandler transaction, IEnumerable<T> items, CollectionNamespace collectionNamespace, CancellationToken token);
        ValueTask<DeleteResult> DeleteAsync(TransactionHandler transaction, BsonDocument filter, int limit, CollectionNamespace collectionNamespace, CancellationToken token);
        ValueTask DropCollectionAsync(TransactionHandler transaction, CollectionNamespace collectionNamespace, CancellationToken token);
        ValueTask CreateCollectionAsync(TransactionHandler transaction, CollectionNamespace collectionNamespace, CancellationToken token);
        ValueTask CommitTransactionAsync(TransactionHandler transactionHandler, CancellationToken cancellationToken);
        ValueTask AbortTransactionAsync(TransactionHandler transactionHandler, CancellationToken cancellationToken);
    }
}
