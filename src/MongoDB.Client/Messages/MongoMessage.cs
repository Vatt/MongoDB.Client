using MongoDB.Client.Protocol.Readers;

namespace MongoDB.Client.Messages
{
    class MongoMessage
    {
        public MessageHeader Header { get; }

        public MongoMessage(in MessageHeader header)
        {
            Header = header;
        }
    }
}
