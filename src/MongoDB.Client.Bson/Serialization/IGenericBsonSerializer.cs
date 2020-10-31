using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;
using System.Diagnostics.CodeAnalysis;

namespace MongoDB.Client.Bson.Serialization
{
    public interface IGenericBsonSerializer<T> : IBsonSerializer
    {
        bool TryParse(ref BsonReader reader, [MaybeNullWhen(false)] out T message);
        void Write(ref BsonWriter writer, in T message);
    }
}
