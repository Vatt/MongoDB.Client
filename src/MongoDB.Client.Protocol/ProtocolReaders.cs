using MongoDB.Client.Protocol.Readers;


namespace MongoDB.Client.Protocol
{
    public static class ProtocolReaders
    {
        public static readonly MessageHeaderReader MessageHeaderReader = new MessageHeaderReader();
        public static readonly ReplyMessageReader ReplyMessageReader = new ReplyMessageReader();
        public static readonly MsgMessageReader MsgMessageReader = new MsgMessageReader();
    }
}
