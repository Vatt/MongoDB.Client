using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Connections;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Core;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Network;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Protocol.Writers;

namespace MongoDB.Client
{
    internal class Channel : IAsyncDisposable
    {
        private readonly EndPoint _endpoint;
        private readonly NetworkConnectionFactory _connectionFactory;
        private Connection? _connection;
        private ProtocolReader? _reader;
        private ProtocolWriter? _writer;
        private static readonly MessageHeaderReader messageHeaderReader = new MessageHeaderReader();
        private static readonly ReplyMessageReader replyMessageReader = new ReplyMessageReader();

        private static readonly MemoryWriter memoryWriter = new MemoryWriter();

        private static ReadOnlyMemory<byte> Hello = new byte[] { 42, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 212, 7, 0, 0, 4, 0, 0, 0, 97, 100, 109, 105, 110, 46, 36, 99, 109, 100, 0, 0, 0, 0, 0, 255, 255, 255, 255, 3, 1, 0, 0, 16, 105, 115, 77, 97, 115, 116, 101, 114, 0, 1, 0, 0, 0, 3, 99, 108, 105, 101, 110, 116, 0, 214, 0, 0, 0, 3, 100, 114, 105, 118, 101, 114, 0, 56, 0, 0, 0, 2, 110, 97, 109, 101, 0, 20, 0, 0, 0, 109, 111, 110, 103, 111, 45, 99, 115, 104, 97, 114, 112, 45, 100, 114, 105, 118, 101, 114, 0, 2, 118, 101, 114, 115, 105, 111, 110, 0, 8, 0, 0, 0, 48, 46, 48, 46, 48, 46, 48, 0, 0, 3, 111, 115, 0, 111, 0, 0, 0, 2, 116, 121, 112, 101, 0, 8, 0, 0, 0, 87, 105, 110, 100, 111, 119, 115, 0, 2, 110, 97, 109, 101, 0, 29, 0, 0, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 87, 105, 110, 100, 111, 119, 115, 32, 49, 48, 46, 48, 46, 49, 57, 48, 52, 49, 0, 2, 97, 114, 99, 104, 105, 116, 101, 99, 116, 117, 114, 101, 0, 7, 0, 0, 0, 120, 56, 54, 95, 54, 52, 0, 2, 118, 101, 114, 115, 105, 111, 110, 0, 11, 0, 0, 0, 49, 48, 46, 48, 46, 49, 57, 48, 52, 49, 0, 0, 2, 112, 108, 97, 116, 102, 111, 114, 109, 0, 16, 0, 0, 0, 46, 78, 69, 84, 32, 67, 111, 114, 101, 32, 51, 46, 49, 46, 57, 0, 0, 4, 99, 111, 109, 112, 114, 101, 115, 115, 105, 111, 110, 0, 5, 0, 0, 0, 0, 0 };
        private TaskCompletionSource<MongoMessage> completionSource = new TaskCompletionSource<MongoMessage>();
        private CancellationTokenSource _shutdownToken = new CancellationTokenSource();
        private Task? _readingTask;

        private static readonly Dictionary<Type, IBsonSerializable> _serializerMap = new Dictionary<Type, IBsonSerializable>
        {
            [typeof(BsonDocument)] = new BsonDocumentSerializer()
        };

        public Channel(EndPoint endpoint)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _connectionFactory = new NetworkConnectionFactory();
        }

        public async Task<BsonDocument> SendHelloAsync(CancellationToken cancellationToken)
        {
            _connection = await _connectionFactory.ConnectAsync(_endpoint, null, cancellationToken).ConfigureAwait(false);
            if (_connection is null)
            {
                ThrowHelper.ConnectionException(_endpoint);
            }

            _reader = new ProtocolReader(_connection.Pipe.Input);
            _writer = new ProtocolWriter(_connection.Pipe.Output);
            _readingTask = StartReadAsync();
            return await SendAsync<BsonDocument>(Hello, cancellationToken);
        }

        private async Task StartReadAsync()
        {
            if (_reader is null)
            {
                ThrowHelper.ConnectionException(_endpoint);
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
                    case Opcode.Message:
                    case Opcode.Update:
                    case Opcode.Insert:
                    case Opcode.Query:
                    case Opcode.GetMore:
                    case Opcode.Delete:
                    case Opcode.KillCursors:
                    case Opcode.Compressed:
                    case Opcode.OpMsg:
                    default:
                        ThrowHelper.OpcodeNotSupportedException(headerResult.Message.Opcode); //TODO: need to read pipe to end
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
                    completionSource = new TaskCompletionSource<MongoMessage>();

                    await _writer.WriteAsync(memoryWriter, message, cancellationToken).ConfigureAwait(false);

                    var response = await completionSource.Task.ConfigureAwait(false);
                    return await ParseAsync<TResp>(response).ConfigureAwait(false);
                }

                ThrowHelper.ConnectionException(_endpoint);
                return default;
            }
            ThrowHelper.ObjectDisposedException(nameof(Channel));
            return default;


            // Temp implementation
            async ValueTask<T> ParseAsync<T>(MongoMessage message)
            {
                switch (message)
                {
                    case ReplyMessage replyMessage:
                        if (_serializerMap.TryGetValue(typeof(T), out var serializer))
                        {
                            var bodyReader = new BodyReader(serializer);
                            var bodyResult = await _reader!.ReadAsync(bodyReader, _shutdownToken.Token).ConfigureAwait(false);
                            _reader.Advance();
                            return (T)bodyResult.Message;
                        }

                        ThrowHelper.UnsupportedTypeException(typeof(T));
                        return default;
                    default:
                        ThrowHelper.UnsupportedTypeException(typeof(T));
                        return default;
                }
            }
        }

        private BsonDocument GetHelloMessage()
        {
            var root = new BsonDocument();
            var driverDoc = new BsonDocument();

            driverDoc.Elements.AddRange(new List<BsonElement>{
                BsonElement.Create(driverDoc, "driver", "MongoDB.Client"),
                BsonElement.Create(driverDoc, "version", "0.0.0"),
            });
            root.Elements.AddRange(new List<BsonElement>
            {
                BsonElement.Create(root, "driver", driverDoc)
            });

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
