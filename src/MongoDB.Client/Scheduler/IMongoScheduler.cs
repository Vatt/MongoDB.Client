using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Scheduler
{
    internal interface IMongoScheduler : IAsyncDisposable
    {
        ValueTask StartAsync();
        ValueTask<CursorResult<T>> FindAsync<T>(BsonDocument filter, int limit, CollectionNamespace collectionNamespace, TransactionHandler transaction, CancellationToken token);
        ValueTask<CursorResult<T>> GetMoreAsync<T>(long cursorId, CollectionNamespace collectionNamespace, TransactionHandler transaction, CancellationToken token);
        ValueTask InsertAsync<T>(TransactionHandler transaction, IEnumerable<T> items, CollectionNamespace collectionNamespace, CancellationToken token);
        ValueTask<DeleteResult> DeleteAsync(TransactionHandler transaction, BsonDocument filter, int limit, CollectionNamespace collectionNamespace, CancellationToken token);
        ValueTask DropCollectionAsync(TransactionHandler transaction, CollectionNamespace collectionNamespace, CancellationToken token);
        ValueTask CreateCollectionAsync(TransactionHandler transaction, CollectionNamespace collectionNamespace, CancellationToken token);
        ValueTask TransactionAsync(TransactionMessage message, CancellationToken token);
        Task ConnectionLost(MongoConnection connection);
    }
}
