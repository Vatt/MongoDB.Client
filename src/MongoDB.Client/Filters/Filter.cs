using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    public abstract class Filter : IBsonSerializer<Filter>
    {
        public static readonly Filter Empty = new EmptyFilter();
        public abstract void Write(ref BsonWriter writer);
        public static Filter Document(BsonDocument document) => new BsonDocumentFilter(document);
        public static void WriteBson(ref BsonWriter writer, in Filter message)
        {
            message.Write(ref writer);
        }

        public static bool TryParseBson(ref BsonReader reader, out Filter message) => throw new NotImplementedException();
    }
}
