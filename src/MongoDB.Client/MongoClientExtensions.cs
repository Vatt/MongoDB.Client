using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;

namespace MongoDB.Client
{
    public static class MongoClientExtensions
    {
        public static async ValueTask<List<T>> ToListAsync<T>(this ValueTask<CursorResult<T>> cursorTask) where T : IBsonSerializer<T>
        {
            var cursorResult = await cursorTask.ConfigureAwait(false);
            return cursorResult.MongoCursor.ToList();
        }

        public static async ValueTask<T?> FirstOrDefaultAsync<T>(this ValueTask<CursorResult<T>> cursorTask) where T : IBsonSerializer<T>
        {
            var cursorResult = await cursorTask.ConfigureAwait(false);
            var cursor = cursorResult.MongoCursor;
            var first = cursor.FirstBatch;
            var next = cursor.NextBatch;
            return first is not null ? first.FirstOrDefault() : next!.FirstOrDefault();
        }

        public static List<T> ToList<T>(this MongoCursor<T> mongoCursor) where T : IBsonSerializer<T>
        {
            var cursor = mongoCursor;
            var first = cursor.FirstBatch;
            var next = cursor.NextBatch;
            return first is not null ? first : next!;
        }
    }
}
