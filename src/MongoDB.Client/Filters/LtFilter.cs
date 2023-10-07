using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    internal sealed class LtFilter<T> : Filter
    {
        private readonly string _propertyName;
        private readonly T? _value;
        public LtFilter(string propertyName, T? value)
        {
            _propertyName = propertyName;
            _value = value;
        }
        public override void Write(ref BsonWriter writer)
        {
            var checkpoint = writer.Written;

            var reserved = writer.Reserve(sizeof(int));

            var typeReserved = writer.Reserve(sizeof(byte));
            writer.WriteName(_propertyName);
            writer.WriteGeneric(_value, ref typeReserved);
            writer.WriteByte((byte)'\x00');

            reserved.Write(writer.Written - checkpoint);
        }
    }
}
