using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Connections;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Network;

namespace MongoDB.Client
{
    internal class Channel : IAsyncDisposable
    {
        private readonly EndPoint _endpoint;
        private readonly NetworkConnectionFactory _connectionFactory;
        private Connection? _connection;
        private PipelineBinaryReader? _reader;
        private PipelineBinaryWriter? _writer;
        private static ReadOnlySpan<byte> Hello => new byte[] { 42, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 212, 7, 0, 0, 4, 0, 0, 0, 97, 100, 109, 105, 110, 46, 36, 99, 109, 100, 0, 0, 0, 0, 0, 255, 255, 255, 255, 3, 1, 0, 0, 16, 105, 115, 77, 97, 115, 116, 101, 114, 0, 1, 0, 0, 0, 3, 99, 108, 105, 101, 110, 116, 0, 214, 0, 0, 0, 3, 100, 114, 105, 118, 101, 114, 0, 56, 0, 0, 0, 2, 110, 97, 109, 101, 0, 20, 0, 0, 0, 109, 111, 110, 103, 111, 45, 99, 115, 104, 97, 114, 112, 45, 100, 114, 105, 118, 101, 114, 0, 2, 118, 101, 114, 115, 105, 111, 110, 0, 8, 0, 0, 0, 48, 46, 48, 46, 48, 46, 48, 0, 0, 3, 111, 115, 0, 111, 0, 0, 0, 2, 116, 121, 112, 101, 0, 8, 0, 0, 0, 87, 105, 110, 100, 111, 119, 115, 0, 2, 110, 97, 109, 101, 0, 29, 0, 0, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 87, 105, 110, 100, 111, 119, 115, 32, 49, 48, 46, 48, 46, 49, 57, 48, 52, 49, 0, 2, 97, 114, 99, 104, 105, 116, 101, 99, 116, 117, 114, 101, 0, 7, 0, 0, 0, 120, 56, 54, 95, 54, 52, 0, 2, 118, 101, 114, 115, 105, 111, 110, 0, 11, 0, 0, 0, 49, 48, 46, 48, 46, 49, 57, 48, 52, 49, 0, 0, 2, 112, 108, 97, 116, 102, 111, 114, 109, 0, 16, 0, 0, 0, 46, 78, 69, 84, 32, 67, 111, 114, 101, 32, 51, 46, 49, 46, 57, 0, 0, 4, 99, 111, 109, 112, 114, 101, 115, 115, 105, 111, 110, 0, 5, 0, 0, 0, 0, 0 };
        private TaskCompletionSource<ReplyMessage> completionSource = new TaskCompletionSource<ReplyMessage>();
        private CancellationTokenSource _shutdownToken = new CancellationTokenSource();
        private Task? _readingTask;

        //private static readonly Dictionary<Type, ISerializer> _serializerMap = new Dictionary<Type, ISerializer>
        //{
        //    [typeof(BsonDocument)] = new BsonDocumentSerializer(),
        //    [typeof(Data)] = new DataSerializer()
        //};

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
            _reader = new PipelineBinaryReader(_connection.Pipe.Input);
            _writer = new PipelineBinaryWriter(_connection.Pipe.Output);
            _readingTask = StartReadAsync();
            return await SendAsync<BsonDocument>(Hello, cancellationToken);
        }

        private async Task StartReadAsync()
        {
            if (_reader is null)
            {
                ThrowHelper.ConnectionException(_endpoint);
            }
            var messageReader = new FrameContentReader();
            while (_shutdownToken.IsCancellationRequested == false)
            {
                await _reader.ReadAsync(messageReader);
                var message = messageReader.Message;
                completionSource.TrySetResult(message);
                messageReader.Reset();
                _reader.Advance();
            }
        }

        public ValueTask<T> SendAsync<T>(ReadOnlySpan<byte> message, CancellationToken cancellationToken)
        {
            if (_shutdownToken.IsCancellationRequested == false)
            {
                if (_writer is not null)
                {
                    completionSource = new TaskCompletionSource<ReplyMessage>();

                    _writer.Write(message);

                    return WaitForResponse(cancellationToken);
                }

                ThrowHelper.ConnectionException(_endpoint);
                return default;
            }
            ThrowHelper.ObjectDisposedException(nameof(Channel));
            return default;


            async ValueTask<T> WaitForResponse(CancellationToken cancellationToken)
            {
                var flushTask = _writer.FlushAsync(cancellationToken);
                if (flushTask.IsCompleted == false)
                {
                    await flushTask.ConfigureAwait(false);
                }
                var response = await completionSource.Task.ConfigureAwait(false);
                return Parse<T>(response.Message);
            }


            // Temp implementation
            static T Parse<T>(ReadOnlyMemory<byte> message)
            {
                if (typeof(T) == typeof(BsonDocument))
                {
                    // var serializer = _serializerMap[typeof(T)];
                    var serializer = new MongoDBBsonReader(message);
                    serializer.TryParseDocument(null, out var doc);
                    return (T)(object)doc;
                }
                ThrowHelper.UnsupportedTypeException(typeof(T));
                return default;
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
