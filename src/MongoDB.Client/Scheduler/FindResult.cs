using MongoDB.Client.Messages;

namespace MongoDB.Client.Scheduler
{
    internal readonly struct FindResult<T>
    {
        public FindResult(CursorResult<T> cursorResult, MongoScheduler scheduler)
        {
            CursorResult = cursorResult;
            Scheduler = scheduler;
        }

        public CursorResult<T> CursorResult { get;}
        public MongoScheduler Scheduler { get; }
    }
}
