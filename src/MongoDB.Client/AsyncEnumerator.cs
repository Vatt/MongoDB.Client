using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Utils;

namespace MongoDB.Client
{
    public class AsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        public AsyncEnumerator(Cursor<T> cursor, CancellationToken token)
        {
            _cursor = cursor;
            _token = token;
            _items = default!;
            Current = default!;
            _idx = default;
        }

        public T Current { get; private set; }

        private readonly Cursor<T> _cursor;
        private readonly CancellationToken _token;
        private List<T> _items;
        private int _idx;

        public ValueTask DisposeAsync()
        {
            if (_items is not null)
            {
                ListsPool<T>.Pool.Return(_items);
            }
            return default;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            if (_items is not null && _idx < _items.Count)
            {
                Current = _items[_idx];
                _idx++;
                return new ValueTask<bool>(true);
            }

            return NextAsync();
        }

        private async ValueTask<bool> NextAsync()
        {
            if (_items is not null)
            {
                ListsPool<T>.Pool.Return(_items);
                _items = null!;
            }
            if (_cursor.HasNext == false)
            {
                return false;
            }

            _items = await _cursor.GetNextBatchAsync(_token);
            _idx = 1;
            if (_items.Count > 0)
            {
                Current = _items[0];
                return true;
            }
            return false;
        }
    }
}
