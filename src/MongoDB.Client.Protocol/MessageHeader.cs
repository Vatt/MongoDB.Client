namespace MongoDB.Client.Protocol
{
    public readonly struct MessageHeader
    {
        public readonly int MessageLength;
        public readonly int RequestId;
        public readonly int ResponseTo;
        public readonly int Opcode;
        public MessageHeader(int messageLength, int requestId, int responseTo, int opcode)
        {
            MessageLength = messageLength;
            RequestId = requestId;
            ResponseTo = responseTo;
            Opcode = opcode;
        }
    }
}
