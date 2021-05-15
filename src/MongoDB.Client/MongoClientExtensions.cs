﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Client.Messages;

namespace MongoDB.Client
{
    public static class MongoClientExtensions
    {
        public static async ValueTask<List<T>> ToListAsync<T>(this ValueTask<CursorResult<T>> cursorTask)
        {
            var cursorResult = await cursorTask.ConfigureAwait(false);
            return cursorResult.MongoCursor.ToList();
        }

        public static async ValueTask<T?> FirstOrDefaultAsync<T>(this ValueTask<CursorResult<T>> cursorTask)
        {
            var cursorResult = await cursorTask.ConfigureAwait(false);
            var cursor = cursorResult.MongoCursor;
            var first = cursor.FirstBatch;
            var next = cursor.NextBatch;
            return first is not null ? first.FirstOrDefault() : next!.FirstOrDefault();
        }

        public static List<T> ToList<T>(this MongoCursor<T> mongoCursor)
        {
            var cursor = mongoCursor;
            var first = cursor.FirstBatch;
            var next = cursor.NextBatch;
            return first is not null ? first : next!;
        }
    }
}
