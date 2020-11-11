using System;

namespace MongoDB.Client.Protocol.Readers
{
    public readonly struct ReplyMessageHeader
    {
        public ReplyMessageHeader(int responseFlags, long cursorId, int startingFrom, int numberReturned)
        {
            _responseFlags = (ResponseFlags)responseFlags;
            CursorId = cursorId;
            StartingFrom = startingFrom;
            NumberReturned = numberReturned;
        }

        private readonly ResponseFlags _responseFlags;
        public long CursorId { get; }
        public int StartingFrom { get; }
        public int NumberReturned { get; }

        public bool AwaitCapable => (_responseFlags & ResponseFlags.AwaitCapable) == ResponseFlags.AwaitCapable;
        public bool CursorNotFound => (_responseFlags & ResponseFlags.CursorNotFound) == ResponseFlags.CursorNotFound;
        public bool QueryFailure => (_responseFlags & ResponseFlags.QueryFailure) == ResponseFlags.QueryFailure;

        [Flags]
        private enum ResponseFlags
        {
            None = 0,
            CursorNotFound = 1,
            QueryFailure = 2,
            AwaitCapable = 8
        }
    }
}
