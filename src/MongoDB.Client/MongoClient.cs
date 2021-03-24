using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Client.Connection;
using MongoDB.Client.Experimental;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;

namespace MongoDB.Client
{
    public class MongoClient
    {
        public MongoClientSettings Settings { get; }
        private readonly IMongoScheduler _scheduler;


        internal MongoClient(MongoClientSettings settings, ILoggerFactory loggerFactory)
        {
            Settings = settings;
            if (Settings.Endpoints.Length > 1)
            {
                _scheduler = new ReplicaSetScheduler(Settings, loggerFactory);
            }
            else
            {
                IMongoConnectionFactory connectionFactory = settings.ClientType == ClientType.Default ? new MongoConnectionFactory(settings.Endpoints[0], loggerFactory) : new ExperimentalMongoConnectionFactory(settings.Endpoints[0], loggerFactory);
                _scheduler = new StandaloneScheduler(settings, connectionFactory, loggerFactory);
            }
        }


        public MongoClient()
         : this(new MongoClientSettings(), new NullLoggerFactory())
        {
        }

        public MongoClient(MongoClientSettings settings)
            : this(settings, new NullLoggerFactory())
        {
        }

        public MongoClient(string connectionString)
            : this(MongoClientSettings.FromConnectionString(connectionString))
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

        public MongoClient(string connectionString, ILoggerFactory loggerFactory)
            : this(MongoClientSettings.FromConnectionString(connectionString), loggerFactory)
        {

        }

        public MongoClient(EndPoint endPoint, ILoggerFactory loggerFactory)
            : this(new MongoClientSettings(endPoint), loggerFactory)
        {
        }


        public MongoDatabase GetDatabase(string name)
        {
            return new MongoDatabase(this, name, _scheduler);
        }


        public ValueTask InitAsync(CancellationToken token)
        {
            return _scheduler.StartAsync(token);
        }


        public TransactionHandler StartTransaction()
        {
            return TransactionHandler.Create(_scheduler);
        }

        public static async Task<MongoClient> CreateClient(MongoClientSettings settings, ILoggerFactory? loggerFactory = null, CancellationToken token = default)
        {
            loggerFactory ??= new NullLoggerFactory();
            var client = new MongoClient(settings, loggerFactory);
            await client.InitAsync(token);
            return client;
        }

        public static async Task<MongoClient> CreateClient(string connectionString, ILoggerFactory? loggerFactory = null, CancellationToken token = default)
        {
            loggerFactory ??= new NullLoggerFactory();
            var client = new MongoClient(connectionString, loggerFactory);
            await client.InitAsync(token);
            return client;
        }

        public static async Task<MongoClient> CreateClient(EndPoint endPoint, ILoggerFactory? loggerFactory = null, CancellationToken token = default)
        {
            loggerFactory ??= new NullLoggerFactory();
            var client = new MongoClient(endPoint, loggerFactory);
            await client.InitAsync(token);
            return client;
        }
    }
}
