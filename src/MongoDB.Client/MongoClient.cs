using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MongoDB.Client
{
    public class MongoClient
    {
        public EndPoint EndPoint { get; }
        private readonly IChannelsPool _channelsPool;

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
            _channelsPool = new ChannelsPool(endPoint, loggerFactory);
        }

        public MongoDatabase GetDatabase(string name)
        {
            return new MongoDatabase(this, name, _channelsPool);
        }
    }
}
