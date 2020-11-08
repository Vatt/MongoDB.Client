using System;

namespace MongoDB.Client.Protocol.Writers
{
    [Flags]
    internal enum OpMsgFlags
    {
        ChecksumPresent = 1 << 0,
        MoreToCome = 1 << 1,
        ExhaustAllowed = 1 << 16,
        All = ChecksumPresent | MoreToCome | ExhaustAllowed
    }
}