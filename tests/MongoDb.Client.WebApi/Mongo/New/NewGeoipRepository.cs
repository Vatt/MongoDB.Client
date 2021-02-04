using MongoDB.Client;
using MongoDB.Client.Bson.Document;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoDb.Client.WebApi.Mongo
{
    public class NewGeoipRepository : IMongoRepository<GeoIp>
    {
        private readonly MongoCollection<GeoIp> _collection;

        public NewGeoipRepository(INewMongo mongo)
        {
            _collection = mongo.GetCollection<GeoIp>("GeoipCollection");
        }

        public IAsyncEnumerable<GeoIp> GetAllAsync()
        {
            return _collection.Find(BsonDocument.Empty);
        }

        public ValueTask<GeoIp> GetAsync(string id)
        {
            return _collection.Find(new BsonDocument("_id", new BsonObjectId(id))).FirstOrDefaultAsync();
        }

        public ValueTask InsertAsync(GeoIp geoIp)
        {
            return _collection.InsertAsync(geoIp);
        }

        public async ValueTask DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(new BsonDocument("_id", new BsonObjectId(id)));
        }
    }
}
