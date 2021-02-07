using MongoDB.Client.Connection;
using System.Threading;
using System.Threading.Tasks;

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
