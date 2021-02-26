using MongoDB.Client.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection
    {
        private int _requestId = 0;
        private int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _requestId);
        }

        private async Task StartChannelListerAsync()
        {
            if (_protocolWriter is null)
            {
                ThrowHelper.ThrowNotInitialized();
            }

            while (!_shutdownCts.IsCancellationRequested)
            {
                var request = await _channelReader.ReadAsync(_shutdownCts.Token).ConfigureAwait(false);
                _completions.GetOrAdd(request.RequestNumber, request);
                await request.WriteAsync!(_protocolWriter, _shutdownCts.Token).ConfigureAwait(false);
            }
        }

        private async Task StartFindChannelListerAsync()
        {
            if (_protocolWriter is null)
            {
                ThrowHelper.ThrowNotInitialized();
            }

            while (!_shutdownCts.IsCancellationRequested)
            {
                var request = await _findReader.ReadAsync(_shutdownCts.Token).ConfigureAwait(false);
                _completions.GetOrAdd(request.RequestNumber, request);
                await request.WriteAsync!(_protocolWriter, _shutdownCts.Token).ConfigureAwait(false);
            }

        }
    }
}