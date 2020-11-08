using System.Collections.Generic;
using System.Threading;

namespace MongoDB.Client.Messages
{
    public class CursorResult<T> : IParserResult
    {
        public MongoCursor<T> MongoCursor { get; set; }

        public double Ok { get; set; }

        public string? ErrorMessage { get; set; }
        public int Code { get; set; }
        public string? CodeName { get; set; }
    }

    public class MongoCursor<T> : IAsyncEnumerable<T>
    {
        public long Id { get; set; }

        public string Namespace { get; set; }

        public List<T> Items { get; set; }
        
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
