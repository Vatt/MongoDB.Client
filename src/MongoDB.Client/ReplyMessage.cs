using System;
using System.Buffers;

namespace MongoDB.Client
{
    public class ReplyMessage : IDisposable
    {
        public int RequestId { get; set; }
        public int ResponseTo { get; set; }
        public bool AwaitCapable { get; set; }
        public long CursorId { get; set; }
        public bool CursorNotFound { get; set; }
        public int NumberReturned { get; set; }
        public bool QueryFailure { get; set; }
        public int StartingFrom { get; set; }

        public IMemoryOwner<byte>? Payload { get; set; }
        public int PayloadSize { get; set; }

        public ReadOnlyMemory<byte> Message => Payload?.Memory.Slice(0, PayloadSize) ?? default;

        public void Dispose()
        {
            Payload?.Dispose();
        }
    }
}
