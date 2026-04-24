using MongoDB.Client.Connection;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Experimental
{
    internal static class MongoConnectionExt
    {
        internal static ValueTask<ConnectionInfo> StartAsyncExperimental(this MongoConnection mongoConnection, IMongoConnectionInitializer initializer, System.Net.Connections.Connection connection, CancellationToken cancellationToken = default)
        {
            return mongoConnection.StartAsync(initializer, connection.CreateReader(), connection.CreateWriter(), connection, cancellationToken);
        }
    }
}
