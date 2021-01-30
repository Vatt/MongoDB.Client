using Microsoft.Extensions.Logging;
using MongoDB.Client.Protocol.Readers;
using System;

namespace MongoDB.Client.Connection
{
    internal static class LoggerHelper
    {
        private static readonly Action<ILogger, int, Exception> _gotReplyMessage =
            LoggerMessage.Define<int>(LogLevel.Trace, new EventId(1, nameof(MongoConnection)),
                "Got Reply message '{requestNumber}'");

        public static void GotReplyMessage(this ILogger logger, int requestNum)
        {
            _gotReplyMessage(logger, requestNum, default!);
        }

        private static readonly Action<ILogger, int, Exception> _gotMessage =
            LoggerMessage.Define<int>(LogLevel.Debug, new EventId(1, nameof(MongoConnection)),
                "Got message '{requestNumber}'");

        public static void GotMessage(this ILogger logger, int requestNum)
        {
            _gotMessage(logger, requestNum, default!);
        }

        private static readonly Action<ILogger, int, Exception> _gotMessageComplete =
            LoggerMessage.Define<int>(LogLevel.Debug, new EventId(1, nameof(MongoConnection)),
                "Got message '{requestNumber}' complete");

        public static void GotMessageComplete(this ILogger logger, int requestNum)
        {
            _gotMessageComplete(logger, requestNum, default!);
        }

        private static readonly Action<ILogger, int, Exception> _gotMsgMessage = LoggerMessage.Define<int>(LogLevel.Trace,
            new EventId(1, nameof(MongoConnection)), "Got Msg message '{requestNumber}'");

        public static void GotMsgMessage(this ILogger logger, int requestNum)
        {
            _gotMsgMessage(logger, requestNum, default!);
        }


        private static readonly Action<ILogger, MessageHeader, Exception> _unknownOpcodeMessage =
            LoggerMessage.Define<MessageHeader>(LogLevel.Error, new EventId(1, nameof(MongoConnection)),
                "Unknown opcode: {message}");
        public static void UnknownOpcodeMessage(this ILogger logger, MessageHeader opcode)
        {
            _unknownOpcodeMessage(logger, opcode, default!);
        }

        private static readonly Action<ILogger, int, Exception> _sentCursorMessage =
            LoggerMessage.Define<int>(LogLevel.Trace, new EventId(1, nameof(MongoConnection)),
                "Sent cursor message '{requestNumber}'");

        public static void SentCursorMessage(this ILogger logger, int requestNum)
        {
            _sentCursorMessage(logger, requestNum, default!);
        }

        private static readonly Action<ILogger, int, Exception> _sentInsertMessage =
            LoggerMessage.Define<int>(LogLevel.Trace, new EventId(1, nameof(MongoConnection)),
                "Sent insert message '{requestNumber}'");

        public static void SentInsertMessage(this ILogger logger, int requestNum)
        {
            _sentInsertMessage(logger, requestNum, default!);
        }

        private static readonly Action<ILogger, int, Exception> _parsingMsgMessage =
            LoggerMessage.Define<int>(LogLevel.Trace, new EventId(1, nameof(MongoConnection)),
                "Parsing msg message '{requestNumber}'");

        public static void ParsingMsgMessage(this ILogger logger, int requestNum)
        {
            _parsingMsgMessage(logger, requestNum, default!);
        }


        private static readonly Action<ILogger, int, Exception> _parsingMsgCompleteMessage =
            LoggerMessage.Define<int>(LogLevel.Trace, new EventId(1, nameof(MongoConnection)),
                "Parsing msg message '{requestNumber}' complete");

        public static void ParsingMsgCompleteMessage(this ILogger logger, int requestNum)
        {
            _parsingMsgCompleteMessage(logger, requestNum, default!);
        }
    }
}