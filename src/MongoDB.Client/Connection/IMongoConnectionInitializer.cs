using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal interface IMongoConnectionInitializer
    {
        ValueTask<ConnectionInfo> InitializeAsync(IMongoConnection connection, CancellationToken cancellationToken);
    }
}
