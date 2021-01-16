using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Messages
{
    public class DropCollectionMessage
    {
        public DropCollectionMessage(int requestNumber, DropCollectionHeader dropCollectionHeader)
        {
            Header = new MongoMsgHeader(requestNumber, Opcode.OpMsg);
            DropCollectionHeader = dropCollectionHeader;
        }

        public MongoMsgHeader Header { get; }
        public DropCollectionHeader DropCollectionHeader { get; }
    }
}
