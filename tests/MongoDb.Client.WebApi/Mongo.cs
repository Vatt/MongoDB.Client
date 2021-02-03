using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Client;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Messages;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDb.Client.WebApi
{
    public class Mongo : IMongo
    {
        private MongoCollection<GeoIp> _collection;
        private readonly IOptions<MongoConfig> _options;
        private readonly ILoggerFactory _loggerFactory;

        public Mongo(IOptions<MongoConfig> options, ILoggerFactory loggerFactory)
        {
            _options = options;
            _loggerFactory = loggerFactory;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var client = new MongoClient(new DnsEndPoint(_options.Value.ConnectionString, 27017), _loggerFactory);
            await client.InitAsync();
            var db = client.GetDatabase("WebApiDb");
            _collection = db.GetCollection<GeoIp>("WebApiCollection");
        }

        public IAsyncEnumerable<GeoIp> GetAllAsync()
        {
            return _collection.Find(BsonDocument.Empty);
        }

        public ValueTask<GeoIp> GetAsync(BsonObjectId id)
        {
            return _collection.Find(new BsonDocument("_id", id)).FirstOrDefaultAsync();
        }

        public ValueTask InsertAsync(GeoIp geoIp)
        {
            return _collection.InsertAsync(geoIp);
        }

        public ValueTask<DeleteResult> DeleteAsync(BsonObjectId id)
        {
            return _collection.DeleteOneAsync(new BsonDocument("_id", id));
        }
    }
}
