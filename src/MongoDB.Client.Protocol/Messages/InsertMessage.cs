using System.Collections.Generic;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Protocol.Common;

namespace MongoDB.Client.Protocol.Messages
{
    public class InsertMessage<T>
    {
        public InsertMessage(int requestNumber, BsonDocument document, IEnumerable<T> items)
            : this(requestNumber, Opcode.OpMsg, false, false, document, items)
        {
        }

        public InsertMessage(int requestNumber, Opcode opcode, bool moreToCome, bool exhaustAllowed, BsonDocument document, IEnumerable<T> items)
        {
            Header = new MongoMsgHeader(requestNumber, opcode);
            MoreToCome = moreToCome;
            ExhaustAllowed = exhaustAllowed;
            Document = document;
            Items = items;
        }

        public MongoMsgHeader Header { get; }
        public bool MoreToCome { get; }
        public bool ExhaustAllowed { get; }
        public BsonDocument Document { get; }
        public IEnumerable<T> Items { get; }
    }
}
