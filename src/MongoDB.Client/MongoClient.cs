using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Client.Connection;
using System.Net;

namespace MongoDB.Client
{
    public class MongoClient
    {
        public MongoClientSettings Settings { get; }
        private readonly RequestScheduler _scheduler;
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
            //_channelsPool = new NodeConnectionsPool(settings, settings.Endpoints[0], loggerFactory);
            _scheduler = new RequestScheduler(new MongoConnectionFactory(settings.Endpoints[0], loggerFactory));
        }

        public MongoDatabase GetDatabase(string name)
        {
            return new MongoDatabase(this, name, _scheduler);
        }
    }
}
