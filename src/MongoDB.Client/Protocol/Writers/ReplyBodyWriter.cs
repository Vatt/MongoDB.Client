using System.Buffers;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Protocol.Writers
{
    public class ReplyBodyWriter<T> : IMessageWriter<T>
        //where T : IBsonSerializer<T>
    {
        public unsafe void WriteMessage(T message, IBufferWriter<byte> output)
        {
            var writer = new BsonWriter(output);
            //T.WriteBson(ref writer, message);
            SerializerFnPtrProvider<T>.WriteFnPtr(ref writer, message);
        }
    }
}
