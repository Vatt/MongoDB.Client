using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    internal class EmptyFilter : Filter
    {
        private static ReadOnlySpan<byte> _bytes => new byte[5] { 5, 0, 0, 0, 0 };
        public override void Write(ref BsonWriter writer)
        {
            writer.WriteBytes(_bytes);
        }
    }
}
