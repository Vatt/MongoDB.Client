using Microsoft.Extensions.ObjectPool;

namespace MongoDB.Client.Utils
{
    public static class ListsPool<TPool>
    {
        public static readonly ObjectPool<List<TPool>> Pool =
            new DefaultObjectPool<List<TPool>>(new PooledListPolicy<TPool>());

        private sealed class PooledListPolicy<T> : IPooledObjectPolicy<List<T>>
        {
            public List<T> Create()
            {
                return new List<T>();
            }

            public bool Return(List<T> obj)
            {
                obj.Clear();
                return true;
            }
        }
    }
}