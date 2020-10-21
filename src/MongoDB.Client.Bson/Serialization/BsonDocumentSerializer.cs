using System;
using MongoDB.Client.Bson.Reader;

namespace MongoDB.Client.Bson.Serialization
{
    public class BsonDocumentSerializer : IBsonSerializable
    {
        public bool TryParse(ref MongoDBBsonReader reader, ref SequencePosition consumed, ref SequencePosition examined, out object message)
        {
            var parseResult = reader.TryParseDocument(ref consumed, ref examined, out var doc);
            message = doc;
            return parseResult;
        }

        public bool TryParse(ref MongoDBBsonReader reader, out object message) => throw new NotImplementedException();
        public void Write(object message) => throw new NotImplementedException();
    }
}
