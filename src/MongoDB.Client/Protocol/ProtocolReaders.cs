using MongoDB.Client.Protocol.Readers;


namespace MongoDB.Client.Protocol
{
    internal static class ProtocolReaders
    {
        public static readonly MessageHeaderReader MessageHeaderReader = new MessageHeaderReader();
        public static readonly ReplyMessageReader ReplyMessageReader = new ReplyMessageReader();
        public static readonly MsgMessageReader MsgMessageReader = new MsgMessageReader();
        public static readonly DeleteMsgType0BodyReader DeleteMsgType0BodyReader = new DeleteMsgType0BodyReader();
    }
}
