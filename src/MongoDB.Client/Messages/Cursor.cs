using System.Collections.Generic;

namespace MongoDB.Client.Messages
{
    public class CursorResult<T>
    {
        public Cursor<T> Cursor { get; set; }

        public double Ok { get; set; }
    }

    public class Cursor<T>
    {
        public long Id { get; set; }

        public string Namespace { get; set; }

        public List<T> Items { get; set; }
    }
}
