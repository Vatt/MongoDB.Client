using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal interface IMongoConnectionFactory
    {
        ValueTask<MongoConnection> CreateAsync(MongoClientSettings settings);
    }
}
