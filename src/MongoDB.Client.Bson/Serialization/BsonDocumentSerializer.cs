using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using System;

namespace MongoDB.Client.Bson.Serialization
{
    public class BsonDocumentSerializer : IGenericBsonSerializer<BsonDocument>
    {
        public bool TryParse(ref MongoDBBsonReader reader, out object message)
        {
            var parseResult = reader.TryParseDocument(out var doc);
            message = doc;
            return parseResult;
        }

        bool GenericTryParse(ref MongoDBBsonReader reader, out BsonDocument message)
        {
            return reader.TryParseDocument(out message);
        }

        public void Write(object message) => throw new NotImplementedException();
    }
}
