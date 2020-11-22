using MongoDB.Client.Messages;
using MongoDB.Client.Network;
using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Protocol.Writers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Bson.Serialization;

namespace MongoDB.Client
{
    internal class Channel : IAsyncDisposable
    {
        private readonly ILogger _logger;
        private readonly NetworkConnectionFactory _connectionFactory;
        private System.Net.Connections.Connection? _connection;
        private ProtocolReader? _reader;
        private ProtocolWriter? _writer;
        private readonly MessageHeaderReader _messageHeaderReader = new();
        private readonly ReplyMessageReader _replyMessageReader = new();
        private readonly MsgMessageReader _msgMessageReader = new();

        private readonly QueryMessageWriter _queryWriter = new();
        private readonly FindMessageWriter _findWriter = new();
        private readonly DeleteMessageWriter _deleteWriter = new();

        private readonly ConcurrentDictionary<int, ParserCompletion> _completionMap = new();
        private readonly ConcurrentQueue<ManualResetValueTaskSource<IParserResult>> _queue = new();

        private readonly CancellationTokenSource _shutdownToken = new();
        private Task? _readingTask;
        public int RequestsInProgress => _completionMap.Count;

        private readonly int _channelNum;

        public Channel(ILoggerFactory loggerFactory, int channelNum)
        {
            _channelNum = channelNum;
            _logger = loggerFactory.CreateLogger($"Channel: {channelNum}");
            _connectionFactory = new NetworkConnectionFactory();
        }

        private static int _counter;

        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _counter);
        }

        public async Task ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken)
        {
            _connection = await _connectionFactory.ConnectAsync(endPoint, null, cancellationToken)
                .ConfigureAwait(false);
            if (_connection is null)
            {
                ThrowHelper.ConnectionException<bool>(endPoint);
            }

            _reader = new ProtocolReader(_connection.Pipe.Input);
            _writer = new ProtocolWriter(_connection.Pipe.Output);
            _readingTask = StartReadAsync();
        }

        private async Task StartReadAsync()
        {
            _logger.LogInformation($"Channel {_channelNum} start reading");
            MongoResponseMessage message;
            ParserCompletion completion;
            var reader = _reader!;
            while (_shutdownToken.IsCancellationRequested == false)
            {
                try
                {
                    var headerResult = await reader.ReadAsync(_messageHeaderReader, _shutdownToken.Token)
                        .ConfigureAwait(false);
                    reader.Advance();

                    _logger.GotMessage(headerResult.Message.ResponseTo);
                    switch (headerResult.Message.Opcode)
                    {
                        case Opcode.Reply:
                            _logger.GotReplyMessage(headerResult.Message.ResponseTo);
                            var replyResult = await reader.ReadAsync(_replyMessageReader, _shutdownToken.Token)
                                .ConfigureAwait(false);
                            reader.Advance();
                            message = new ReplyMessage(headerResult.Message, replyResult.Message);
                            break;
                        case Opcode.OpMsg:
                            _logger.GotMsgMessage(headerResult.Message.ResponseTo);
                            var msgResult = await reader.ReadAsync(_msgMessageReader, _shutdownToken.Token)
                                .ConfigureAwait(false);
                            reader.Advance();
                            message = new ResponseMsgMessage(headerResult.Message, msgResult.Message);
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
                            _logger.UnknownOpcodeMessage(headerResult.Message);
                            if (_completionMap.TryGetValue(headerResult.Message.ResponseTo, out completion))
                            {
                                completion.CompletionSource.SetException(
                                    new NotSupportedException($"Opcode '{headerResult.Message.Opcode}' not supported"));
                            }

                            continue;
                            //TODO: need to read pipe to end
                            break;
                    }

                    if (_completionMap.TryGetValue(message.Header.ResponseTo, out completion))
                    {
                        var result = await completion.ParseAsync(reader, message).ConfigureAwait(false);
                        completion.CompletionSource.TrySetResult(result);
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

        public async ValueTask<QueryResult<TResp>> SendQueryAsync<TResp>(QueryMessage message,
            CancellationToken cancellationToken)
        {
            ManualResetValueTaskSource<IParserResult> taskSource;
            if (_queue.TryDequeue(out taskSource) == false)
            {
                taskSource = new ManualResetValueTaskSource<IParserResult>();
            }

            var completion = _completionMap.GetOrAdd(message.RequestNumber,
                i => new ParserCompletion(taskSource, (reader, response) => ParseAsync<TResp>(reader, response)));

            try
            {
                await _writer.WriteAsync(_queryWriter, message, cancellationToken).ConfigureAwait(false);
                var response =
                    await new ValueTask<IParserResult>(completion.CompletionSource, completion.CompletionSource.Version)
                        .ConfigureAwait(false);

                if (response is QueryResult<TResp> queryResult)
                {
                    return queryResult;
                }

                return default!;
            }
            finally
            {
                _completionMap.TryRemove(message.RequestNumber, out _);
                taskSource.Reset();
                _queue.Enqueue(taskSource);
            }


            static async ValueTask<IParserResult> ParseAsync<T>(ProtocolReader reader, MongoResponseMessage mongoResponse)
            {
                switch (mongoResponse)
                {
                    case ReplyMessage replyMessage:
                        if (SerializersMap.TryGetSerializer<T>(out var replySerializer))
                        {
                            var bodyReader = new ReplyBodyReader<T>(replySerializer, replyMessage);
                            var bodyResult = await reader.ReadAsync(bodyReader, default)
                                .ConfigureAwait(false);
                            reader.Advance();
                            return bodyResult.Message;
                        }

                        return ThrowHelper.UnsupportedTypeException<QueryResult<T>>(typeof(T));
                    default:
                        return ThrowHelper.UnsupportedTypeException<QueryResult<T>>(typeof(T));
                }
            }
        }


        private static class CursorParserCallbackHolder<T>
        {
            public static readonly Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>> Parser;
            public static readonly Func<int, ParserCompletion> Completion;
            public static readonly IGenericBsonSerializer<T> Serializer;
            private static readonly ConcurrentQueue<ManualResetValueTaskSource<IParserResult>> _queue = new();

            static CursorParserCallbackHolder()
            {
                SerializersMap.TryGetSerializer<T>(out Serializer);
                Parser = (reader, response) => CursorParseAsync<T>(reader, Serializer, response);
                Completion = i =>
                {
                    ManualResetValueTaskSource<IParserResult> taskSource;
                    if (_queue.TryDequeue(out taskSource) == false)
                    {
                        taskSource = new ManualResetValueTaskSource<IParserResult>();
                    }

                    return new ParserCompletion(taskSource, Parser);
                };
            }

            private static async ValueTask<IParserResult> CursorParseAsync<T>(ProtocolReader reader, IGenericBsonSerializer<T> serializer, MongoResponseMessage mongoResponse)
            {
                if (mongoResponse is ResponseMsgMessage msgMessage)
                {
                    MsgBodyReader<T> bodyReader;
                    if (msgMessage.MsgHeader.PayloadType == 0)
                    {
                        bodyReader = new FindMsgType0BodyReader<T>(serializer, msgMessage);
                    }
                    else
                    {
                        return ThrowHelper.InvalidPayloadTypeException<CursorResult<T>>(msgMessage.MsgHeader
                            .PayloadType);
                    }

                    var result = await reader.ReadAsync(bodyReader).ConfigureAwait(false);
                    reader.Advance();
                    return result.Message;
                }
                return ThrowHelper.UnsupportedTypeException<CursorResult<T>>(typeof(T));
            }
        }


        public async ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message,
            CancellationToken cancellationToken)
        {
            var completion = _completionMap.GetOrAdd(message.Header.RequestNumber, CursorParserCallbackHolder<T>.Completion);

            try
            {
                await _writer!.WriteAsync(_findWriter, message, cancellationToken).ConfigureAwait(false);
                _logger.SentCursorMessage(message.Header.RequestNumber);
                var response = await completion.CompletionSource.GetValueTask().ConfigureAwait(false);
                if (response is CursorResult<T> cursor)
                {
                    return cursor;
                }

                return default!;
            }
            finally
            {
                _completionMap.TryRemove(message.Header.RequestNumber, out _);
                completion.CompletionSource.Reset();
                _queue.Enqueue(completion.CompletionSource);
            }
        }

        private static class InsertParserCallbackHolder<T>
        {
            public static Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>>? Parser;
            public static Func<int, ParserCompletion>? Completion;
        }

        public async ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken cancellationToken)
        {
            if (InsertParserCallbackHolder<T>.Parser is null)
            {
                InsertParserCallbackHolder<T>.Parser = (reader, response) => InsertParseAsync<T>(reader, response);
                InsertParserCallbackHolder<T>.Completion = i =>
                {
                    ManualResetValueTaskSource<IParserResult> taskSource;
                    if (_queue.TryDequeue(out taskSource) == false)
                    {
                        taskSource = new ManualResetValueTaskSource<IParserResult>();
                    }

                    return new ParserCompletion(taskSource, InsertParserCallbackHolder<T>.Parser);
                };
            }

            var completion = _completionMap.GetOrAdd(message.Header.RequestNumber, InsertParserCallbackHolder<T>.Completion!);
            try
            {
                if (SerializersMap.TryGetSerializer<T>(out var serializer))
                {
                    var insertWriter = new InsertMessageWriter<T>(serializer);
                    await _writer!.WriteAsync(insertWriter, message, cancellationToken).ConfigureAwait(false);
                    _logger.SentInsertMessage(message.Header.RequestNumber);
                    var response =
                        await completion.CompletionSource.GetValueTask().ConfigureAwait(false);

                    if (response is InsertResult result)
                    {
                        if (result.WriteErrors is null)
                        {
                            return;
                        }

                        throw new MongoInsertException(result.WriteErrors);
                    }
                    else if (response is BsonParseResult bson)
                    {
                        Debugger.Break();
                    }
                }
            }
            finally
            {
                _completionMap.TryRemove(message.Header.RequestNumber, out _);
                completion.CompletionSource.Reset();
                _queue.Enqueue(completion.CompletionSource);
            }
        }

        private static readonly InsertMsgType0BodyReader InsertBodyReader = new InsertMsgType0BodyReader();
        private static readonly BsonBodyReader BsonBodyReader = new BsonBodyReader();
        private static async ValueTask<IParserResult> InsertParseAsync<TResp>(ProtocolReader reader, MongoResponseMessage mongoResponse)
        {
            switch (mongoResponse)
            {
                case ResponseMsgMessage msgMessage:

                    if (msgMessage.MsgHeader.PayloadType != 0)
                    {
                        return ThrowHelper.InvalidPayloadTypeException<InsertResult>(msgMessage.MsgHeader.PayloadType);
                    }

                    var result = await reader.ReadAsync(BsonBodyReader, default).ConfigureAwait(false);
                    reader.Advance();

                    return result.Message;

                default:
                    return ThrowHelper.UnsupportedTypeException<InsertResult>(typeof(TResp));
            }
        }

        public async ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken cancellationToken)
        {
            ManualResetValueTaskSource<IParserResult> taskSource;
            if (_queue.TryDequeue(out taskSource) == false)
            {
                taskSource = new ManualResetValueTaskSource<IParserResult>();
            }

            var completion = _completionMap.GetOrAdd(message.Header.RequestNumber,
                i => new ParserCompletion(taskSource, (reader, response) => ParseAsync(reader, response)));
            try
            {
                await _writer.WriteAsync(_deleteWriter, message, cancellationToken).ConfigureAwait(false);
                _logger.SentInsertMessage(message.Header.RequestNumber);
                var response =
                    await new ValueTask<IParserResult>(completion.CompletionSource,
                        completion.CompletionSource.Version).ConfigureAwait(false);

                return (DeleteResult)response;
            }
            finally
            {
                _completionMap.TryRemove(message.Header.RequestNumber, out _);
                taskSource.Reset();
                _queue.Enqueue(taskSource);
            }

            static async ValueTask<IParserResult> ParseAsync(ProtocolReader reader, MongoResponseMessage mongoResponse)
            {
                switch (mongoResponse)
                {
                    case ResponseMsgMessage msgMessage:

                        DeleteMsgType0BodyReader bodyReader;
                        if (msgMessage.MsgHeader.PayloadType == 0)
                        {
                            bodyReader = new DeleteMsgType0BodyReader();
                        }
                        else
                        {
                            return ThrowHelper.InvalidPayloadTypeException<DeleteResult>(msgMessage.MsgHeader
                                .PayloadType);
                        }

                        var result = await reader.ReadAsync(bodyReader).ConfigureAwait(false);
                        reader.Advance();

                        return result.Message;
                    default:
                        return ThrowHelper.UnsupportedTypeException<DeleteResult>(typeof(DeleteResult));
                }
            }
        }


        public async ValueTask DisposeAsync()
        {
            _shutdownToken.Cancel();
            if (_readingTask is not null)
            {
                await _readingTask.ConfigureAwait(false);
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync().ConfigureAwait(false);
                await _connection.DisposeAsync().ConfigureAwait(false);
            }
        }

        private readonly struct ParserCompletion
        {
            public ParserCompletion(ManualResetValueTaskSource<IParserResult> completionSource,
                Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>> parseAsync)
            {
                CompletionSource = completionSource;
                ParseAsync = parseAsync;
            }

            public ManualResetValueTaskSource<IParserResult> CompletionSource { get; }
            public Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>> ParseAsync { get; }
        }
    }
}
