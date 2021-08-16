using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Bson.Serialization
{
    public interface IBsonSerializer<T>
    {
        static abstract bool TryParseBson(ref BsonReader reader, out SerializerStateBase state);
        static abstract bool TryContinueParseBson(ref BsonReader reader, SerializerStateBase state);
        static abstract T CreateMessage(SerializerStateBase state);
        static abstract void WriteBson(ref BsonWriter writer, in T message);
    }
}
