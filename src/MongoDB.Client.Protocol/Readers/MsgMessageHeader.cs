using System;

namespace MongoDB.Client.Protocol.Readers
{
    public readonly struct MsgMessageHeader
    {
        public MsgMessageHeader(int msgFlags, byte payloadType)
        {
            _msgFlags = (OpMsgFlags)msgFlags;
            PayloadType = payloadType;
        }

        private readonly OpMsgFlags _msgFlags;

        public bool MoreToCome => (_msgFlags & OpMsgFlags.MoreToCome) != 0;
        public bool ExhaustAllowed => (_msgFlags & OpMsgFlags.ExhaustAllowed) != 0;
        public bool ChecksumPresent => (_msgFlags & OpMsgFlags.ChecksumPresent) != 0;

        public byte PayloadType { get; }

        private void EnsureFlagsAreValid(OpMsgFlags flags)
        {
            var invalidFlags = ~OpMsgFlags.All;
            if ((flags & invalidFlags) != 0)
            {
                throw new FormatException("Command message has invalid flags.");
            }
            if ((flags & OpMsgFlags.ChecksumPresent) != 0)
            {
                throw new FormatException("Command message CheckSumPresent flag not supported.");
            }
        }

        [Flags]
        private enum OpMsgFlags
        {
            ChecksumPresent = 1 << 0,
            MoreToCome = 1 << 1,
            ExhaustAllowed = 1 << 16,
            All = ChecksumPresent | MoreToCome | ExhaustAllowed
        }
    }
}
