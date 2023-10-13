using MongoDB.Client.Bson;
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
        public string PropertyName { get; protected set; }
        public T[] Values { get; protected set; }
        public RangeFilterType Type { get; protected set; }
        public RangeFilter(string propertyName, T[] values, RangeFilterType type)
        {
            PropertyName = propertyName;
            Values = values;
            Type = type;
        }

        public override void Write(ref BsonWriter writer)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(sizeof(int));
            writer.WriteBsonType(BsonType.Document);
            writer.WriteName(PropertyName);
            var checkpoint1 = writer.Written;
            var reserved1 = writer.Reserve(sizeof(int));

            switch (Type)
            {
                case RangeFilterType.In:
                    writer.Write_Type_Name(4, "$in"u8);

                    break;
                case RangeFilterType.NotIn:
                    writer.Write_Type_Name(4, "$nin"u8);

                    break;
            }
            

            WriteInner(ref writer);
            writer.WriteByte((byte)'\x00');
            reserved1.Write(writer.Written - checkpoint1);
            writer.WriteByte(0);

            reserved.Write(writer.Written - checkpoint);
        }

        private void WriteInner(ref BsonWriter writer)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            for (int i = 0; i < Values.Length; i++)
            {
                var item = Values[i];

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
