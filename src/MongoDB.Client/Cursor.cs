using System.Collections.Generic;
using System.Threading;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Utils;

namespace MongoDB.Client
{
    public class Cursor<T> : IAsyncEnumerable<T>
    {
        private readonly TransactionHandler _transaction;
        private readonly IMongoScheduler _scheduler;
        private readonly BsonDocument _filter;
        private readonly CollectionNamespace _collectionNamespace;
        private int _limit;

        internal Cursor(TransactionHandler transaction, IMongoScheduler scheduler, BsonDocument filter, CollectionNamespace collectionNamespace)
        {
            _transaction = transaction;
            _scheduler = scheduler;
            _filter = filter;
            _collectionNamespace = collectionNamespace;
        }

        internal void AddLimit(int limit)
        {
            _limit = limit;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            var result = await _scheduler.FindAsync<T>(_filter, _limit, _collectionNamespace, _transaction, cancellationToken).ConfigureAwait(false);
            if (result.CursorResult.ErrorMessage is not null)
            {
                ThrowHelper.CursorException(result.CursorResult.ErrorMessage);
            }
            foreach (var item in result.CursorResult.MongoCursor.Items)
            {
                yield return item;
            }

            ListsPool<T>.Pool.Return(result.CursorResult.MongoCursor.Items);
            long cursorId = result.CursorResult.MongoCursor.Id;
            while (cursorId != 0)
            {
                var getMoreResult = await _scheduler.GetMoreAsync<T>(result.Scheduler, cursorId, _collectionNamespace, _transaction, cancellationToken).ConfigureAwait(false);
                if (getMoreResult.ErrorMessage is not null)
                {
                    ThrowHelper.CursorException(getMoreResult.ErrorMessage);
                }
                cursorId = getMoreResult.MongoCursor.Id;
                foreach (var item in getMoreResult.MongoCursor.Items)
                {
                    yield return item;
                }
                ListsPool<T>.Pool.Return(getMoreResult.MongoCursor.Items);
            }
        }

        private long _cursorId = -1;

        public bool HasNext => _cursorId != 0;
    }
}
