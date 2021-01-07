using Microsoft.Extensions.Logging;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection
    {
        private async Task StartProtocolListenerAsync()
        {
            MongoResponseMessage message;
            MongoReuqestBase? request;
            while (!_shutdownCts.IsCancellationRequested)
            {
                try
                {
                    var header = await ReadAsyncPrivate(ProtocolReaders.MessageHeaderReader, _shutdownCts.Token).ConfigureAwait(false);
                    switch (header.Opcode)
                    {
                        case Opcode.Reply:
                            _logger.GotReplyMessage(header.ResponseTo);
                            var replyResult = await ReadAsyncPrivate(ProtocolReaders.ReplyMessageReader, _shutdownCts.Token).ConfigureAwait(false);
                            message = new ReplyMessage(header, replyResult);
                            break;
                        case Opcode.OpMsg:
                            _logger.GotMsgMessage(header.ResponseTo);
                            var msgResult = await ReadAsyncPrivate(ProtocolReaders.MsgMessageReader, _shutdownCts.Token).ConfigureAwait(false);
                            message = new ResponseMsgMessage(header, msgResult);
                            break;
                        case Opcode.Message:
                        case Opcode.Update:
                        case Opcode.Insert:
                        case Opcode.Query:
                        case Opcode.GetMore:
                        case Opcode.Delete:
                        case Opcode.KillCursors:
                        case Opcode.Compressed:
                        default:
                            _logger.UnknownOpcodeMessage(header);
                            if (_completions.TryRemove(header.ResponseTo, out request))
                            {
                                var oldRequestsInWork = _requestsInWork;
                                Interlocked.Decrement(ref _requestsInWork);
                                if (oldRequestsInWork == Threshold && _requestsInWork < Threshold)
                                {
                                    _channelListenerLock.Release();
                                    request.CompletionSource.SetException(new NotSupportedException($"Opcode '{header.Opcode}' not supported"));
                                }
                            }
                            continue;
                            //TODO: need to read pipe to end
                            break;
                    }

                    if (_completions.TryRemove(message.Header.ResponseTo, out request))
                    {
                        var oldRequestsInWork = _requestsInWork;
                        Interlocked.Decrement(ref _requestsInWork);
                        if (oldRequestsInWork == Threshold && _requestsInWork < Threshold)
                        {
                            //Console.WriteLine($"Connection {ConnectionId}: Threshold unlock");
                            _channelListenerLock.Release();
                        }
                        switch (request.Type)
                        {
                            case RequestType.FindRequest:
                                {
                                    var findRequest = (FindMongoRequest)request;
                                    var result = await findRequest.ParseAsync(_protocolReader, message).ConfigureAwait(false);
                                    request.CompletionSource.TrySetResult(result);
                                    break;
                                }
                            case RequestType.QueryRequest:
                                {
                                    var queryRequest = (QueryMongoRequest)request;
                                    var result = await queryRequest.ParseAsync(_protocolReader, message).ConfigureAwait(false);
                                    request.CompletionSource.TrySetResult(result);
                                    break;
                                }
                            case RequestType.InsertRequest:
                                {
                                    var insertRequest = (InsertMongoRequest)request;
                                    var result = await insertRequest.ParseAsync(_protocolReader, message).ConfigureAwait(false);
                                    request.CompletionSource.TrySetResult(result);
                                    break;
                                }
                            case RequestType.DeleteRequest:
                                {
                                    var deleteRequest = (DeleteMongoRequest)request;
                                    var result = await deleteRequest.ParseAsync(_protocolReader, message).ConfigureAwait(false);
                                    request.CompletionSource.TrySetResult(result);
                                    break;
                                }
                            default:
                                _logger.UnknownRequestType(request.Type);
                                request.CompletionSource.SetException(new NotSupportedException($"Request type '{request.Type}' not supported"));
                                break;
                        }
                    }
                    else
                    {
                        _logger.LogError("Message not found");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }
            }
        }
        private async ValueTask<T> ReadAsyncPrivate<T>(IMessageReader<T> reader, CancellationToken token)
        {
            var task = ProtocolReadAsyncPrivate(reader, token);
            if (task.IsCompleted)
            {
                return task.Result;
            }

            return await task.ConfigureAwait(false);
        }
        private async ValueTask<T> ProtocolReadAsyncPrivate<T>(IMessageReader<T> reader, CancellationToken token)
        {

            var result = await _protocolReader.ReadAsync(reader, token).ConfigureAwait(false);
            _protocolReader.Advance();
            if (result.IsCanceled || result.IsCompleted)
            {
                //TODO: DO SOME
            }
            return result.Message;
        }
    }
}