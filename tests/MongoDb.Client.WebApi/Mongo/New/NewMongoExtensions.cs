using Microsoft.Extensions.DependencyInjection;

namespace MongoDb.Client.WebApi.Mongo.New
{
    public static class NewMongoExtensions
    {
        public static IServiceCollection AddNewMongoClient(this IServiceCollection services)
        {
            services.AddSingleton<INewMongo, NewMongo>();
            services.AddHostedService<MongoStarter>();
            services.AddSingleton<IMongoRepository<GeoIp>, NewGeoipRepository>();

            return services;
        }
    }
}
