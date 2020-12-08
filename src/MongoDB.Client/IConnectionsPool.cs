using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    internal interface IConnectionsPool
    {
        ValueTask<MongoConnection> GetChannelAsync(CancellationToken cancellationToken);
    }
}