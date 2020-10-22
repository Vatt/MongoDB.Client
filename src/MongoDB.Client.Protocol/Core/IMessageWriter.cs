using System.Buffers;

namespace MongoDB.Client.Protocol.Core
{
    public interface IMessageWriter<TMessage>
    {
        void WriteMessage(TMessage message, IBufferWriter<byte> output);
    }
}