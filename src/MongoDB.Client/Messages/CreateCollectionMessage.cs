using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Messages
{
    public class CreateCollectionMessage
    {
        public CreateCollectionMessage(int requestNumber, CreateCollectionHeader dropCollectionHeader)
        {
            Header = new MongoMsgHeader(requestNumber, Opcode.OpMsg);
            CreateCollectionHeader = dropCollectionHeader;
        }

        public MongoMsgHeader Header { get; }
        public CreateCollectionHeader CreateCollectionHeader { get; }
    }
}
