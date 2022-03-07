using MongoDB.Client;
using MongoDB.Client.Bson.Serialization;

namespace MongoDb.Client.WebApi
{
    public interface INewMongo
    {
        Task StartAsync(CancellationToken cancellationToken);
        public MongoCollection<T> GetCollection<T>(string name);// where T : IBsonSerializer<T>;
    }
}
