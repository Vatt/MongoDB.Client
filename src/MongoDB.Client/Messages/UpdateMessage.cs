using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Messages
{
    public class UpdateMessage
    {
        public UpdateMessage(int requestNumber, UpdateHeader updateHeader, UpdateBody updateBody)
        {
            Header = new MongoMsgHeader(requestNumber, Opcode.OpMsg);
            UpdateBody = updateBody;
            UpdateHeader = updateHeader;
        }
        public MongoMsgHeader Header { get; }
        public UpdateHeader UpdateHeader { get; }
        public UpdateBody UpdateBody { get; }
    }
}
