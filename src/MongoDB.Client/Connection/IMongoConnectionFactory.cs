using MongoDB.Client.Authentication;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal interface IMongoConnectionFactory
    {
        ValueTask<MongoConnection> CreateAsync(MongoClientSettings settings, ScramAuthenticator authenticator, ChannelReader<MongoRequest> reader, ChannelReader<MongoRequest> findReader, MongoScheduler requestScheduler, CancellationToken token);
    }
}
