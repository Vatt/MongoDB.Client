namespace MongoDB.Client.Protocol.Readers
{
    public readonly struct MessageHeader
    {
        public readonly int MessageLength;
        public readonly int RequestId;
        public readonly int ResponseTo;
        public readonly Opcode Opcode;
        public MessageHeader(int messageLength, int requestId, int responseTo, int opcode)
        {
            MessageLength = messageLength;
            RequestId = requestId;
            ResponseTo = responseTo;
            Opcode = (Opcode)opcode;
        }
    }
}
