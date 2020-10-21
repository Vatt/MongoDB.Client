using System;
using MongoDB.Client.Bson.Reader;

namespace MongoDB.Client.Bson.Serialization
{
    public interface IBsonSerializable
    {
        bool TryParse(ref MongoDBBsonReader reader, out object message);
        
        void Write(object message);
    }
}
