using MongoDB.Client.Bson.Document;
using MongoDB.Client.Messages;
using MongoDB.Client.Network;
using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Protocol.Writers;
using MongoDB.Client.Readers;
using System;
using System.Net;
using System.Net.Connections;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    internal class Channel : IAsyncDisposable
    {
        private readonly EndPoint _endpoint;
        private readonly NetworkConnectionFactory _connectionFactory;
        private System.Net.Connections.Connection? _connection;
        private ProtocolReader? _reader;
        private ProtocolWriter? _writer;
        private static readonly MessageHeaderReader messageHeaderReader = new MessageHeaderReader();
        private static readonly ReplyMessageReader replyMessageReader = new ReplyMessageReader();
        private static readonly MsgMessageReader msgMessageReader = new MsgMessageReader();

        private static readonly ReadOnlyMemoryWriter memoryWriter = new ReadOnlyMemoryWriter();
        private static readonly QueryMessageWriter queryWriter = new QueryMessageWriter();

        private readonly ManualResetValueTaskSource<MongoMessage> completionSource = new ManualResetValueTaskSource<MongoMessage>();
        private CancellationTokenSource _shutdownToken = new CancellationTokenSource();
        private Task? _readingTask;

        public Channel(EndPoint endpoint)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _connectionFactory = new NetworkConnectionFactory();
        }

        internal async Task ConnectAsync(CancellationToken cancellationToken)
        {
            _connection = await _connectionFactory.ConnectAsync(_endpoint, null, cancellationToken).ConfigureAwait(false);
            if (_connection is null)
            {
                ThrowHelper.ConnectionException<bool>(_endpoint);
            }

            _reader = new ProtocolReader(_connection.Pipe.Input);
            _writer = new ProtocolWriter(_connection.Pipe.Output);
            _readingTask = StartReadAsync();
        }


        private async Task StartReadAsync()
        {
            if (_reader is null)
            {
                ThrowHelper.ConnectionException<bool>(_endpoint);
            }
            MongoMessage message;
            while (_shutdownToken.IsCancellationRequested == false)
            {
                var headerResult = await _reader.ReadAsync(messageHeaderReader, _shutdownToken.Token).ConfigureAwait(false);
                _reader.Advance();
                switch (headerResult.Message.Opcode)
                {
                    case Opcode.Reply:
                        var replyResult = await _reader.ReadAsync(replyMessageReader, _shutdownToken.Token).ConfigureAwait(false);
                        _reader.Advance();
                        message = new ReplyMessage(headerResult.Message, replyResult.Message);
                        completionSource.TrySetResult(message);
                        break;
                    case Opcode.OpMsg:
                        var msgResult = await _reader.ReadAsync(msgMessageReader, _shutdownToken.Token).ConfigureAwait(false);
                        _reader.Advance();
                        message = new MsgMessage(headerResult.Message, msgResult.Message);
                        completionSource.TrySetResult(message);
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
                        ThrowHelper.OpcodeNotSupportedException<bool>(headerResult.Message.Opcode); //TODO: need to read pipe to end
                        break;
                }
            }
        }

        public async ValueTask<TResp> SendAsync<TResp>(ReadOnlyMemory<byte> message, CancellationToken cancellationToken)
        {
            if (_shutdownToken.IsCancellationRequested == false)
            {
                if (_writer is not null)
                {
                    await _writer.WriteAsync(memoryWriter, message, cancellationToken).ConfigureAwait(false);

                    var response = await new ValueTask<MongoMessage>(completionSource, completionSource.Version).ConfigureAwait(false);
                    completionSource.Reset();
                    return await ParseAsync<TResp>(response).ConfigureAwait(false);
                }

                return ThrowHelper.ConnectionException<TResp>(_endpoint);
            }
            return ThrowHelper.ObjectDisposedException<TResp>(nameof(Channel));


            async ValueTask<T> ParseAsync<T>(MongoMessage message)
            {
                var reader = _reader!;
                switch (message)
                {
                    case ReplyMessage replyMessage:
                        if (SerializersMap.TryGetSerializer<T>(out var replySerializer))
                        {
                            var bodyReader = new ReplyBodyReader<T>(replySerializer);
                            var bodyResult = await reader.ReadAsync(bodyReader, _shutdownToken.Token).ConfigureAwait(false);
                            reader.Advance();
                            return bodyResult.Message;
                        }

                        return ThrowHelper.UnsupportedTypeException<T>(typeof(T));
                    default:
                        return ThrowHelper.UnsupportedTypeException<T>(typeof(T));
                }
            }
        }

        public async ValueTask<TResp> SendQueryAsync<TResp>(QueryMessage message, CancellationToken cancellationToken)
        {
            if (_shutdownToken.IsCancellationRequested == false)
            {
                if (_writer is not null)
                {
                    await _writer.WriteAsync(queryWriter, message, cancellationToken).ConfigureAwait(false);

                    var response = await new ValueTask<MongoMessage>(completionSource, completionSource.Version).ConfigureAwait(false);
                    completionSource.Reset();
                    return await ParseAsync<TResp>(response).ConfigureAwait(false);
                }

                return ThrowHelper.ConnectionException<TResp>(_endpoint);
            }
            return ThrowHelper.ObjectDisposedException<TResp>(nameof(Channel));


            async ValueTask<T> ParseAsync<T>(MongoMessage message)
            {
                var reader = _reader!;
                switch (message)
                {
                    case ReplyMessage replyMessage:
                        if (SerializersMap.TryGetSerializer<T>(out var replySerializer))
                        {
                            var bodyReader = new ReplyBodyReader<T>(replySerializer);
                            var bodyResult = await reader.ReadAsync(bodyReader, _shutdownToken.Token).ConfigureAwait(false);
                            reader.Advance();
                            return bodyResult.Message;
                        }

                        return ThrowHelper.UnsupportedTypeException<T>(typeof(T));
                    default:
                        return ThrowHelper.UnsupportedTypeException<T>(typeof(T));
                }
            }
        }

        public async ValueTask<Cursor<TResp>> GetCursorAsync<TResp>(ReadOnlyMemory<byte> message, CancellationToken cancellationToken)
        {
            if (_shutdownToken.IsCancellationRequested == false)
            {
                if (_writer is not null)
                {
                    await _writer.WriteAsync(memoryWriter, message, cancellationToken).ConfigureAwait(false);

                    var response = await new ValueTask<MongoMessage>(completionSource, completionSource.Version).ConfigureAwait(false);
                    completionSource.Reset();
                    return await ParseAsync<TResp>(response).ConfigureAwait(false);
                }

                return ThrowHelper.ConnectionException<Cursor<TResp>>(_endpoint);
            }
            return ThrowHelper.ObjectDisposedException<Cursor<TResp>>(nameof(Channel));


            async ValueTask<Cursor<T>> ParseAsync<T>(MongoMessage message)
            {
                var reader = _reader!;
                switch (message)
                {
                    case MsgMessage msgMessage:
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
                                return ThrowHelper.InvalidPayloadTypeException<Cursor<T>>(msgMessage.MsgHeader.PayloadType);
                            }

                            while (bodyReader.Complete == false)
                            {
                                _ = await reader.ReadAsync(bodyReader, _shutdownToken.Token).ConfigureAwait(false);
                                reader.Advance();
                            }

                            return bodyReader.CursorResult.Cursor;
                        }

                        return ThrowHelper.UnsupportedTypeException<Cursor<T>>(typeof(T));
                    default:
                        return ThrowHelper.UnsupportedTypeException<Cursor<T>>(typeof(T));
                }
            }
        }

        private BsonDocument GetHelloMessage()
        {
            var root = new BsonDocument
            {
                {"driver",  new BsonDocument
                {
                    {"driver",  "MongoDB.Client"},
                    {"version", "0.0.0" }
                } }
            };

            return root;
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
    }
}
