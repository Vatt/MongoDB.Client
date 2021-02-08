using Microsoft.Extensions.Logging;
using System.Net;


namespace MongoDB.Client.Experimental
{
    public static class MongoExperimental
    {        
        public static MongoClient CreateWithExperimentalConnection(EndPoint endPoint, ILoggerFactory loggerFactory)
        {
            return CreateWithExperimentalConnection(new MongoClientSettings(endPoint), loggerFactory);
        }
        public static MongoClient CreateWithExperimentalConnection(MongoClientSettings settings, ILoggerFactory loggerFactory)
        {
            return new MongoClient(settings, new ExperimentalMongoConnectionFactory(settings.Endpoints[0], loggerFactory), loggerFactory);
        }
    }
}
