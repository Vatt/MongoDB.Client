using System.Collections.Generic;

namespace MongoDB.Client.Messages
{
    public class CursorResult<T> : IParserResult
    {
        public MongoCursor<T> MongoCursor { get; }

        public CursorResult(MongoCursor<T> mongoCursor)
        {
            MongoCursor = mongoCursor;
        }

        public double Ok { get; set; }

        public string? ErrorMessage { get; set; }
        public int Code { get; set; }
        public string? CodeName { get; set; }
    }

    public class MongoCursor<T>
    {
        public long Id { get; set; }

        public string? Namespace { get; set; }

        public List<T> Items { get; }

        public MongoCursor(List<T> items)
        {
            Items = items;
        }
    }
}
