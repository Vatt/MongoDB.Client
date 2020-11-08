using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
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

        public async ValueTask InsertAsync(T item)
        {
            var list = ListsPool<T>.Pool.Get();
            try
            {
                list.Add(item);
                await InsertAsync(list).ConfigureAwait(false);
            }
            finally
            {
                ListsPool<T>.Pool.Return(list);
            }
        }
        
        public ValueTask InsertAsync(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }
    }
}
