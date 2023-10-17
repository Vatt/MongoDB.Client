using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    public enum ArrayFilterType
    {
        Any = 1,
        All,
    }
    public class ArrayFilter<T> : Filter
    {
        public string PropertyName { get; protected set; }
        public ArrayFilterType Type { get; protected set; }
        public List<ArrayOperator<T>> Inner { get; protected set; }
        public int? Size { get; protected set; }

        public ArrayFilter(string propertyName, ArrayFilterType type, int? size)
        {
            PropertyName = propertyName;
            Inner = new();
            Type = type;
            Size = size;
        }
        public ArrayFilter(string propertyName, ArrayFilterType type)
        {
            PropertyName = propertyName;
            Inner = new();
            Type = type;
            Size = null;
        }
        public override void Write(ref BsonWriter writer)
        {
            throw new NotImplementedException();
        }
        public void Add()
        {

        }
    }
    public class ArrayOperator<T> : Filter
    {
        public T? Value { get; protected set; }
        public FilterType Operation { get; protected set; }
        public override void Write(ref BsonWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
