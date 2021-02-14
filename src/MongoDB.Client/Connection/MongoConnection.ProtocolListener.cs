using Microsoft.Extensions.Logging;
using MongoDB.Client.Exceptions;
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
            if (_protocolReader is null)
            {
                ThrowHelper.ThrowNotInitialized();
            }
            MongoResponseMessage message;
            MongoRequest? request;
            while (!_shutdownCts.IsCancellationRequested)
            {
                try
                {
                    var header = await ReadAsyncPrivate(_protocolReader, ProtocolReaders.MessageHeaderReader, _shutdownCts.Token).ConfigureAwait(false);
                    switch (header.Opcode)
                    {
                        case Opcode.Reply:
                            _logger.GotReplyMessage(header.ResponseTo);
                            var replyResult = await ReadAsyncPrivate(_protocolReader, ProtocolReaders.ReplyMessageReader, _shutdownCts.Token).ConfigureAwait(false);
                            message = new ReplyMessage(header, replyResult);
                            break;
                        case Opcode.OpMsg:
                            _logger.GotMsgMessage(header.ResponseTo);
                            var msgResult = await ReadAsyncPrivate(_protocolReader, ProtocolReaders.MsgMessageReader, _shutdownCts.Token).ConfigureAwait(false);
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
                                request.CompletionSource.SetException(new NotSupportedException($"Opcode '{header.Opcode}' not supported"));
                            }
                            continue;
                            //TODO: need to read pipe to end
                    }

                    if (_completions.TryRemove(message.Header.ResponseTo, out request))
                    {
                        var result = await request.ParseAsync!(_protocolReader, message).ConfigureAwait(false);
                        request.CompletionSource.TrySetResult(result);
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

        private static async ValueTask<T> ReadAsyncPrivate<T>(ProtocolReader protocolReader, IMessageReader<T> reader, CancellationToken token)
        {
            var result = await protocolReader.ReadAsync(reader, token).ConfigureAwait(false);
            protocolReader.Advance();
            if (result.IsCanceled || result.IsCompleted)
            {
                //TODO: DO SOME
            }
            return result.Message;
        }
    }
}