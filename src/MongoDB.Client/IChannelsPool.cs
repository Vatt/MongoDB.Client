using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    internal interface IChannelsPool
    {
        ValueTask<Channel> GetChannelAsync(CancellationToken cancellationToken);
    }
}