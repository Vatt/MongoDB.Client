using MongoDB.Client.Bson.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MongoDB.Client.Bson.Document
{
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    public class BsonDocument : IEnumerable<BsonElement>
    {

        private readonly List<BsonElement> _elements;

        public BsonDocument()
        {
            _elements = new List<BsonElement>();
        }

        public BsonDocument(string name, string value)
            : this()
        {
            Add(name, value);
        }


        public void Add(BsonElement element)
        {
            _elements.Add(element);
        }


        public void Add(string name, int value)
        {
            _elements.Add(BsonElement.Create(this, name, value));
        }

        public void Add(string name, long value)
        {
            _elements.Add(BsonElement.Create(this, name, value));
        }

        public void Add(string name, long? value)
        {
            if (value.HasValue)
            {
                _elements.Add(BsonElement.Create(this, name, value.Value));
            }
            else
            {
                _elements.Add(BsonElement.Create(this, name));
            }
        }

        public void Add(string name, long? value, bool condition)
        {
            if (condition)
            {
                Add(name, value);
            }
        }

        public void Add(string name, bool value)
        {
            _elements.Add(BsonElement.Create(this, name, value));
        }


        public void Add(string name, BsonDocument? value)
        {
            _elements.Add(BsonElement.Create(this, name, value));
        }

        public void Add(string name, BsonDocument? value, bool condition)
        {
            if (condition)
            {
                _elements.Add(BsonElement.Create(this, name, value));
            }
        }

        public void Add(string name, BsonArray? value)
        {
            _elements.Add(BsonElement.CreateArray(this, name, value));
        }

        public void Add(string name, BsonArray? value, bool condition)
        {
            if (condition)
            {
                _elements.Add(BsonElement.CreateArray(this, name, value));
            }
        }

        public void Add(string name, string? value)
        {
            _elements.Add(BsonElement.Create(this, name, value));
        }

        public void Add(string name, string? value, bool condition)
        {
            if (condition)
            {
                _elements.Add(BsonElement.Create(this, name, value));
            }
        }


        public BsonElement this[int idx] => _elements[idx];
        public BsonElement this[string name] => _elements.First(e => e.Name.Equals(name, StringComparison.Ordinal));

        public int Count => _elements.Count;

        public IEnumerator<BsonElement> GetEnumerator() => _elements.GetEnumerator();

        public override string ToString()
        {
            return string.Join(';', _elements.Select(e => e.ToString()));
        }

        IEnumerator IEnumerable.GetEnumerator() => _elements.GetEnumerator();
    }
}
