using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Bson.Serialization
{
    public interface IGenericBsonSerializer<T> : IBsonSerializer
    {
        bool TryParseBson(ref BsonReader reader, [MaybeNullWhen(false)] out T message);
        void WriteBson(ref BsonWriter writer, in T message);
    }
}
