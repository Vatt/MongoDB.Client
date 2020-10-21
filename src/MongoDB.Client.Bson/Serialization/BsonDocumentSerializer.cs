using System;
using MongoDB.Client.Bson.Reader;

namespace MongoDB.Client.Bson.Serialization
{
    public class BsonDocumentSerializer : IBsonSerializable
    {
        public bool TryParse(ref MongoDBBsonReader reader, out object message)
        {
            var parseResult = reader.TryParseDocument(out var doc);
            message = doc;
            return parseResult;
        }

        public void Write(object message) => throw new NotImplementedException();
    }
}
