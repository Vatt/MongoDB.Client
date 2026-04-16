using MongoDB.Client.Settings;

namespace MongoDB.Client.Connection
{
    internal static class MongoConnectionInitializerFactory
    {
        public static IMongoConnectionInitializer Create(
            MongoClientSettings settings,
            params IMongoConnectionInitializerPlugin[] plugins)
        {
            return new MongoConnectionInitializer(settings, plugins);
        }
    }
}
