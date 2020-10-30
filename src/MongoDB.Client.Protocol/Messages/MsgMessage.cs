using MongoDB.Client.Bson.Document;
using MongoDB.Client.Protocol.Common;

namespace MongoDB.Client
{
    public class MsgMessage
    {
        public MsgMessage(int requestNumber, string database, BsonDocument document)
            : this(requestNumber, database, Opcode.OpMsg, false, false, PayloadType.Type0, document)
        {
        }

        public MsgMessage(int requestNumber, string database, Opcode opcode, bool moreToCome, bool exhaustAllowed, PayloadType payloadType, BsonDocument document)
        {
            RequestNumber = requestNumber;
            Database = database;
            Opcode = opcode;
            MoreToCome = moreToCome;
            ExhaustAllowed = exhaustAllowed;
            Type = payloadType;
            Document = document;
        }

        public int RequestNumber { get; }
        public string Database { get; }
        public Opcode Opcode { get; }
        public bool MoreToCome { get; }
        public bool ExhaustAllowed { get; }
        public PayloadType Type { get;}
        public BsonDocument Document { get; }


        public enum PayloadType
        {
            Type0 = 0,
            Type1 = 1
        }
    }
}
