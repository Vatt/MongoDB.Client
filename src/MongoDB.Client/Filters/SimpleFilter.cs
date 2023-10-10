using MongoDB.Client.Bson;
using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    internal enum FilterOp
    {
        Gt = 1,
        Gte,
        Lt,
        Lte,
    }
    internal sealed class Filter<T> : Filter
    {
        private readonly string _propertyName;
        private readonly T? _value;
        private readonly FilterOp _operation;
        public Filter(string propertyName, T? value, FilterOp operation)
        {
            _propertyName = propertyName;
            _value = value;
            _operation = operation;
        }
        public override void Write(ref BsonWriter writer)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(sizeof(int));
            writer.WriteBsonType(BsonType.Document);
            writer.WriteName(_propertyName);
            var checkpoint1 = writer.Written;
            var reserved1 = writer.Reserve(sizeof(int));
            var typeReserved = writer.Reserve(sizeof(byte));

            switch (_operation)
            {
                case FilterOp.Gt:
                    writer.WriteName("$gt"u8);
                    break;
                case FilterOp.Gte:
                    writer.WriteName("$gte"u8);
                    break;
                case FilterOp.Lt:
                    writer.WriteName("$lt"u8);
                    break;
                case FilterOp.Lte:
                    writer.WriteName("$lte"u8);
                    break;
            }

            writer.WriteGeneric(_value, ref typeReserved);
            writer.WriteByte((byte)'\x00');
            reserved1.Write(writer.Written - checkpoint1);
            writer.WriteByte(0);

            reserved.Write(writer.Written - checkpoint);
        }
    }
}
