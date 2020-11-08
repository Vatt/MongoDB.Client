using MongoDB.Client.Protocol.Common;

namespace MongoDB.Client.Protocol.Messages
{
    public readonly struct MongoMsgHeader
    {
        public MongoMsgHeader(int requestNumber, Opcode opcode)
        {
            RequestNumber = requestNumber;
            Opcode = opcode;
        }

        public int RequestNumber { get; }
        public Opcode Opcode { get; }
    }
}