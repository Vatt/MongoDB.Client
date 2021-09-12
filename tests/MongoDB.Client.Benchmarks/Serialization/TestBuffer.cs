using System.Buffers;

namespace MongoDB.Client.Benchmarks.Serialization
{
    public class TestBuffer : IBufferWriter<byte>
    {
        private readonly byte[] _buffer;
        private int _position;

        public int Written => _position;

        public TestBuffer(int size = 4096)
        {
            _buffer = new byte[size];
        }

        public TestBuffer(byte[] buffer)
        {
            _buffer = buffer;
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

        public byte[] ToArray()
        {
            return _buffer.AsSpan(0, _position).ToArray();
        }
    }
}