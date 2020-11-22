using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Utils;

namespace MongoDB.Client
{
    public class Cursor<T> : IAsyncEnumerable<T>
    {
        private readonly IChannelsPool _channelPool;
        private readonly BsonDocument _filter;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly BsonDocument _sessionId;
        private int _limit;
        private static readonly BsonDocument SharedSessionId = new BsonDocument("id", BsonBinaryData.Create(Guid.NewGuid()));

        public static readonly SessionId SharedSession = new SessionId();

        internal Cursor(IChannelsPool channelPool, BsonDocument filter, CollectionNamespace collectionNamespace)
            : this(channelPool, filter, collectionNamespace, SharedSessionId)
        {
        }

        internal Cursor(IChannelsPool channelPool, BsonDocument filter, CollectionNamespace collectionNamespace, Guid sessionId)
            : this(channelPool, filter, collectionNamespace, new BsonDocument("id", BsonBinaryData.Create(sessionId)))
        {
        }
        
        internal Cursor(IChannelsPool channelPool, BsonDocument filter, CollectionNamespace collectionNamespace,
            BsonDocument sessionId)
        {
            _channelPool = channelPool;
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
            var channel = await _channelPool.GetChannelAsync(cancellationToken).ConfigureAwait(false);
            var requestNum = channel.GetNextRequestNumber();
            var requestDocument = CreateFindRequest(_filter);
            var request = new FindMessage(requestNum, requestDocument);
            var result = await channel.GetCursorAsync<T>(request, cancellationToken).ConfigureAwait(false);
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
                requestNum = channel.GetNextRequestNumber();
                requestDocument = CreateGetMoreRequest(cursorId);
                request = new FindMessage(requestNum, requestDocument);
                var getMoreResult = await channel.GetCursorAsync<T>(request, cancellationToken).ConfigureAwait(false);
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

        private Channel _channel;
        private long _cursorId = -1;

        public bool HasNext => _cursorId != 0;

        public IAsyncEnumerator<T> GetAsyncEnumerator2(CancellationToken cancellationToken)
        {
            return new AsyncEnumerator<T>(this, cancellationToken);
        }

        public async ValueTask<List<T>> GetNextBatchAsync(CancellationToken cancellationToken)
        {
            if (_channel is null)
            {
                _channel = await _channelPool.GetChannelAsync(cancellationToken).ConfigureAwait(false);
            }


            var requestNum = _channel.GetNextRequestNumber();
            var requestDocument = _cursorId == -1 ? CreateFindRequest(_filter) : CreateGetMoreRequest(_cursorId);
            var request = new FindMessage(requestNum, requestDocument);
            var result = await _channel.GetCursorAsync<T>(request, cancellationToken).ConfigureAwait(false);
            _cursorId = result.MongoCursor.Id;
            return result.MongoCursor.Items;
        }

        private FindRequest CreateFindRequest(BsonDocument filter)
        {
            return new FindRequest
            {
                Find = _collectionNamespace.CollectionName,
                Filter =  filter,
                Limit = _limit,
                Db = _collectionNamespace.DatabaseName,
                Lsid = SharedSession
            };
        }

        private FindRequest CreateGetMoreRequest(long cursorId)
        {
            return new FindRequest
            {
                GetMore = cursorId,
                Collection = _collectionNamespace.CollectionName,
                Db = _collectionNamespace.DatabaseName,
                Lsid = SharedSession
            };
        }
    }
}