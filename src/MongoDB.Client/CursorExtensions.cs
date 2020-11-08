﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    public static class CursorExtensions
    {
        public static async ValueTask<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> cursor, CancellationToken token = default)
        {
            var list = new List<T>();
            await foreach (var item in cursor.WithCancellation(token))
            {
                list.Add(item);
            }

            return list;
        }

        public static async ValueTask<T?> FirstOrDefaultAsync<T>(this Cursor<T> cursor, CancellationToken token = default)
        {
            cursor.AddLimit(1);
            await foreach (var item in cursor.WithCancellation(token))
            {
                return item;
            }

            return default;
        }

        public static async ValueTask<T?> SingleOrDefaultAsync<T>(this Cursor<T> cursor, CancellationToken token = default)
        {
            cursor.AddLimit(2);
            T firstItem = default;
            var first = false;
            await foreach (var item in cursor.WithCancellation(token))
            {
                if (first == false)
                {
                    firstItem = item;
                }
                else
                {
                    throw new InvalidOperationException("MoreThanOneElement");
                }
            }

            return firstItem;
        }
    }
}
