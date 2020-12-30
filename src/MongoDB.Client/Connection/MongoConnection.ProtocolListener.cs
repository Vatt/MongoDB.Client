﻿using Microsoft.Extensions.Logging;
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
                            continue;
                            //TODO: need to read pipe to end
                    }

                    if (_completions.TryGetValue(message.Header.ResponseTo, out request))
                    {
                        switch (request.Type)
                        {
                            case RequestType.FindRequest:
                            {
                                var findRequest = (FindMongoRequest) request;
                                var result = await findRequest.ParseAsync(_protocolReader, message).ConfigureAwait(false);
                                request.CompletionSource.TrySetResult(result);
                                break;
                            }
                            case RequestType.QueryRequest:
                            {
                                var queryRequest = (QueryMongoRequest) request;
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
                                throw new Exception(nameof(StartChannelListerAsync)); //TODO: FIXIT
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

            return await task;
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