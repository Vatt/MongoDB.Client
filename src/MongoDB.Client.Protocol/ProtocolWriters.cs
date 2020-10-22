using MongoDB.Client.Protocol.Writers;


namespace MongoDB.Client.Protocol
{
    public static class ProtocolWriters
    {
        public static readonly ReadOnlyMemoryWriter ReadOnlyMemoryWriter = new ReadOnlyMemoryWriter();
    }
}
