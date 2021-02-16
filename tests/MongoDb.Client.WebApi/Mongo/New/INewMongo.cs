using MongoDB.Client;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDb.Client.WebApi
{
    public interface INewMongo
    {
        Task StartAsync(CancellationToken cancellationToken);
        public MongoCollection<T> GetCollection<T>(string name);
    }
}