using Microsoft.AspNetCore.Connections;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;

namespace MongoDB.Client.Protocol.Core
{
    public static class Protocol
    {
        public static ProtocolWriter CreateWriter(this ConnectionContext connection)
            => new ProtocolWriter(connection.Transport.Output);

        public static ProtocolWriter CreateWriter(this ConnectionContext connection, SemaphoreSlim semaphore)
            => new ProtocolWriter(connection.Transport.Output, semaphore);

        public static ProtocolReader CreateReader(this ConnectionContext connection)
            => new ProtocolReader(connection.Transport.Input);

        public static PipeReader CreatePipeReader(this ConnectionContext connection, IMessageReader<ReadOnlySequence<byte>> messageReader)
            => new MessagePipeReader(connection.Transport.Input, messageReader);
    }
}