using MongoDB.Client.Bson.Document;
using MongoDB.Client.Protocol.Common;

namespace MongoDB.Client.Protocol.Messages
{
    public class QueryMessage
    {
        public QueryMessage(int requestNumber, string fullCollectionName, int numberToSkip, int batchSize, BsonDocument document)
            : this(requestNumber, fullCollectionName, Opcode.Query, numberToSkip, batchSize, false, false, true, false, false, document)
        {
        }

        public QueryMessage(int requestNumber, string fullCollectionName, BsonDocument document)
            : this(requestNumber, fullCollectionName, Opcode.Query, 0, -1, false, false, true, false, false, document)
        {
        }
        
        public QueryMessage(
            int requestNumber, 
            string fullCollectionName,
            Opcode opcode,
            int numberToSkip,
            int batchSize,
            bool noCursorTimeout,
            bool partialOk,
            bool slaveOk,
            bool tailableCursor, bool awaitData, BsonDocument document)
        {
            RequestNumber = requestNumber;
            FullCollectionName = fullCollectionName;
            Opcode = opcode;
            NoCursorTimeout = noCursorTimeout;
            PartialOk = partialOk;
            SlaveOk = slaveOk;
            TailableCursor = tailableCursor;
            AwaitData = awaitData;
            Document = document;
            NumberToSkip = numberToSkip;
            BatchSize = batchSize;
        }

        public int RequestNumber { get; }
        public string FullCollectionName { get; }
        public Opcode Opcode { get; }
        public int NumberToSkip { get; }
        public int BatchSize { get; }
        public bool NoCursorTimeout { get; }
        public bool PartialOk { get; }
        public bool SlaveOk { get; } = true;
        public bool TailableCursor { get; }
        public bool AwaitData { get; }
        public BsonDocument Document { get; }
    }
}
