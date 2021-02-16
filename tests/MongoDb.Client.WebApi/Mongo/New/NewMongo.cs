using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Client;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

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
            var client = new MongoClient(new DnsEndPoint(_options.Value.ConnectionString, 27017), _loggerFactory);
            await client.InitAsync();
            _db = client.GetDatabase("WebApiDb");
        }

        public MongoCollection<T> GetCollection<T>(string name)
        {
            return _db.GetCollection<T>(name);
        }
    }
}
