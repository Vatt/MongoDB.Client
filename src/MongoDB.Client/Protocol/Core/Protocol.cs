using Microsoft.AspNetCore.Connections;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;

namespace MongoDB.Client.Protocol.Core
{
    public static class Protocol
    {
        public static ProtocolWriter CreateWriter(this System.Net.Connections.Connection connection)
            => new ProtocolWriter(connection.Pipe.Output);

        public static ProtocolWriter CreateWriter(this System.Net.Connections.Connection connection, SemaphoreSlim semaphore)
            => new ProtocolWriter(connection.Pipe.Output, semaphore);

        public static ProtocolReader CreateReader(this System.Net.Connections.Connection connection)
            => new ProtocolReader(connection.Pipe.Input);

        public static PipeReader CreatePipeReader(this System.Net.Connections.Connection connection, IMessageReader<ReadOnlySequence<byte>> messageReader)
            => new MessagePipeReader(connection.Pipe.Input, messageReader);
    }
}