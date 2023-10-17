using MongoDB.Client.Bson;
using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    public enum FilterType
    {
        Gt = 1,
        Gte,
        Lt,
        Lte,
        Ne,
        Eq
    }
    public class Filter<T> : Filter
    {
        public string PropertyName { get; protected set; }
        public T? Value { get; protected set; }
        public FilterType Operation { get; protected set; }
        public Filter(string propertyName, T? value, FilterType type)
        {
            PropertyName = propertyName;
            Value = value;
            Operation = type;
        }
        public override void Write(ref BsonWriter writer)
        {
            if (Operation is FilterType.Eq)
            {
                WriteEq(ref writer);
                return;
            }

            var checkpoint = writer.Written;
            var reserved = writer.Reserve(sizeof(int));
            writer.WriteBsonType(BsonType.Document);
            writer.WriteName(PropertyName);
            var checkpoint1 = writer.Written;
            var reserved1 = writer.Reserve(sizeof(int));
            var typeReserved = writer.Reserve(sizeof(byte));

            switch (Operation)
            {
                case FilterType.Gt:
                    writer.WriteName("$gt"u8);
                    break;
                case FilterType.Gte:
                    writer.WriteName("$gte"u8);
                    break;
                case FilterType.Lt:
                    writer.WriteName("$lt"u8);
                    break;
                case FilterType.Lte:
                    writer.WriteName("$lte"u8);
                    break;
                case FilterType.Ne:
                    writer.WriteName("$ne"u8);
                    break;
            }

            writer.WriteGeneric(Value, ref typeReserved);
            writer.WriteByte((byte)'\x00');
            reserved1.Write(writer.Written - checkpoint1);
            writer.WriteByte(0);

            reserved.Write(writer.Written - checkpoint);
        }
        private void WriteEq(ref BsonWriter writer)
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
