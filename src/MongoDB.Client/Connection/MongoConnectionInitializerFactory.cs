using MongoDB.Client.Settings;

namespace MongoDB.Client.Connection
{
    internal static class MongoConnectionInitializerFactory
    {
        public static IMongoConnectionInitializer Create(
            MongoClientSettings settings,
            params IMongoConnectionInitializerPlugin[] plugins)
        {
            if (plugins.Length == 0)
            {
                throw new ArgumentException(
                    "At least one connection initializer plugin is required. Use CreateRaw for an explicit no-auth initializer.",
                    nameof(plugins));
            }

            return new MongoConnectionInitializer(settings, plugins);
        }

        public static IMongoConnectionInitializer CreateRaw(MongoClientSettings settings)
        {
            return MongoConnectionInitializer.CreateRaw(settings);
        }
    }
}
