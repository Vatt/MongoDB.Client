using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    public class EqFilter<T> : Filter
    {
        public string PropertyName { get; protected set; }
        public T? Value { get; protected set; }
        public EqFilter(string propertyName, T? value)
        {
            PropertyName = propertyName;
            Value = value;
        }
        public override void Write(ref BsonWriter writer)
        {
            var checkpoint = writer.Written;

            var reserved = writer.Reserve(sizeof(int));

            var typeReserved = writer.Reserve(sizeof(byte));
            writer.WriteName(PropertyName);
            writer.WriteGeneric(Value, ref typeReserved);
            writer.WriteByte((byte)'\x00');

            reserved.Write(writer.Written - checkpoint);
        }
    }
}
