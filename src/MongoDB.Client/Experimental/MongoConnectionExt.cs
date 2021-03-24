using MongoDB.Client.Authentication;
using MongoDB.Client.Connection;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Experimental
{
    internal static class MongoConnectionExt
    {
        internal static ValueTask<ConnectionInfo> StartAsyncExperimental(this MongoConnection mongoConnection, ScramAuthenticator authenticator, System.Net.Connections.Connection connection, CancellationToken cancellationToken = default)
        {
            return mongoConnection.StartAsync(authenticator, connection.CreateReader(), connection.CreateWriter(), cancellationToken);
        }
    }
}
