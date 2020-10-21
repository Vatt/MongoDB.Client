using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MongoDB.Client.Bson.Utils;

namespace MongoDB.Client.Bson.Document
{
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    public class BsonDocument
    {

        public readonly List<BsonElement> Elements;
       
        public BsonDocument()
        {
            Elements = new List<BsonElement>();
        }


        public override string ToString()
        {
            return string.Join(';', Elements.Select(e => e.ToString()));
        }
    }
}
