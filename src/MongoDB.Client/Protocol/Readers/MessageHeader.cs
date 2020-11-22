using System;
using System.Runtime.InteropServices;
using MongoDB.Client.Protocol.Common;

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

        public override string ToString()
        {
            return
                $"MessageLength '{MessageLength}', RequestId '{RequestId}', ResponseTo '{ResponseTo}', Opcode '{Opcode}'";
        }
    }
}
