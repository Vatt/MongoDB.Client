using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    internal class InFilter<T> : Filter
    {
        private string _propertyName;
        private T[] _values;
        public InFilter(string propertyName, T[] values)
        {
            _propertyName = propertyName;
            _values = values;
        }

        public override void Write(ref BsonWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
