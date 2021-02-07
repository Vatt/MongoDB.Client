using MongoDB.Client.Protocol.Readers;

namespace MongoDB.Client.Messages
{
    class MongoResponseMessage
    {
        public MessageHeader Header { get; }
        public long Consumed { get; set; }

        public MongoResponseMessage(in MessageHeader header)
        {
            Header = header;
            Consumed += 16;
        }
    }
}