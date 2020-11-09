using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Utils;

namespace MongoDB.Client
{
    public class MongoCollection<T>
    {
        private readonly IChannelsPool _channelsPool;

        internal MongoCollection(MongoDatabase database, string name, IChannelsPool channelsPool)
        {
            _channelsPool = channelsPool;
            Database = database;
            Namespace = new CollectionNamespace(database.Name, name);
        }
        
        public MongoDatabase Database { get; }

        public CollectionNamespace Namespace { get; }

        public Cursor<T> Find(BsonDocument filter)
        {
            return new Cursor<T>(_channelsPool, filter, Namespace);
        }

        public async ValueTask InsertAsync(T item, CancellationToken cancellationToken = default)
        {
            var list = ListsPool<T>.Pool.Get();
            try
            {
                list.Add(item);
                await InsertAsync(list, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ListsPool<T>.Pool.Return(list);
            }
        }
        
        public async ValueTask InsertAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            var channel = await _channelsPool.GetChannelAsync(cancellationToken).ConfigureAwait(false);
            var requestNumber = channel.GetNextRequestNumber();
            var document = new BsonDocument
            {
                {"insert", Namespace.CollectionName},
                {"ordered" , true },
                {"$db", Namespace.DatabaseName},
                {"lsid", SharedSessionId}
            };
            var request = new InsertMessage<T>(requestNumber, document, items);
            await channel.InsertAsync(request, cancellationToken).ConfigureAwait(false);
        }
        
        private static readonly BsonDocument SharedSessionId = new BsonDocument("id", BsonBinaryData.Create(Guid.NewGuid()));
    }
}
