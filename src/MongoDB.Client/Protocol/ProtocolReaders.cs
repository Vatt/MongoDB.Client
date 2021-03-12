using MongoDB.Client.Protocol.Readers;


namespace MongoDB.Client.Protocol
{
    internal static class ProtocolReaders
    {
        public static readonly MessageHeaderReader MessageHeaderReader = new MessageHeaderReader();
        public static readonly ReplyMessageReader ReplyMessageReader = new ReplyMessageReader();
        public static readonly MsgMessageReader MsgMessageReader = new MsgMessageReader();
        public static readonly DeleteMsgType0BodyReader DeleteMsgType0BodyReader = new DeleteMsgType0BodyReader();
        public static readonly TransactionBodyReader TransactionBodyReader = new TransactionBodyReader();
        public static readonly DropCollectionBodyReader DropCollectionBodyReader = new DropCollectionBodyReader();
        public static readonly CreateCollectionBodyReader CreateCollectionBodyReader = new CreateCollectionBodyReader();
    }
}
