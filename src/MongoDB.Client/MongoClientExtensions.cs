using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Client.Messages;

namespace MongoDB.Client
{
    public static class MongoClientExtensions
    {
        public static async ValueTask<List<T>> ToListAsync<T>(this ValueTask<CursorResult<T>> cursorTask)
        {
            var cursorResult = await cursorTask.ConfigureAwait(false);
            return cursorResult.Cursor.ToList();
        }

        public static List<T> ToList<T>(this Cursor<T> cursor)
        {
            return cursor.Items;
        }
    }
}
