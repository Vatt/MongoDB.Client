using Microsoft.Extensions.Logging;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection
    {
        private async Task StartProtocolListenerAsync()
        {
            if (_protocolReader is null)
            {
                ThrowHelper.ThrowNotInitialized();
            }
            MongoResponseMessage message = default!;
            MongoRequest? request;
            while (!_shutdownCts.IsCancellationRequested)
            {
                try
                {
                    var header = await ReadAsyncPrivate(ProtocolReaders.MessageHeaderReader, _shutdownToken).ConfigureAwait(false);
                    switch (header.Opcode)
                    {
                        case Opcode.Reply:
                            _logger.GotReplyMessage(header.ResponseTo);
                            var replyResult = await ReadAsyncPrivate(ProtocolReaders.ReplyMessageReader, _shutdownToken).ConfigureAwait(false);
                            message = new ReplyMessage(header, replyResult);
                            break;
                        case Opcode.OpMsg:
                            _logger.GotMsgMessage(header.ResponseTo);
                            var msgResult = await ReadAsyncPrivate(ProtocolReaders.MsgMessageReader, _shutdownToken).ConfigureAwait(false);
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
                            HandleListenerFault(new MongoException("Received broken data"));
                            break;
                    }

                    if (_shutdownCts.IsCancellationRequested)
                    {
                        break;
                    }

                    if (_completions.TryRemove(message.Header.ResponseTo, out request))
                    {
                        var generation = request.CurrentGeneration;
                        try
                        {
                            var result = await request.ParseAsync!(_protocolReader, message).ConfigureAwait(false);
                            request.TrySetResult(generation, result);
                        }
                        catch (Exception e)
                        {
                            // read rest of the responce
                            request.TrySetException(generation, e);
                        }
                    }
                    else
                    {
                        _logger.LogError("Message not found");
                    }
                }
                catch (OperationCanceledException) when (_shutdownCts.IsCancellationRequested && _suppressConnectionLost)
                {
                    break;
                }
                catch (ObjectDisposedException) when (_shutdownCts.IsCancellationRequested && _suppressConnectionLost)
                {
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                    HandleListenerFault(e);
                }
            }

            if (_shutdownCts.IsCancellationRequested)
            {
                FailPendingCompletions(GetTerminalException());
            }
        }

        private async ValueTask<T> ReadAsyncPrivate<T>(IMessageReader<T> reader, CancellationToken token)
        {
            var result = await _protocolReader!.ReadAsync(reader, token).ConfigureAwait(false);
            _protocolReader!.Advance();
            if (result.IsCanceled)
            {
                throw new OperationCanceledException(token);
            }

            if (result.IsCompleted)
            {
                throw new MongoException("Connection terminated.");
            }
            return result.Message;
        }
    }
}
