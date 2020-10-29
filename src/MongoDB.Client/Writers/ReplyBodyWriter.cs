using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Protocol.Core;
using System.Buffers;

namespace MongoDB.Client.Writers
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
            //writer.WriteDocument((BsonDocument)(object)message);
            _serializer.Write(ref writer, message);
        }
    }
}
