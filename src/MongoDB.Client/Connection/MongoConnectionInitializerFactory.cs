using MongoDB.Client.Authentication;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Connection
{
    internal static class MongoConnectionInitializerFactory
    {
        public static IMongoConnectionInitializer Create(MongoClientSettings settings)
        {
            return new MongoConnectionInitializer(
                settings,
                new ScramMongoConnectionInitializerPlugin(new ScramAuthenticator(settings)));
        }
    }
}
