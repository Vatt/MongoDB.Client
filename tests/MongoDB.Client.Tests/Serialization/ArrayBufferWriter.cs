using System.Buffers;

namespace MongoDB.Client.Tests.Serialization
{
    public class ArrayBufferWriter : IBufferWriter<byte>
    {
        private byte[] _buffer;
        private int _position;

        public ArrayBufferWriter(int bufferSize = 1024 * 1024)
        {
            _buffer = new byte[bufferSize];
        }

        public void Reset()
        {
            _position = 0;
        }

        public void Advance(int count)
        {
            _position += count;
        }
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            return _buffer.AsMemory(_position);
        }
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return _buffer.AsSpan(_position);
        }

        public ReadOnlyMemory<byte> WrittenMemory => _buffer.AsMemory(0, _position);
    }
}
