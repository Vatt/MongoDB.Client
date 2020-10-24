using MongoDB.Client.Bson.Reader;
using System.Diagnostics.CodeAnalysis;

namespace MongoDB.Client.Bson.Serialization
{
    public interface IGenericBsonSerializer<T> : IBsonSerializer
    {
        bool TryParse(ref MongoDBBsonReader reader, [MaybeNullWhen(false)] out T message);
        //{
        //    if (TryParse(ref reader, out var result))
        //    {
        //        message = (T)result;
        //        return true;
        //    }
        //    message = default;
        //    return false;
        //}
    }
}
