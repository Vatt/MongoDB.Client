using MongoDB.Client.Bson;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Filters
{
    internal abstract class LogicalFilter : Filter
    {
        protected readonly string _op;
        protected readonly List<Filter> Inner;
        public LogicalFilter(string op)
        {
            Inner = new();
            _op = op;
        }
        public void Add(params Filter[] filters)
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
            int index = 0;
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            for (int i = 0; i < Inner.Count; i++)
            {
                var item = Inner[i];

                writer.Write_Type_Name(3, i);
                item.Write(ref writer);

                index += 1;
            }

            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            reserved.Write(docLength);
            writer.Commit();
        }
    }

    internal sealed class AndFilter : LogicalFilter
    {
        public AndFilter() : base("$and")
        {
        }
    }
    internal sealed class OrFilter : LogicalFilter
    {
        public OrFilter() : base("$or")
        {
        }
    }
}
