using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using System;

namespace MongoDB.Client.Bson.Serialization
{
    public class BsonDocumentSerializer : IGenericBsonSerializer<BsonDocument>
    {
        bool IGenericBsonSerializer<BsonDocument>.TryParse(ref MongoDBBsonReader reader, out BsonDocument message)
        {
            return reader.TryParseDocument(out message);
        }

        public void Write(object message) => throw new NotImplementedException();

    }
}
