using MongoDB.Client.Protocol;
using System;
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
            while (true)
            {
                var request = await _channelReader.ReadAsync();
                switch (request)
                {
                    case FindMongoRequest findRequest:
                        {
                            _completions.GetOrAdd(findRequest.Message.Header.RequestNumber, findRequest);
                            await _protocolWriter!.WriteAsync(ProtocolWriters.FindMessageWriter, findRequest.Message, _shutdownCts.Token).ConfigureAwait(false);
                            _logger.SentCursorMessage(findRequest.Message.Header.RequestNumber);
                            break;
                        }
                    case QueryMongoRequest queryRequest:
                        {
                            _completions.GetOrAdd(queryRequest.Message.RequestNumber, queryRequest);
                            await _protocolWriter!.WriteAsync(ProtocolWriters.QueryMessageWriter, queryRequest.Message, _shutdownCts.Token).ConfigureAwait(false);
                            _logger.SentCursorMessage(queryRequest.Message.RequestNumber);
                            break;
                        }
                    default:
                        throw new Exception(nameof(StartChannelListerAsync));
                }
            }
        }

    }
}