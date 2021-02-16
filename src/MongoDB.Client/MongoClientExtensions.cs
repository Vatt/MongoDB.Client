using MongoDB.Client.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            return cursorResult.MongoCursor.Items.FirstOrDefault();
        }

        public static List<T> ToList<T>(this MongoCursor<T> mongoCursor)
        {
            return mongoCursor.Items;
        }
    }
}
