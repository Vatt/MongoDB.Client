using MongoDB.Client;
using MongoDB.Client.Bson.Document;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoDb.Client.WebApi.Mongo
{
    public class BaseNewRepository<T> : IMongoRepository<T>
    {
        private readonly MongoCollection<T> _collection;

        public BaseNewRepository(INewMongo mongo, string collectionName)
        {
            _collection = mongo.GetCollection<T>(collectionName);
        }

        public IAsyncEnumerable<T> GetAllAsync()
        {
            return _collection.Find(BsonDocument.Empty);
        }

        public ValueTask<T> GetAsync(string id)
        {
            return _collection.Find(new BsonDocument("_id", new BsonObjectId(id))).FirstOrDefaultAsync();
        }

        public ValueTask InsertAsync(T geoIp)
        {
            return _collection.InsertAsync(geoIp);
        }

        public async ValueTask DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(new BsonDocument("_id", new BsonObjectId(id)));
        }
    }
}
