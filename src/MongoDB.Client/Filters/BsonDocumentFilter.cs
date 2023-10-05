using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    internal sealed class BsonDocumentFilter : Filter
    {
        private readonly BsonDocument _document;
        public BsonDocumentFilter(BsonDocument document)
        {
            _document = document;
        }
        public override void Write(ref BsonWriter writer)
        {
            BsonDocument.WriteBson(ref writer, _document);
        }
    }
}
