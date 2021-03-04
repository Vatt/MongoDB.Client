using MongoDB.Client.Scheduler;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal interface IMongoConnectionFactory
    {
        ValueTask<MongoConnection> CreateAsync(MongoClientSettings settings, ChannelReader<MongoRequest> reader, ChannelReader<MongoRequest> findReader, IMongoScheduler requestScheduler);
    }
}
