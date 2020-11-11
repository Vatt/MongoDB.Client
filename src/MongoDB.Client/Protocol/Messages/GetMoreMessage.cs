using MongoDB.Client.Protocol.Common;

namespace MongoDB.Client.Protocol.Messages
{
    public class GetMoreMessage
    {
        public GetMoreMessage(int requestNumber, string fullCollectionName, long cursorId)
            : this(requestNumber, fullCollectionName, Opcode.GetMore, cursorId, 1000)
        {
        }

        public GetMoreMessage(int requestNumber, string fullCollectionName, Opcode opcode, long cursorId, int numberToReturn)
        {
            RequestNumber = requestNumber;
            FullCollectionName = fullCollectionName;
            Opcode = opcode;
            NumberToReturn = numberToReturn;
            CursorId = cursorId;
        }

        public int RequestNumber { get; }
        public string FullCollectionName { get; }
        public Opcode Opcode { get; }
        public int NumberToReturn { get; }
        public long CursorId { get; }
    }
}
