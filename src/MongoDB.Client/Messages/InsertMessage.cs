using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Messages;
using System.Collections.Generic;

namespace MongoDB.Client.Messages
{
    public class InsertMessage<T>
    {
        public InsertMessage(int requestNumber, InsertHeader insertHeader, IEnumerable<T> items)
             : this(requestNumber, Opcode.OpMsg, false, false, insertHeader, items)
        {

        }

        public InsertMessage(int requestNumber, Opcode opcode, bool moreToCome, bool exhaustAllowed, InsertHeader insertHeader, IEnumerable<T> items)
        {
            Header = new MongoMsgHeader(requestNumber, opcode);
            MoreToCome = moreToCome;
            ExhaustAllowed = exhaustAllowed;
            Items = items;
            InsertHeader = insertHeader;
        }

        public MongoMsgHeader Header { get; }
        public bool MoreToCome { get; }
        public bool ExhaustAllowed { get; }
        public IEnumerable<T> Items { get; }
        public InsertHeader InsertHeader { get; }
    }
}
