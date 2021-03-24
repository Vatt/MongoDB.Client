using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDb.Client.WebApi.Mongo
{
    public class BaseOldRepository<T> : IMongoRepository<T>
    {
        private readonly IMongoCollection<T> _collection;

        public BaseOldRepository(IOptions<MongoConfig> options, string collectionName)
        {
            var client = new MongoClient($"mongodb://{options.Value.ConnectionString}:27017/");
            var db = client.GetDatabase("WebApiDb");
            _collection = db.GetCollection<T>(collectionName);
        }

        public async IAsyncEnumerable<T> GetAllAsync()
        {
            using var cursor = await _collection.FindAsync(FilterDefinition<T>.Empty);

            while (await cursor.MoveNextAsync(default).ConfigureAwait(false))
            {
                foreach (var item in cursor.Current)
                {
                    yield return item;
                }
            }
        }

        public async ValueTask<T> GetAsync(string id)
        {
            var bsonId = ObjectId.Parse(id);
            var result = await _collection.Find(new BsonDocument("_id", bsonId)).FirstOrDefaultAsync();
            return result;
        }

        public async ValueTask InsertAsync(T geoIp)
        {
            await _collection.InsertOneAsync(geoIp);
        }

        public async ValueTask DeleteAsync(string id)
        {
            var bsonId = ObjectId.Parse(id);
            await _collection.DeleteOneAsync(new BsonDocument("_id", bsonId));
        }
    }
}
