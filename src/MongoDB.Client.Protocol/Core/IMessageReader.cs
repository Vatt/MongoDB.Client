using System;
using System.Buffers;

namespace MongoDB.Client.Protocol.Core
{
    public interface IMessageReader<TMessage>
    {
        bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out TMessage message);
    }
}