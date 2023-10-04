using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    internal sealed class AndFilter : Filter
    {
        private readonly Filter[] _inner;

        public AndFilter(params Filter[] inner)
        {
            _inner = inner;
        }

        protected override void Write(ref BsonWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
