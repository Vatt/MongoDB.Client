using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MongoDB.Client
{
    public class MongoClient
    {
        private readonly ILogger _logger;
        public EndPoint EndPoint { get; }

        public MongoClient()
         : this(new IPEndPoint(IPAddress.Loopback, 27017), new NullLoggerFactory())
        {
        }

        public MongoClient(EndPoint endPoint)
        : this(endPoint, new NullLoggerFactory())
        {
        }
        
        public MongoClient(ILoggerFactory loggerFactory)
            : this(new IPEndPoint(IPAddress.Loopback, 27017), loggerFactory)
        {
        }
        
        public MongoClient(EndPoint endPoint, ILoggerFactory loggerFactory)
        {
            EndPoint = endPoint;
            _logger = loggerFactory.CreateLogger($"MongoClient:{EndPoint.ToString()}");
        }

        public MongoDatabase GetDatabase(string name)
        {
            return new MongoDatabase(this, name, _logger);
        }
    }
}
