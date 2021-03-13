using MongoDB.Client.Bson.Document;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
            var requestNum = _scheduler.GetNextRequestNumber();
            var requestDocument = CreateFindRequest(_filter, _transaction);
            var request = new FindMessage(requestNum, requestDocument);
            var result = await _scheduler.GetCursorAsync<T>(request, cancellationToken).ConfigureAwait(false);
            if (result.ErrorMessage is not null)
            {
                ThrowHelper.CursorException(result.ErrorMessage);
            }
            foreach (var item in result.MongoCursor.Items)
            {
                yield return item;
            }

            ListsPool<T>.Pool.Return(result.MongoCursor.Items);
            long cursorId = result.MongoCursor.Id;
            while (cursorId != 0)
            {
                requestNum = _scheduler.GetNextRequestNumber();
                requestDocument = CreateGetMoreRequest(cursorId, _transaction);
                request = new FindMessage(requestNum, requestDocument);
                var getMoreResult = await _scheduler.GetCursorAsync<T>(request, cancellationToken).ConfigureAwait(false);
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

        public IAsyncEnumerator<T> GetAsyncEnumerator2(CancellationToken cancellationToken)
        {
            return new AsyncEnumerator<T>(this, cancellationToken);
        }

        public async ValueTask<List<T>> GetNextBatchAsync(CancellationToken cancellationToken)
        {
            //if (_channel is null)
            //{
            //    _channel = await _channelPool.GetChannelAsync(cancellationToken).ConfigureAwait(false);
            //}            

            var requestNum = _scheduler.GetNextRequestNumber();
            var requestDocument = _cursorId == -1 ? CreateFindRequest(_filter, _transaction) : CreateGetMoreRequest(_cursorId, _transaction);
            var request = new FindMessage(requestNum, requestDocument);
            var result = await _scheduler.GetCursorAsync<T>(request, cancellationToken).ConfigureAwait(false);
            _cursorId = result.MongoCursor.Id;
            return result.MongoCursor.Items;
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
                    return new FindRequest(_collectionNamespace.CollectionName, filter, _limit, default, null, _collectionNamespace.DatabaseName, transaction.SessionId, transaction.TxNumber);
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
    }
}