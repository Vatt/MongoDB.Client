using MongoDB.Client.Protocol.Writers;


namespace MongoDB.Client.Protocol
{
    public static class ProtocolWriters
    {
        public static readonly QueryMessageWriter QueryMessageWriter = new QueryMessageWriter();
        public static readonly FindMessageWriter FindMessageWriter = new FindMessageWriter();
        public static readonly GetMoreMessageWriter GetMoreMessageWriter = new GetMoreMessageWriter();
        public static readonly DeleteMessageWriter DeleteMessageWriter = new DeleteMessageWriter();
        public static readonly UpdateMessageWriter UpdateMessageWriter = new UpdateMessageWriter();
        public static readonly TransactionMessageWriter TransactionMessageWriter = new TransactionMessageWriter();
        public static readonly DropCollectionMessageWriter DropCollectionMessageWriter = new DropCollectionMessageWriter();
        public static readonly CreateCollectionMessageWriter CreateCollectionMessageWriter = new CreateCollectionMessageWriter();
    }
}
