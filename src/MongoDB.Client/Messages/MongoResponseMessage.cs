using MongoDB.Client.Protocol.Readers;

namespace MongoDB.Client.Messages
{
    class MongoResponseMessage
    {
        public MessageHeader Header { get; }

#if DEBUG
        public long Consumed { get; set; }
#endif

        public MongoResponseMessage(in MessageHeader header)
        {
            Header = header;
#if DEBUG
            Consumed += 16;
#endif
        }
    }
}