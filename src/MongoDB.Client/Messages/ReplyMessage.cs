using MongoDB.Client.Protocol.Readers;

namespace MongoDB.Client.Messages
{
    class ReplyMessage : MongoResponseMessage
    {
        public ReplyMessageHeader ReplyHeader { get; }

        public ReplyMessage(in MessageHeader header, in ReplyMessageHeader replyHeader) : base(header)
        {
            ReplyHeader = replyHeader;
#if DEBUG
            Consumed += 20;
#endif
        }
    }
}
