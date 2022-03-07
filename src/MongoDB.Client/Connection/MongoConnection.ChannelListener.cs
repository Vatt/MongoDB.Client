using MongoDB.Client.Exceptions;

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
                while (await _channelReader.WaitToReadAsync().ConfigureAwait(false))
                {
                    while (_channelReader.TryRead(out var request))
                    {
                        _completions.GetOrAdd(request.RequestNumber, request);
                        await request.WriteAsync!(_protocolWriter, _shutdownCts.Token).ConfigureAwait(false);
                    }
                }
                //var request = await _channelReader.ReadAsync(_shutdownCts.Token).ConfigureAwait(false);
                //_completions.GetOrAdd(request.RequestNumber, request);
                //await request.WriteAsync!(_protocolWriter, _shutdownCts.Token).ConfigureAwait(false);
            }
        }
    }
}
