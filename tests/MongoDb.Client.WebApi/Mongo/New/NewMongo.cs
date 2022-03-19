using System.Net;
using Microsoft.Extensions.Options;
using MongoDB.Client;

namespace MongoDb.Client.WebApi
{
    public class NewMongo : INewMongo
    {
        private MongoDatabase _db;
        private readonly IOptions<MongoConfig> _options;
        private readonly ILoggerFactory _loggerFactory;

        public NewMongo(IOptions<MongoConfig> options, ILoggerFactory loggerFactory)
        {
            _options = options;
            _loggerFactory = loggerFactory;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var client = await MongoClient.CreateClient(new DnsEndPoint(_options.Value.ConnectionString, 27017));
            _db = client.GetDatabase("WebApiDb");
        }

        public MongoCollection<T> GetCollection<T>(string name) //where T : IBsonSerializer<T>
        {
            return _db.GetCollection<T>(name);
        }
    }
}
