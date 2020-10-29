using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;
using System;

namespace MongoDB.Client.Bson.Serialization
{
    public class BsonDocumentSerializer : IGenericBsonSerializer<BsonDocument>
    {
        bool IGenericBsonSerializer<BsonDocument>.TryParse(ref BsonReader reader, out BsonDocument message)
        {
            return reader.TryParseDocument(out message);
        }

        public void Write(ref BsonWriter writer, in BsonDocument message)
        {
            writer.WriteDocument(message);
        }
    }
}
