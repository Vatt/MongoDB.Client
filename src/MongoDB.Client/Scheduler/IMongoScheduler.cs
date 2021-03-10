using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Scheduler
{
    internal interface IMongoScheduler : IAsyncDisposable
    {
        int GetNextRequestNumber();
        ValueTask StartAsync();
        ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token);
        ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token);
        ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken cancellationToken);
        ValueTask DropCollectionAsync(DropCollectionMessage message, CancellationToken cancellationToken);
        ValueTask CreateCollectionAsync(CreateCollectionMessage message, CancellationToken cancellationToken);
        Task ConnectionLost(MongoConnection connection);

        MongoClusterTime ClusterTime { get; }
    }
}
