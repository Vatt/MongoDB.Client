using System;
using MongoDB.Client.Bson.Reader;

namespace MongoDB.Client.Bson.Serialization
{
    public interface IBsonSerializable
    {
        bool TryParse(ref MongoDBBsonReader reader, out object message);

        bool TryParse(ref MongoDBBsonReader reader, ref SequencePosition consumed, ref SequencePosition examined, out object message)
        {
            message = null;
            return true;
        }
        
        void Write(object message);
    }
}
