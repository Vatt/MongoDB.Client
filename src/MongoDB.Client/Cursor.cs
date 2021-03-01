using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    public class Cursor<T> : IAsyncEnumerable<T>
    {
        private readonly StandaloneScheduler _scheduler;
        private readonly BsonDocument _filter;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly BsonDocument _sessionId;
        private int _limit;
        private static readonly BsonDocument SharedSessionId = new BsonDocument("id", BsonBinaryData.Create(Guid.NewGuid()));

        public static readonly SessionId SharedSession = new SessionId();

        internal Cursor(StandaloneScheduler channelPool, BsonDocument filter, CollectionNamespace collectionNamespace)
            : this(channelPool, filter, collectionNamespace, SharedSessionId)
        {
        }

        internal Cursor(StandaloneScheduler channelPool, BsonDocument filter, CollectionNamespace collectionNamespace, Guid sessionId)
            : this(channelPool, filter, collectionNamespace, new BsonDocument("id", BsonBinaryData.Create(sessionId)))
        {
        }
        internal Cursor(StandaloneScheduler scheduler, BsonDocument filter, CollectionNamespace collectionNamespace, BsonDocument sessionId)
        {
            _scheduler = scheduler;
            _filter = filter;
            _collectionNamespace = collectionNamespace;
            _sessionId = sessionId;
        }

        internal void AddLimit(int limit)
        {
            _limit = limit;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            var requestNum = _scheduler.GetNextRequestNumber();
            var requestDocument = CreateFindRequest(_filter);
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
                requestDocument = CreateGetMoreRequest(cursorId);
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
            var requestDocument = _cursorId == -1 ? CreateFindRequest(_filter) : CreateGetMoreRequest(_cursorId);
            var request = new FindMessage(requestNum, requestDocument);
            var result = await _scheduler.GetCursorAsync<T>(request, cancellationToken).ConfigureAwait(false);
            _cursorId = result.MongoCursor.Id;
            return result.MongoCursor.Items;
        }

        private FindRequest CreateFindRequest(BsonDocument filter)
        {
            return new FindRequest(_collectionNamespace.CollectionName, filter, _limit, default, null,  _collectionNamespace.DatabaseName, SharedSession);
        }

        private FindRequest CreateGetMoreRequest(long cursorId)
        {
            return new FindRequest(null, null, default, cursorId, _collectionNamespace.CollectionName, _collectionNamespace.DatabaseName, SharedSession);
        }
    }
}