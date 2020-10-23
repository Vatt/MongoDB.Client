using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Reader;

namespace MongoDB.Client.Bson.Serialization
{
    public interface IGenericBsonSerializer<T> : IBsonSerializable
    {
        bool GenericTryParse(ref MongoDBBsonReader reader, [MaybeNullWhen(false)] out T message)
        {
            if (TryParse(ref reader, out var result))
            {
                message = (T)result;
                return true;
            }
            message = default;
            return false;
        }
    }
}
