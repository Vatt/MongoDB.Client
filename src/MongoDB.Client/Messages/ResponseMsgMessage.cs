using MongoDB.Client.Protocol.Readers;

namespace MongoDB.Client.Messages
{
    class ResponseMsgMessage : MongoResponseMessage
    {
        public MsgMessageHeader MsgHeader { get; }

        public ResponseMsgMessage(in MessageHeader header, in MsgMessageHeader replyHeader) : base(header)
        {
            MsgHeader = replyHeader;
            Consumed += 5;
        }
    }
}
