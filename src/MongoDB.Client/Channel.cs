using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Generated;
using MongoDB.Client.Messages;
using MongoDB.Client.Network;
using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Protocol.Writers;
using MongoDB.Client.Readers;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Connection;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client
{
    internal class Channel : IAsyncDisposable
    {
        private readonly EndPoint _endpoint;
        private readonly NetworkConnectionFactory _connectionFactory;
        private System.Net.Connections.Connection? _connection;
        private readonly BsonDocument _initialDocument;
        private ConnectionInfo? _connectionInfo;
        private ProtocolReader? _reader;
        private ProtocolWriter? _writer;
        private static readonly MessageHeaderReader messageHeaderReader = new MessageHeaderReader();
        private static readonly ReplyMessageReader replyMessageReader = new ReplyMessageReader();
        private static readonly MsgMessageReader msgMessageReader = new MsgMessageReader();

        private static readonly ReadOnlyMemoryWriter memoryWriter = new ReadOnlyMemoryWriter();
        private static readonly QueryMessageWriter queryWriter = new QueryMessageWriter();
        private static readonly MsgMessageWriter msgWriter = new MsgMessageWriter();

        private readonly ConcurrentDictionary<int, TaskCompletionSourceWithCancellation<MongoResponseMessage>>
            _completionMap =
                new ConcurrentDictionary<int, TaskCompletionSourceWithCancellation<MongoResponseMessage>>();

        // private readonly ManualResetValueTaskSource<MongoResponseMessage> completionSource =
        //     new ManualResetValueTaskSource<MongoResponseMessage>();

        private CancellationTokenSource _shutdownToken = new CancellationTokenSource();
        private Task? _readingTask;
        private readonly SemaphoreSlim _initSemaphore = new SemaphoreSlim(1);
        private Task<ConnectionInfo>? _initTask;
        public bool Init { get; private set; }

        public Channel(EndPoint endpoint)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _connectionFactory = new NetworkConnectionFactory();
            _initialDocument = InitHelper.CreateInitialCommand();
        }

        private static readonly byte[] Hell = new byte[]
        {
            59, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 212, 7, 0, 0, 4, 0, 0, 0, 97, 100, 109, 105, 110, 46, 36, 99, 109, 100,
            0, 0, 0, 0, 0, 255, 255, 255, 255, 20, 0, 0, 0, 16, 98, 117, 105, 108, 100, 73, 110, 102, 111, 0, 1, 0, 0,
            0, 0
        };

        private static int _counter;

        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _counter);
        }

        public ValueTask<ConnectionInfo> InitConnectAsync(CancellationToken cancellationToken)
        {
            if (_connectionInfo is not null)
            {
                return new ValueTask<ConnectionInfo>(_connectionInfo);
            }

            return StartConnectAsync(cancellationToken);

            async ValueTask<ConnectionInfo> StartConnectAsync(CancellationToken ct)
            {
                if (_initTask is null)
                {
                    await _initSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    try
                    {
                        if (_initTask is null)
                        {
                            _initTask = DoConnectAsync(ct);
                        }
                    }
                    finally
                    {
                        _initSemaphore.Release();
                    }
                }

                return await _initTask.ConfigureAwait(false);
            }

            async Task<ConnectionInfo> DoConnectAsync(CancellationToken ct)
            {
                await ConnectAsync(ct).ConfigureAwait(false);
                QueryMessage? request = CreateConnectRequest();
                var configMessage = await SendQueryAsync<BsonDocument>(request, ct).ConfigureAwait(false);
                // var hell = await SendAsync<BsonDocument>(Hell, ct).ConfigureAwait(false);
                _connectionInfo = new ConnectionInfo(configMessage, null);
                Init = true;
                return _connectionInfo;
            }
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            _connection = await _connectionFactory.ConnectAsync(_endpoint, null, cancellationToken)
                .ConfigureAwait(false);
            if (_connection is null)
            {
                ThrowHelper.ConnectionException<bool>(_endpoint);
            }

            _reader = new ProtocolReader(_connection.Pipe.Input);
            _writer = new ProtocolWriter(_connection.Pipe.Output);
            _readingTask = StartReadAsync();
        }

        private QueryMessage CreateConnectRequest()
        {
            var doc = CreateWrapperDocument();
            return CreateRequest("admin.$cmd", Opcode.Query, doc);
        }

        private QueryMessage CreateRequest(string database, Opcode opcode, BsonDocument document)
        {
            var num = GetNextRequestNumber();
            return new QueryMessage(num, database, opcode, document);
        }

        private BsonDocument CreateWrapperDocument()
        {
            BsonDocument? readPreferenceDocument = null;
            var doc = new BsonDocument
            {
                {"$query", _initialDocument},
                {"$readPreference", readPreferenceDocument, readPreferenceDocument != null}
            };

            if (doc.Count == 1)
            {
                return doc["$query"].AsBsonDocument;
            }
            else
            {
                return doc;
            }
        }


        private async Task StartReadAsync()
        {
            if (_reader is null)
            {
                ThrowHelper.ConnectionException<bool>(_endpoint);
            }

            MongoResponseMessage message;
            TaskCompletionSourceWithCancellation<MongoResponseMessage>? completion;
            while (_shutdownToken.IsCancellationRequested == false)
            {
                var headerResult = await _reader.ReadAsync(messageHeaderReader, _shutdownToken.Token)
                    .ConfigureAwait(false);
                _reader.Advance();
                switch (headerResult.Message.Opcode)
                {
                    case Opcode.Reply:
                        await Console.Out.WriteLineAsync($"Got Reply message '{headerResult.Message.ResponseTo}'");
                        // Debug.WriteLine($"Got Reply message '{headerResult.Message.ResponseTo}'");
                        var replyResult = await _reader.ReadAsync(replyMessageReader, _shutdownToken.Token)
                            .ConfigureAwait(false);
                        _reader.Advance();
                        message = new ReplyMessage(headerResult.Message, replyResult.Message);
                        // Debug.WriteLine($"Parsed Reply message '{headerResult.Message.ResponseTo}'");
                        if (_completionMap.TryGetValue(message.Header.ResponseTo, out completion))
                        {
                            // Debug.WriteLine($"Set result to message '{message.Header.ResponseTo}'");
                            completion.TrySetResult(message);
                        }

                        // Debug.WriteLine($"Reply message '{message.Header.ResponseTo}' completed");
                        // TODO: 
                        break;
                    case Opcode.OpMsg:
                        await Console.Out.WriteLineAsync($"Got Msg message '{headerResult.Message.ResponseTo}'");
                        // Debug.WriteLine($"Got Msg message '{headerResult.Message.ResponseTo}'");
                        var msgResult = await _reader.ReadAsync(msgMessageReader, _shutdownToken.Token)
                            .ConfigureAwait(false);
                        _reader.Advance();
                        // Debug.WriteLine($"Parsed Msg message '{headerResult.Message.ResponseTo}'");
                        message = new ResponseMsgMessage(headerResult.Message, msgResult.Message);

                        if (_completionMap.TryGetValue(message.Header.ResponseTo, out completion))
                        {
                            // Debug.WriteLine($"Set result to message '{message.Header.ResponseTo}'");
                            completion.TrySetResult(message);
                        }

                        // Debug.WriteLine($"Msg message '{message.Header.ResponseTo}' completed");
                        // TODO: 
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
                        await Console.Out.WriteLineAsync($"Unknown opcode '{headerResult.Message.Opcode}'");
                        if (_completionMap.TryGetValue(headerResult.Message.ResponseTo, out completion))
                        {
                            // Debug.WriteLine($"Set result to message '{message.Header.ResponseTo}'");
                            completion.TrySetException(
                                new NotSupportedException($"Opcode '{headerResult.Message.Opcode}' not supported"));
                        }

                        //TODO: need to read pipe to end
                        break;
                }
            }
        }

        public async ValueTask<TResp> SendQueryAsync<TResp>(QueryMessage message, CancellationToken cancellationToken)
        {
            if (_shutdownToken.IsCancellationRequested == false)
            {
                if (_writer is not null)
                {
                    var completion = _completionMap.GetOrAdd(message.RequestNumber,
                        i => new TaskCompletionSourceWithCancellation<MongoResponseMessage>());
                    try
                    {
                        await _writer.WriteAsync(queryWriter, message, cancellationToken).ConfigureAwait(false);
                        // Debug.WriteLine($"Sent query message '{message.RequestNumber}'");
                        // var response = await new ValueTask<MongoResponseMessage>(completionSource, completionSource.Version)
                        //     .ConfigureAwait(false);
                        // completionSource.Reset();
                        var response = await completion.WaitWithCancellationAsync(cancellationToken)
                            .ConfigureAwait(false);

                        return await ParseAsync<TResp>(response).ConfigureAwait(false);
                    }
                    finally
                    {
                        _completionMap.TryRemove(message.RequestNumber, out _);
                    }
                }

                return ThrowHelper.ConnectionException<TResp>(_endpoint);
            }

            return ThrowHelper.ObjectDisposedException<TResp>(nameof(Channel));


            async ValueTask<T> ParseAsync<T>(MongoResponseMessage message)
            {
                var reader = _reader!;
                switch (message)
                {
                    case ReplyMessage replyMessage:
                        // Debug.WriteLine($"Parsing reply message '{message.Header.ResponseTo}'");
                        if (SerializersMap.TryGetSerializer<T>(out var replySerializer))
                        {
                            var bodyReader = new ReplyBodyReader<T>(replySerializer);
                            var bodyResult = await reader.ReadAsync(bodyReader, _shutdownToken.Token)
                                .ConfigureAwait(false);
                            reader.Advance();
                            // Debug.WriteLine($"Parsing reply message '{message.Header.ResponseTo}' complete");
                            return bodyResult.Message;
                        }

                        return ThrowHelper.UnsupportedTypeException<T>(typeof(T));
                    default:
                        return ThrowHelper.UnsupportedTypeException<T>(typeof(T));
                }
            }
        }

        public async ValueTask<CursorResult<TResp>> GetCursorAsync<TResp>(MsgMessage message,
            CancellationToken cancellationToken)
        {
            if (_shutdownToken.IsCancellationRequested == false)
            {
                if (_writer is not null)
                {
                    var completion = _completionMap.GetOrAdd(message.RequestNumber,
                        i => new TaskCompletionSourceWithCancellation<MongoResponseMessage>());
                    try
                    {
                        await _writer.WriteAsync(msgWriter, message, cancellationToken).ConfigureAwait(false);
                        await Console.Out.WriteLineAsync($"Sent cursor message '{message.RequestNumber}'");
                        // Debug.WriteLine($"Sent cursor message '{message.RequestNumber}'");

                        // var response = await new ValueTask<MongoResponseMessage>(completionSource, completionSource.Version)
                        //     .ConfigureAwait(false);
                        // completionSource.Reset();
                        var response = await completion.WaitWithCancellationAsync(cancellationToken)
                            .ConfigureAwait(false);

                        return await ParseAsync<TResp>(response, cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        _completionMap.TryRemove(message.RequestNumber, out _);
                    }
                }

                return ThrowHelper.ConnectionException<CursorResult<TResp>>(_endpoint);
            }

            return ThrowHelper.ObjectDisposedException<CursorResult<TResp>>(nameof(Channel));


            async ValueTask<CursorResult<T>> ParseAsync<T>(MongoResponseMessage message,
                CancellationToken cancellationToken)
            {
                var reader = _reader!;
                switch (message)
                {
                    case ResponseMsgMessage msgMessage:
                        // Debug.WriteLine($"Parsing msg message '{message.Header.ResponseTo}'");
                        if (SerializersMap.TryGetSerializer<T>(out var msgSerializer))
                        {
                            MsgBodyReader<T> bodyReader;
                            if (msgMessage.MsgHeader.PayloadType == 0)
                            {
                                bodyReader = new MsgType0BodyReader<T>(msgSerializer, msgMessage);
                            }
                            else if (msgMessage.MsgHeader.PayloadType == 1)
                            {
                                bodyReader = new MsgType1BodyReader<T>(msgSerializer, msgMessage);
                            }
                            else
                            {
                                return ThrowHelper.InvalidPayloadTypeException<CursorResult<T>>(msgMessage.MsgHeader
                                    .PayloadType);
                            }

                            await Console.Out.WriteLineAsync($"Parsing msg message '{message.Header.ResponseTo}'");
                            var result = await reader.ReadAsync(bodyReader, cancellationToken).ConfigureAwait(false);
                            await Console.Out.WriteLineAsync(
                                $"Parsing msg message '{message.Header.ResponseTo}' complete");
                            reader.Advance();
                            // Debug.WriteLine($"Parsing msg message '{message.Header.ResponseTo}' complete");
                            return bodyReader.CursorResult;
                        }

                        return ThrowHelper.UnsupportedTypeException<CursorResult<T>>(typeof(T));
                    default:
                        return ThrowHelper.UnsupportedTypeException<CursorResult<T>>(typeof(T));
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

        #region NeedToRemove

        // public async ValueTask<TResp> SendAsync<TResp>(ReadOnlyMemory<byte> message,
        //     CancellationToken cancellationToken)
        // {
        //     if (_shutdownToken.IsCancellationRequested == false)
        //     {
        //         if (_writer is not null)
        //         {
        //             await _writer.WriteAsync(memoryWriter, message, cancellationToken).ConfigureAwait(false);
        //
        //             var completion = _completionMap.GetOrAdd(message.RequestNumber,
        //                 i => new TaskCompletionSource<MongoResponseMessage>());
        //             // var response = await new ValueTask<MongoResponseMessage>(completionSource, completionSource.Version)
        //             //     .ConfigureAwait(false);
        //             // completionSource.Reset();
        //             var response = await completion.Task.ConfigureAwait(false);
        //             _completionMap.TryRemove(message.RequestNumber, out _);
        //             return await ParseAsync<TResp>(response).ConfigureAwait(false);
        //         }
        //
        //         return ThrowHelper.ConnectionException<TResp>(_endpoint);
        //     }
        //
        //     return ThrowHelper.ObjectDisposedException<TResp>(nameof(Channel));
        //
        //
        //     async ValueTask<T> ParseAsync<T>(MongoResponseMessage message)
        //     {
        //         var reader = _reader!;
        //         switch (message)
        //         {
        //             case ReplyMessage replyMessage:
        //                 if (SerializersMap.TryGetSerializer<T>(out var replySerializer))
        //                 {
        //                     var bodyReader = new ReplyBodyReader<T>(replySerializer);
        //                     var bodyResult = await reader.ReadAsync(bodyReader, _shutdownToken.Token)
        //                         .ConfigureAwait(false);
        //                     reader.Advance();
        //                     return bodyResult.Message;
        //                 }
        //
        //                 return ThrowHelper.UnsupportedTypeException<T>(typeof(T));
        //             default:
        //                 return ThrowHelper.UnsupportedTypeException<T>(typeof(T));
        //         }
        //     }
        // }

        #endregion
    }
}