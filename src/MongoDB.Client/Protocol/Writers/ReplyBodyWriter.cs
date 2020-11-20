using System.Buffers;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Protocol.Writers
{
    public class ReplyBodyWriter<T> : IMessageWriter<T>
    {
        private readonly IGenericBsonSerializer<T> _serializer;

        public ReplyBodyWriter(IGenericBsonSerializer<T> serializer)
        {
            _serializer = serializer;
        }

        public void WriteMessage(T message, IBufferWriter<byte> output)
        {
            var writer = new BsonWriter(output);
            _serializer.Write(ref writer, message);
        }
    }
}
