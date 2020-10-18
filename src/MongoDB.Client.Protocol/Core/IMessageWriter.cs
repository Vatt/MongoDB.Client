using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Core
{
    public interface IMessageWriter<TMessage>
    {
        void WriteMessage(TMessage message, IBufferWriter<byte> output);
    }
}