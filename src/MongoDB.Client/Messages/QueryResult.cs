using System;
using System.Collections;
using System.Collections.Generic;

namespace MongoDB.Client.Messages
{
    public class QueryResult<T> : IReadOnlyList<T>, IParserResult
    {
        public long CursorId { get; }
        private (T, T) _items;
        private IList<T>? _other;

        // Static lists to store real length (-1 field in struct)
        private static readonly IList<T> LengthIs1 = new List<T> {default};
        private static readonly IList<T> LengthIs2 = new List<T> {default, default};

        public QueryResult(long cursorId)
        {
            CursorId = cursorId;
        }

        public const int Capacity = 2;
        
        public void Add(T item)
        {
            if (_other == null)
            {
                _items.Item1 = item;
                _other = LengthIs1;
            }
            else if (ReferenceEquals(_other, LengthIs1))
            {
                _items.Item2 = item;
                _other = LengthIs2;
            }
            else
            {
                if (ReferenceEquals(_other, LengthIs2))
                {
                    _other = new List<T>(8);
                    _other.Add(_items.Item1);
                    _items.Item1 = default;

                    _other.Add(_items.Item2);
                    _items.Item2 = default;
                }

                _other.Add(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_other == null) yield break;
            if (_other.Count <= Capacity)
            {
                yield return _items.Item1;
                if (ReferenceEquals(_other, LengthIs2)) yield return _items.Item2;
                yield break;
            }

            using var enumerator = _other.GetEnumerator();
            while (enumerator.MoveNext()) yield return enumerator.Current;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _other?.Count ?? 0;

        public T this[int index]
        {
            get
            {
                if (_other == null || index >= Count || index < 0)
                    throw new IndexOutOfRangeException();

                if (_other?.Count > Capacity) return _other[index];
                if (_other.Count > 0 && index == 0) return _items.Item1;
                if (_other.Count > 1 && index == 1) return _items.Item2;

                throw new InvalidOperationException("Uncovered branch");
            }
            set
            {
                if (_other == null || index >= Count || index < 0)
                    throw new IndexOutOfRangeException();

                if (_other.Count > Capacity) _other[index] = value;
                if (_other.Count > 0 && index == 0) _items.Item1 = value;
                if (_other.Count > 1 && index == 1) _items.Item2 = value;

                throw new InvalidOperationException("Uncovered branch");
            }
        }
    }
}