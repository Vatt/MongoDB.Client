using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Common;

namespace MongoDB.Client.Protocol.Messages
{
    public class FindMessage
    {
        public FindMessage(int requestNumber, FindRequest document)
            : this(requestNumber, Opcode.OpMsg, false, false, PayloadType.Type0, document)
        {
        }

        public FindMessage(int requestNumber, Opcode opcode, bool moreToCome, bool exhaustAllowed, PayloadType payloadType, FindRequest document)
        {
            Header = new MongoMsgHeader(requestNumber, opcode);
            MoreToCome = moreToCome;
            ExhaustAllowed = exhaustAllowed;
            Type = payloadType;
            Document = document;
        }

        public MongoMsgHeader Header { get; }
        public bool MoreToCome { get; }
        public bool ExhaustAllowed { get; }
        public PayloadType Type { get; }
        public FindRequest Document { get; }
    }
}
