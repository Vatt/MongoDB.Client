using MongoDB.Client.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection
    {
        private int _requestId = 0;
        private int _requestsInWork = 0;
        private int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _requestId);
        }
        private async Task StartChannelListerAsync()
        {
            while (!_shutdownCts.IsCancellationRequested)
            {
                //if (_requestsInWork == Threshold)
                //{
                //    //Console.WriteLine($"Connection {ConnectionId}: Threshold lock");
                //    await _channelListenerLock.WaitAsync().ConfigureAwait(false);
                //}
                //if (_requestsInWork >= Threshold)
                //{
                //    Console.WriteLine($"Connection {ConnectionId}: {_requestsInWork}");
                //}
                var request = await _channelReader.ReadAsync().ConfigureAwait(false);
                //Interlocked.Increment(ref _requestsInWork);
                switch (request.Type)
                {
                    case RequestType.FindRequest:
                        {
                            var findRequest = (FindMongoRequest)request;
                            _completions.GetOrAdd(findRequest.RequestNumber, findRequest);
                            await _protocolWriter!.WriteAsync(ProtocolWriters.FindMessageWriter, findRequest.Message, _shutdownCts.Token).ConfigureAwait(false);
                            _logger.SentCursorMessage(findRequest.Message.Header.RequestNumber);
                            break;
                        }
                    case RequestType.QueryRequest:
                        {
                            var queryRequest = (QueryMongoRequest)request;
                            _completions.GetOrAdd(queryRequest.RequestNumber, queryRequest);
                            await _protocolWriter!.WriteAsync(ProtocolWriters.QueryMessageWriter, queryRequest.Message, _shutdownCts.Token).ConfigureAwait(false);
                            _logger.SentCursorMessage(queryRequest.Message.RequestNumber);
                            break;
                        }
                    case RequestType.InsertRequest:
                        {
                            var insertRequest = (InsertMongoRequest)request;
                            _completions.GetOrAdd(insertRequest.RequestNumber, insertRequest);
                            await insertRequest.WriteAsync(insertRequest.Message, _protocolWriter, _shutdownCts.Token).ConfigureAwait(false);
                            _logger.SentInsertMessage(insertRequest.RequestNumber);
                            break;
                        }
                    case RequestType.DeleteRequest:
                        {
                            var deleteRequest = (DeleteMongoRequest)request;
                            _completions.GetOrAdd(deleteRequest.RequestNumber, deleteRequest);
                            await _protocolWriter!.WriteAsync(ProtocolWriters.DeleteMessageWriter, deleteRequest.Message, _shutdownCts.Token).ConfigureAwait(false);
                            _logger.SentCursorMessage(deleteRequest.Message.Header.RequestNumber);
                            break;
                        }
                    default:
                        //Interlocked.Decrement(ref _requestsInWork);
                        _logger.UnknownRequestType(request.Type);
                        request.CompletionSource.SetException(new NotSupportedException($"Request type '{request.Type}' not supported"));
                        break;
                }
            }
        }
        private async Task StartFindChannelListerAsync()
        {
            while (!_shutdownCts.IsCancellationRequested)
            {
                //if (_requestsInWork == Threshold)
                //{
                //    //    //Console.WriteLine($"Connection {ConnectionId}: Threshold lock");
                //    await _channelListenerLock.WaitAsync().ConfigureAwait(false);
                //}
                var request = await _findReader.ReadAsync().ConfigureAwait(false);
                //Interlocked.Increment(ref _requestsInWork);
                switch (request.Type)
                {
                    case RequestType.FindRequest:
                        {
                            _completions.GetOrAdd(request.RequestNumber, request);
                            await _protocolWriter!.WriteAsync(ProtocolWriters.FindMessageWriter, request.Message, _shutdownCts.Token).ConfigureAwait(false);
                            _logger.SentCursorMessage(request.Message.Header.RequestNumber);
                            break;
                        }
                    default:
                        _logger.UnknownRequestType(request.Type);
                        request.CompletionSource.SetException(new NotSupportedException($"Request type '{request.Type}' not supported"));
                        break;
                }
            }
        }

    }
}