using System.Threading.Channels;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Connection
{
    internal interface IMongoConnectionFactory
    {
        ValueTask<MongoConnection> CreateAsync(MongoClientSettings settings, ChannelReader<MongoRequest> reader, MongoScheduler requestScheduler, CancellationToken token);
    }
}
