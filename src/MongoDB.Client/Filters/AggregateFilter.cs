using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    internal abstract class AggregateFilter : Filter
    {
        protected readonly string _op;
        protected readonly List<Filter> Inner;
        public AggregateFilter(string op)
        {
            _op = op;
            Inner = new();
        }
        public void Add(params Filter[] filters)
        {
            Inner.AddRange(filters);
        }
        public void AddRange(List<Filter> filters)
        {
            Inner.AddRange(filters);
        }
        public override void Write(ref BsonWriter writer)
        {
            var checkpoint = writer.Written;

            var reserved = writer.Reserve(sizeof(int));
            writer.Write_Type_Name(4, _op);
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

    internal sealed class AndFilter : AggregateFilter
    {
        public AndFilter() : base("$and")
        {
        }
    }
    internal sealed class OrFilter : AggregateFilter
    {
        public OrFilter() : base("$or")
        {
        }
    }
}
