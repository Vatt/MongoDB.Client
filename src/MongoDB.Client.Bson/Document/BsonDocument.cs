using MongoDB.Client.Bson.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MongoDB.Client.Bson.Document
{
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    public class BsonDocument : IEnumerable<BsonElement>, IEquatable<BsonDocument>
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

        public BsonDocument(string name, int value)
            : this()
        {
            Add(name, value);
        }

        public BsonDocument(string name, BsonBinaryData value)
            : this()
        {
            Add(name, value);
        }

        public BsonDocument(string name, BsonObjectId value)
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

        public void Add(string name, int? value)
        {
            if (value.HasValue)
            {
                Add(name, value.Value);
            }
            else
            {
                _elements.Add(BsonElement.Create(this, name));
            }
        }

        public void Add(string name, int? value, bool condition)
        {
            if (condition)
            {
                Add(name, value);
            }
        }

        public void Add(string name, Func<int> valueFactory, bool condition)
        {
            if (condition)
            {
                Add(name, valueFactory());
            }
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

        public void Add(string name, bool? value)
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

        public void Add(string name, bool? value, bool condition)
        {
            if (condition)
            {
                Add(name, value);
            }
        }

        public void Add(string name, Func<bool> valueFactory, bool condition)
        {
            if (condition)
            {
                Add(name, valueFactory());
            }
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

        public void Add(string name, Func<BsonDocument> valueFactory, bool condition)
        {
            if (condition)
            {
                Add(name, valueFactory());
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

        public void Add(string name, BsonBinaryData value)
        {
            _elements.Add(BsonElement.Create(this, name, value));
        }

        public void Add(string name, BsonObjectId value)
        {
            _elements.Add(BsonElement.Create(this, name, value));
        }


        public BsonElement this[int idx] => _elements[idx];
        public BsonElement this[string name] => _elements.First(e => e.Name.Equals(name, StringComparison.Ordinal));

        public int Count => _elements.Count;

        public IEnumerator<BsonElement> GetEnumerator() => _elements.GetEnumerator();

        public override string ToString()
        {
            return "{" + string.Join(',', _elements.Select(e => e.ToString())) + "}";
        }

        IEnumerator IEnumerable.GetEnumerator() => _elements.GetEnumerator();


        public override bool Equals(object? obj)
        {
            return obj is BsonDocument document && Equals(document);
        }

        public bool Equals(BsonDocument? obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(obj, this))
            {
                return true;
            }
            if (obj.Count != Count)
            {
                return false;
            }

            for (int i = 0; i < _elements.Count; i++)
            {
                if (this[i].Equals(obj[i]) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var element in _elements)
            {
                hash.Add(element);
            }

            return hash.ToHashCode();
        }
    }
}
