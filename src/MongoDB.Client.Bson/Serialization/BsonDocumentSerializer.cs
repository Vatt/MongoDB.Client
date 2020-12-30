using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Bson.Serialization
{
    public class BsonDocumentSerializer : IGenericBsonSerializer<BsonDocument>
    {
        bool IGenericBsonSerializer<BsonDocument>.TryParseBson(ref BsonReader reader, out BsonDocument message)
        {
            return reader.TryParseDocument(out message);
        }

        public void WriteBson(ref BsonWriter writer, in BsonDocument message)
        {
            writer.WriteDocument(message);
        }
    }
}
