using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Core;
using System;
using System.IO.Pipelines;
using System.Net.Connections;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    public class MongoDBSession : IAsyncDisposable
    {
        private static ReadOnlyMemory<byte> Hello = new byte[] { 42, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 212, 7, 0, 0, 4, 0, 0, 0, 97, 100, 109, 105, 110, 46, 36, 99, 109, 100, 0, 0, 0, 0, 0, 255, 255, 255, 255, 3, 1, 0, 0, 16, 105, 115, 77, 97, 115, 116, 101, 114, 0, 1, 0, 0, 0, 3, 99, 108, 105, 101, 110, 116, 0, 214, 0, 0, 0, 3, 100, 114, 105, 118, 101, 114, 0, 56, 0, 0, 0, 2, 110, 97, 109, 101, 0, 20, 0, 0, 0, 109, 111, 110, 103, 111, 45, 99, 115, 104, 97, 114, 112, 45, 100, 114, 105, 118, 101, 114, 0, 2, 118, 101, 114, 115, 105, 111, 110, 0, 8, 0, 0, 0, 48, 46, 48, 46, 48, 46, 48, 0, 0, 3, 111, 115, 0, 111, 0, 0, 0, 2, 116, 121, 112, 101, 0, 8, 0, 0, 0, 87, 105, 110, 100, 111, 119, 115, 0, 2, 110, 97, 109, 101, 0, 29, 0, 0, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 87, 105, 110, 100, 111, 119, 115, 32, 49, 48, 46, 48, 46, 49, 57, 48, 52, 49, 0, 2, 97, 114, 99, 104, 105, 116, 101, 99, 116, 117, 114, 101, 0, 7, 0, 0, 0, 120, 56, 54, 95, 54, 52, 0, 2, 118, 101, 114, 115, 105, 111, 110, 0, 11, 0, 0, 0, 49, 48, 46, 48, 46, 49, 57, 48, 52, 49, 0, 0, 2, 112, 108, 97, 116, 102, 111, 114, 109, 0, 16, 0, 0, 0, 46, 78, 69, 84, 32, 67, 111, 114, 101, 32, 51, 46, 49, 46, 57, 0, 0, 4, 99, 111, 109, 112, 114, 101, 115, 115, 105, 111, 110, 0, 5, 0, 0, 0, 0, 0 };

        private Connection _connection;

        public PipeReader Input => _connection.Pipe.Input;
        public PipeWriter Output => _connection.Pipe.Output;

        private ValueTask _listenerTask;
        private CancellationTokenSource _shutdownToken = new CancellationTokenSource();
        private TaskCompletionSource<MongoMessage> _completionSource = new TaskCompletionSource<MongoMessage>();
        private ProtocolReader _protocolReader;
        private ProtocolWriter _protocolWriter;
        public MongoDBSession(Connection connection)
        {
            _connection = connection;
            _protocolReader = _connection.CreateReader();
            _protocolWriter = _connection.CreateWriter();
            _listenerTask = StartListening();

        }
        private async ValueTask StartListening()
        {
            while (!_shutdownToken.IsCancellationRequested)
            {
                var header = await ReadAsyncInternal(ProtocolReaders.MessageHeaderReader, _shutdownToken.Token).ConfigureAwait(false);
                switch (header.Opcode)
                {
                    case Opcode.Reply:
                        var reply = await ReadAsyncInternal(ProtocolReaders.ReplyMessageReader, _shutdownToken.Token).ConfigureAwait(false);
                        var message = new ReplyMessage(header, reply);
                        _completionSource.TrySetResult(message);
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
                        ThrowHelper.OpcodeNotSupportedException<bool>(header.Opcode); //TODO: need to read pipe to end
                        break;
                }
            }
        }
        //public async Task<MongoDBConnectionInfo?> SayHelloAsync(CancellationToken cancellationToken = default)
        //{
        //    var protocol = _connection.CreateWriter();
        //    if (_shutdownToken.IsCancellationRequested == false)
        //    {
        //        if (protocol is not null)
        //        {
        //            _completionSource = new TaskCompletionSource<MongoMessage>();

        //            await protocol.WriteAsync(ProtocolWriters.ReadOnlyMemoryWriter, Hello, cancellationToken).ConfigureAwait(false);

        //            var response = await _completionSource.Task.ConfigureAwait(false);
        //            var bodyReader = new BodyReader(MongoDB.Client.Bson.Serialization.Generated.GlobalSerializationHelperGenerated.MongoDBConnectionInfoGeneratedSerializatorStaticField);
        //            return await ReadAsyncInternal(bodyReader, _shutdownToken.Token) as MongoDBConnectionInfo;
        //        }

        //        return ThrowHelper.ConnectionException<MongoDBConnectionInfo>(_connection.RemoteEndPoint!);
        //    }
        //    return ThrowHelper.ObjectDisposedException<MongoDBConnectionInfo>(nameof(Channel));

        //}

        public async ValueTask DisposeAsync()
        {
            await _connection.Pipe.Input.CompleteAsync().ConfigureAwait(false);
            await _connection.Pipe.Output.CompleteAsync().ConfigureAwait(false);
            await _connection.CloseAsync().ConfigureAwait(false);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<T> ReadAsyncInternal<T>(IMessageReader<T> reader, CancellationToken token = default)
        {
            var result = await _protocolReader.ReadAsync(reader, token).ConfigureAwait(false);
            _protocolReader.Advance();
            if (result.IsCanceled || result.IsCompleted)
            {
                //TODO: do something
            }

            return result.Message;
        }
    }
}
