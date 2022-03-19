using System.Diagnostics;
using MongoDB.Client.Bson.Utils;

namespace MongoDB.Client.Bson.Document
{
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    public class BsonArray : BsonDocument
    {
        private int counter;

        public void Add(string? value)
        {
            base.Add(counter.ToString(), value);
            counter++;
        }

        public void Add(int value)
        {
            base.Add(counter.ToString(), value);
            counter++;
        }


        public void Add(bool value)
        {
            base.Add(counter.ToString(), value);
            counter++;
        }


        public void Add(BsonDocument? value)
        {
            base.Add(counter.ToString(), value);
            counter++;
        }


        public void Add(BsonArray? value)
        {
            base.Add(counter.ToString(), value);
            counter++;
        }
    }
}
