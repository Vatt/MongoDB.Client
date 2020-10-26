using MongoDB.Client.Bson.Reader;
using System.Diagnostics.CodeAnalysis;

namespace MongoDB.Client.Bson.Serialization
{
    public interface IGenericBsonSerializer<T> : IBsonSerializer
    {
        bool TryParse(ref BsonReader reader, [MaybeNullWhen(false)] out T message);
    }
}
