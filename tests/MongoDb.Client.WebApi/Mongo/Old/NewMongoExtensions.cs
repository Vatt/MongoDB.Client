using Microsoft.Extensions.DependencyInjection;

namespace MongoDb.Client.WebApi.Mongo.New
{
    public static class OldMongoExtensions
    {
        public static IServiceCollection AddOldMongoClient(this IServiceCollection services)
        {
            services.AddSingleton<IMongoRepository<GeoIp>, OldGeoipRepository>();

            return services;
        }
    }
}
