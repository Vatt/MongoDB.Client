using System;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Protocol.Common;

namespace MongoDB.Client
{
    internal static class LoggerHelper
    {
        private static readonly Action<ILogger, int, Exception> _gotReplyMessage =
            LoggerMessage.Define<int>(LogLevel.Trace, new EventId(1, nameof(Channel)),
                "Got Reply message '{requestNumber}'");

        public static void GotReplyMessage(this ILogger logger, int requestNum)
        {
            _gotReplyMessage(logger, requestNum, default!);
        }


        private static readonly Action<ILogger, int, Exception> _gotMsgMessage = LoggerMessage.Define<int>(LogLevel.Trace,
            new EventId(1, nameof(Channel)), "Got Msg message '{requestNumber}'");

        public static void GotMsgMessage(this ILogger logger, int requestNum)
        {
            _gotMsgMessage(logger, requestNum, default!);
        }


        private static readonly Action<ILogger, Opcode, Exception> _unknownOpcodeMessage =
            LoggerMessage.Define<Opcode>(LogLevel.Error, new EventId(1, nameof(Channel)),
                "Unknown opcode '{opcode}'");

        public static void UnknownOpcodeMessage(this ILogger logger, Opcode opcode)
        {
            _unknownOpcodeMessage(logger, opcode, default!);
        }


        private static readonly Action<ILogger, int, Exception> _sentCursorMessage =
            LoggerMessage.Define<int>(LogLevel.Trace, new EventId(1, nameof(Channel)),
                "Sent cursor message '{requestNumber}'");

        public static void SentCursorMessage(this ILogger logger, int requestNum)
        {
            _sentCursorMessage(logger, requestNum, default!);
        }


        private static readonly Action<ILogger, int, Exception> _parsingMsgMessage =
            LoggerMessage.Define<int>(LogLevel.Trace, new EventId(1, nameof(Channel)),
                "Parsing msg message '{requestNumber}'");

        public static void ParsingMsgMessage(this ILogger logger, int requestNum)
        {
            _parsingMsgMessage(logger, requestNum, default!);
        }


        private static readonly Action<ILogger, int, Exception> _parsingMsgCompleteMessage =
            LoggerMessage.Define<int>(LogLevel.Trace, new EventId(1, nameof(Channel)),
                "Parsing msg message '{requestNumber}' complete");
        
        public static void ParsingMsgCompleteMessage(this ILogger logger, int requestNum)
        {
            _parsingMsgCompleteMessage(logger, requestNum, default!);
        }
    }
}