using MongoDB.Client.Bson.Document;
using MongoDB.Client.Protocol.Common;

namespace MongoDB.Client
{
    public class QueryMessage
    {
        public QueryMessage(int requestNumber, string database, Opcode opcode, BsonDocument document)
            : this(requestNumber, database, opcode, false, false, true, false, false, document)
        {
        }

        public QueryMessage(
            int requestNumber, 
            string database,
            Opcode opcode,
            bool noCursorTimeout,
            bool partialOk,
            bool slaveOk,
            bool tailableCursor,
            bool awaitData,
            BsonDocument document)
        {
            RequestNumber = requestNumber;
            Database = database;
            Opcode = opcode;
            NoCursorTimeout = noCursorTimeout;
            PartialOk = partialOk;
            SlaveOk = slaveOk;
            TailableCursor = tailableCursor;
            AwaitData = awaitData;
            Document = document;
        }

        public int RequestNumber { get; }
        public string Database { get; }
        public Opcode Opcode { get; }
        public bool NoCursorTimeout { get; }
        public bool PartialOk { get; }
        public bool SlaveOk { get; } = true;
        public bool TailableCursor { get; }
        public bool AwaitData { get; }
        public BsonDocument Document { get; }
    }
}
