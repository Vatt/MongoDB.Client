using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Messages
{
    public class DeleteMessage
    {
        public DeleteMessage(int requestNumber, DeleteHeader deleteHeader, DeleteBody deleteBody)
        {
            Header = new MongoMsgHeader(requestNumber, Opcode.OpMsg);
            DeleteHeader = deleteHeader;
            DeleteBody = deleteBody;
        }

        public MongoMsgHeader Header { get; }
        public DeleteHeader DeleteHeader { get; }
        public DeleteBody DeleteBody { get; }
    }
}
