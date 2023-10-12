using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    public enum RangeFilterType
    {
        In = 1,
        NotIn,
    }
    public class RangeFilter<T> : Filter
    {
        private string _propertyName;
        private T[] _values;
        private RangeFilterType _type;
        public RangeFilter(string propertyName, T[] values, RangeFilterType type)
        {
            _propertyName = propertyName;
            _values = values;
            _type = type;
        }

        public override void Write(ref BsonWriter writer)
        {
            var checkpoint = writer.Written;

            var reserved = writer.Reserve(sizeof(int));

            switch (_type)
            {
                case RangeFilterType.In:
                    writer.Write_Type_Name(4, "$in"u8);

                    break;
                case RangeFilterType.NotIn:
                    writer.Write_Type_Name(4, "$nin"u8);

                    break;
            }
            

            WriteInner(ref writer);
            writer.WriteByte(0);

            reserved.Write(writer.Written - checkpoint);
        }

        private void WriteInner(ref BsonWriter writer)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            for (int i = 0; i < _values.Length; i++)
            {
                var item = _values[i];

                var typeReserved = writer.Reserve(sizeof(byte));
                writer.WriteName(i);
                writer.WriteGeneric(item, ref typeReserved);
            }

            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            reserved.Write(docLength);
            writer.Commit();
        }
    }
}
