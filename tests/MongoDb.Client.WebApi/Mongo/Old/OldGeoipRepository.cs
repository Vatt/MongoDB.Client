using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoDb.Client.WebApi.Mongo
{
    public class OldGeoipRepository : IMongoRepository<GeoIp>
    {
        private readonly IMongoCollection<GeoIp> _collection;

        public OldGeoipRepository(IOptions<MongoConfig> options)
        {
            var client = new MongoClient($"mongodb://{options.Value.ConnectionString}:27017/");
            var db = client.GetDatabase("WebApiDb");
            _collection = db.GetCollection<GeoIp>("GeoipCollection");
        }

        public async IAsyncEnumerable<GeoIp> GetAllAsync()
        {
            using var cursor = await _collection.FindAsync(FilterDefinition<GeoIp>.Empty);

            while (await cursor.MoveNextAsync(default).ConfigureAwait(false))
            {
                foreach (var item in cursor.Current)
                {
                    yield return item;
                }
            }
        }

        public async ValueTask<GeoIp> GetAsync(string id)
        {
            var bsonId = ObjectId.Parse(id);
            var result = await _collection.Find(new BsonDocument("_id", bsonId)).FirstOrDefaultAsync();
            return result;
        }

        public async ValueTask InsertAsync(GeoIp geoIp)
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
