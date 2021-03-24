using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Connection;

namespace MongoDB.Client.Experimental
{
    internal static class MongoConnectionExt
    {
        internal static ValueTask StartAsyncExperimental(this MongoConnection mongoConnection, System.Net.Connections.Connection connection, CancellationToken cancellationToken = default)
        {
            return mongoConnection.StartAsync(connection.CreateReader(), connection.CreateWriter(), cancellationToken);
        }
    }
}
