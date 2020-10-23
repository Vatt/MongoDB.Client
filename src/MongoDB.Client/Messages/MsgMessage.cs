using MongoDB.Client.Protocol.Readers;

namespace MongoDB.Client.Messages
{
    class MsgMessage : MongoMessage
    {
        public MsgMessageHeader MsgHeader { get; }

        public MsgMessage(in MessageHeader header, in MsgMessageHeader replyHeader) : base(header)
        {
            MsgHeader = replyHeader;
        }
    }
}
