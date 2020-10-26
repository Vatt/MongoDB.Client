using MongoDB.Client.Bson.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MongoDB.Client.Bson.Document
{
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    public class BsonDocument : IEnumerable<BsonElement>
    {

        public readonly List<BsonElement> Elements;

        public BsonDocument()
        {
            Elements = new List<BsonElement>();
        }

        public void Add(BsonElement element)
        {
            Elements.Add(element);
        }

        public void Add(string name, int value)
        {
            Elements.Add(BsonElement.Create(this, name, value));
        }

        public void Add(string name, bool value)
        {
            Elements.Add(BsonElement.Create(this, name, value));
        }

        public void Add(string name, string value)
        {
            Elements.Add(BsonElement.Create(this, name, value));
        }

        public IEnumerator<BsonElement> GetEnumerator() => Elements.GetEnumerator();

        public override string ToString()
        {
            return string.Join(';', Elements.Select(e => e.ToString()));
        }

        IEnumerator IEnumerable.GetEnumerator() => Elements.GetEnumerator();
    }
}
