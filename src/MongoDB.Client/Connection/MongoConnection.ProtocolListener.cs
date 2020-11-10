using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
            MongoResponseMessage message;
            ParserCompletion? completion;
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

                    var complection = _completions[message.Header.RequestId % Threshold];
                    ValueTask <IParserResult> resultTask;
                    unsafe
                    {
                        resultTask = complection.AsyncParser(message);
                    }

                    var result = await resultTask; 
                    complection.ComplectionSource.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                finally
                {
                    
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

            return await ContinueReadAsyncPrivate(task);
        }
        private async ValueTask<T> ContinueReadAsyncPrivate<T>(ValueTask<T> task)
        {
            return await task;
        }
        private async  ValueTask<T> ProtocolReadAsyncPrivate<T>(IMessageReader<T> reader, CancellationToken token)
        {

            var result = await _protocolReader.ReadAsync(reader, token).ConfigureAwait(false);
            _protocolReader.Advance();
            if (result.IsCanceled || result.IsCompleted)
            {
                //TODO: DO SOME
            }
            return result.Message;
        }
        private unsafe class ParserCompletion
        {
            public ManualResetValueTaskSource<IParserResult> ComplectionSource;
            public delegate*<MongoResponseMessage, ValueTask<IParserResult>> AsyncParser;

            public void SetData(ManualResetValueTaskSource<IParserResult> completionSource, delegate*<MongoResponseMessage, ValueTask<IParserResult>> parser)
            {
                AsyncParser = parser;
                ComplectionSource = completionSource;
            }
        }
    }
}