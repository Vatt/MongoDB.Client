using Microsoft.Extensions.Logging;
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

            try
            {
                while (!_shutdownCts.IsCancellationRequested)
                {
                    while (await _channelReader.WaitToReadAsync(_shutdownToken).ConfigureAwait(false))
                    {
                        while (_channelReader.TryRead(out var request))
                        {
                            _completions.GetOrAdd(request.RequestNumber, request);
                            await request.WriteAsync!(_protocolWriter, _shutdownToken).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (_shutdownCts.IsCancellationRequested)
            {
            }
            catch (ObjectDisposedException) when (_shutdownCts.IsCancellationRequested)
            {
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                HandleListenerFault(e);
            }

            if (_shutdownCts.IsCancellationRequested)
            {
                FailPendingCompletions(GetTerminalException());
            }
        }
    }
}
