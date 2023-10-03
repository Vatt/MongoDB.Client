using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    internal sealed class EqFilter<T> : Filter
    {
        private string _properyName;
        private T _value;
        public EqFilter(string properyName, T value)
        {
            _properyName = properyName;
            _value = value;
        }
        protected override void Write(ref BsonWriter writer)
        {
            var checkpoint = writer.Written;

            var reserved = writer.Reserve(sizeof(int));

            var typeReserved = writer.Reserve(sizeof(byte));
            writer.WriteName(_properyName);
            writer.WriteGeneric(_value, ref typeReserved);
            writer.WriteByte((byte)'\x00');

            reserved.Write(writer.Written - checkpoint);
        }
    }
}
