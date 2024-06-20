using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    public enum AggregateFilterType
    {
        And = 1,
        Or = 2
    }
    public class AggregateFilter : Filter
    {
        public AggregateFilterType Type { get; }
        public List<Filter> Inner { get; }
        public AggregateFilter(AggregateFilterType type)
        {
            Type = type;
            Inner = new();
        }
        public AggregateFilter(AggregateFilterType type, IEnumerable<Filter> inner)
        {
            Type = type;
            Inner = new(inner);
        }
        public void Add(params Filter[] filters)
        {
            Inner.AddRange(filters);
        }
        public void AddRange(IList<Filter> filters)
        {
            Inner.AddRange(filters);
        }
        
        public override void Write(ref BsonWriter writer)
        {
            var checkpoint = writer.Written;

            var reserved = writer.Reserve(sizeof(int));
            switch (Type)
            {
                case AggregateFilterType.And:
                    writer.Write_Type_Name(4, "$and"u8);
                    break;
                case AggregateFilterType.Or:
                    writer.Write_Type_Name(4, "$or"u8);
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
            for (int i = 0; i < Inner.Count; i++)
            {
                var item = Inner[i];

                writer.Write_Type_Name(3, i);
                item.Write(ref writer);
            }

            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            reserved.Write(docLength);
            writer.Commit();
        }
    }
}
