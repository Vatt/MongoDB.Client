using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Client.Connection;
using MongoDB.Client.Scheduler;
using System.Net;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    public class MongoClient
    {
        public MongoClientSettings Settings { get; }
        private readonly StandaloneScheduler _scheduler;

        internal MongoClient(MongoClientSettings settings, IMongoConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
        {
            Settings = settings;
            _scheduler = new StandaloneScheduler(settings, connectionFactory, loggerFactory);
        }

        public MongoClient()
         : this(new MongoClientSettings(), new NullLoggerFactory())
        {
        }

        public MongoClient(EndPoint endPoint)
        : this(new MongoClientSettings(endPoint), new NullLoggerFactory())
        {
        }

        public MongoClient(ILoggerFactory loggerFactory)
            : this(new MongoClientSettings(), loggerFactory)
        {
        }

        public MongoClient(EndPoint endPoint, ILoggerFactory loggerFactory)
            : this(new MongoClientSettings(endPoint), loggerFactory)
        {
        }

        public MongoClient(MongoClientSettings settings, ILoggerFactory loggerFactory)
        {
            Settings = settings;
            _scheduler = new StandaloneScheduler(settings, new MongoConnectionFactory(settings.Endpoints[0], loggerFactory), loggerFactory);
        }

        public MongoClient(string connectionString, ILoggerFactory loggerFactory)
            : this(MongoClientSettings.FromConnectionString(connectionString), loggerFactory)
        {

        }
        public MongoDatabase GetDatabase(string name)
        {
            return new MongoDatabase(this, name, _scheduler);
        }
        public ValueTask InitAsync()
        {
            return _scheduler.InitAsync();
        }
    }
}
