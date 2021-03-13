using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Messages
{
    public class TransactionMessage
    {
        public TransactionMessage(int requestNumber, TransactionRequest request)
        {
            Header = new MongoMsgHeader(requestNumber, Opcode.OpMsg);
            Request = request;
        }

        public MongoMsgHeader Header { get; }
        public TransactionRequest Request { get; }
    }
}
