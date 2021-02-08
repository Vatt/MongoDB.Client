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
            //while (!_shutdownCts.IsCancellationRequested)
            //{
            //    while (await _channelReader.WaitToReadAsync().ConfigureAwait(false))
            //    {
            //        while (_channelReader.TryRead(out var request))
            //        {
            //            _completions.GetOrAdd(request.RequestNumber, request);
            //            await request.WriteAsync(request, _protocolWriter, _shutdownCts.Token).ConfigureAwait(false);
            //        }
            //    }
            //}
            while (!_shutdownCts.IsCancellationRequested)
            {
                var request = await _channelReader.ReadAsync(_shutdownCts.Token).ConfigureAwait(false);
                _completions.GetOrAdd(request.RequestNumber, request);
                await request.WriteAsync(_protocolWriter, _shutdownCts.Token).ConfigureAwait(false);
            }
        }

        private async Task StartFindChannelListerAsync()
        {
            //while (!_shutdownCts.IsCancellationRequested)
            //{
            //    while (await _findReader.WaitToReadAsync().ConfigureAwait(false))
            //    {
            //        while (_findReader.TryRead(out var request))
            //        {
            //            _completions.GetOrAdd(request.RequestNumber, request);
            //            await request.WriteAsync(request, _protocolWriter, _shutdownCts.Token).ConfigureAwait(false);
            //        }
            //    }
            //}
            while (!_shutdownCts.IsCancellationRequested)
            {
                var request = await _findReader.ReadAsync(_shutdownCts.Token).ConfigureAwait(false);
                _completions.GetOrAdd(request.RequestNumber, request);
                await request.WriteAsync(_protocolWriter, _shutdownCts.Token).ConfigureAwait(false);
            }

        }
    }
}