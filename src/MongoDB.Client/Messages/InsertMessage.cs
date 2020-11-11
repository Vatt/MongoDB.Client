using System.Collections.Generic;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Common;

namespace MongoDB.Client.Protocol.Messages
{
    public class InsertMessage<T>
    {
        public InsertMessage(int requestNumber, BsonDocument document, IEnumerable<T> items)
            : this(requestNumber, Opcode.OpMsg, false, false,null, document, items)
        {
        }

        public InsertMessage(int requestNumber, InsertHeader insertHeader, IEnumerable<T> items)
             : this(requestNumber, Opcode.OpMsg, false, false, insertHeader, null, items)
        {

        }

        public InsertMessage(int requestNumber, BsonDocument document, InsertHeader insertHeader, IEnumerable<T> items)
            : this(requestNumber, Opcode.OpMsg, false, false, insertHeader, document, items)
        {
        }

        public InsertMessage(int requestNumber, Opcode opcode, bool moreToCome, bool exhaustAllowed, InsertHeader insertHeader, BsonDocument document, IEnumerable<T> items)
        {
            Header = new MongoMsgHeader(requestNumber, opcode);
            MoreToCome = moreToCome;
            ExhaustAllowed = exhaustAllowed;
            Document = document;
            Items = items;
            InsertHeader = insertHeader;
        }

        public MongoMsgHeader Header { get; }
        public bool MoreToCome { get; }
        public bool ExhaustAllowed { get; }
        public BsonDocument Document { get; }
        public IEnumerable<T> Items { get; }
        public int RequestNumber { get; }
        public InsertHeader InsertHeader { get; }
    }
}
