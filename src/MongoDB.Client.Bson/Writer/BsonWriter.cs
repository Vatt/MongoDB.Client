using MongoDB.Client.Bson.Document;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace MongoDB.Client.Bson.Writer
{
    public ref partial struct BsonWriter
    {
        private const byte EndMarker = (byte)'\x00';

        private IBufferWriter<byte> _output;
        private Span<byte> _span;
#if DEBUG
        private Span<byte> _origin;
#endif
        private int _buffered;
        private int _written;
        public int Written => _written;

        public readonly ref struct Reserved
        {
            private readonly Span<byte> _reserved1;
            private readonly Span<byte> _reserved2;
            public Reserved(Span<byte> r1, Span<byte> r2)
            {
                _reserved1 = r1;
                _reserved2 = r2;
            }
            public Reserved(Span<byte> r1)
            {
                _reserved1 = r1;
                _reserved2 = null;
            }
            public void Write(Span<byte> source)
            {
                if (_reserved2 == null)
                {
                    WriteSingleSpan(source);
                }
                else
                {
                    WriteMultiSpan(source);
                }
            }
            private void WriteSingleSpan(Span<byte> source)
            {
                Debug.Assert(_reserved1.Length == source.Length);
                source.CopyTo(_reserved1);
            }
            private void WriteMultiSpan(Span<byte> source)
            {
                Debug.Assert(source.Length == _reserved1.Length + _reserved2.Length);
                source.Slice(0, _reserved1.Length).CopyTo(_reserved1);
                source.Slice(_reserved1.Length, _reserved2.Length).CopyTo(_reserved2);
            }
        }


        public Reserved Reserve(int length)
        {
            if (length > 4096)
            {
                //TODO: do something
            }
            if (_span.Length == 0)
            {
                GetNextSpan();
            }
            if (_span.Length >= length)
            {
                var reserved = new Reserved(_span.Slice(0, length));
                Advance(length);
                if (_span.Length == 0)
                {
                    GetNextSpan();
                }
                return reserved;
            }
            else
            {
                var secondLen = length - _span.Length;
                var first = _span.Slice(0, _span.Length);
                _output.Advance(first.Length);
                GetNextSpan();
                var second = _span.Slice(0, secondLen);
                Advance(secondLen);
                return new Reserved(first, second);
            }
        }


        public BsonWriter(IBufferWriter<byte> output)
        {
            _output = output;
            _span = _output.GetSpan();
#if DEBUG
            _origin = _span;
#endif
            _buffered = 0;
            _written = 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Commit()
        {
            var buffered = _buffered;
            if (buffered > 0)
            {
                _buffered = 0;
                _output.Advance(buffered);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetNextSpan()
        {
            Commit();
            _span = _output.GetSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetNextSpanWithoutCommit()
        {
            _buffered = 0;
            _span = _output.GetSpan();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            _buffered += count;
            _written += count;
            _span = _span.Slice(count);
            if (_span.IsEmpty)
            {
                GetNextSpan();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(Span<byte> source)
        {
            var slice = source;
            while (slice.Length > 0)
            {
                if (_span.Length == 0)
                {
                    GetNextSpan();
                }

                var writable = Math.Min(slice.Length, _span.Length);
                slice.Slice(0, writable).CopyTo(_span);
                slice = slice.Slice(writable);
                Advance(writable);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            _span[0] = value;
            Advance(1);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            if (BinaryPrimitives.TryWriteInt16LittleEndian(_span, value))
            {
                Advance(sizeof(short));
                return;
            }

            _span[0] = (byte)value;
            Advance(1);
            GetNextSpan();
            _span[0] = (byte)(value << 8);
            Advance(1);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(int value)
        {
            if (BinaryPrimitives.TryWriteInt32LittleEndian(_span, value))
            {
                Advance(sizeof(int));
                return;
            }

            Span<byte> buffer = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, value);

            var rem = _span.Length;
            buffer.Slice(0, rem).CopyTo(_span);
            Advance(rem);
            buffer.Slice(rem).CopyTo(_span);
            Advance(sizeof(int) - rem);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            if (BinaryPrimitives.TryWriteInt64LittleEndian(_span, value))
            {
                Advance(sizeof(long));
                return;
            }


            Span<byte> buffer = stackalloc byte[sizeof(long)];
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value);

            var rem = _span.Length;
            buffer.Slice(0, rem).CopyTo(_span);
            Advance(rem);
            buffer.Slice(rem).CopyTo(_span);
            Advance(sizeof(long) - rem);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDouble(double value)
        {
            long longValue = BitConverter.DoubleToInt64Bits(value);
            WriteInt64(longValue);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCString(ReadOnlySpan<char> value)
        {
            Commit();
            var written = Encoding.UTF8.GetBytes(value, _output);
            Advance((int)written);
            GetNextSpanWithoutCommit();
            WriteByte(EndMarker);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(ReadOnlySpan<char> value)
        {
            var len = Encoding.UTF8.GetByteCount(value);
            WriteInt32(len + 1);
            Commit();
            var written = Encoding.UTF8.GetBytes(value, _output);
            Advance((int)written);
            GetNextSpanWithoutCommit();
            WriteByte(EndMarker);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteObjectId(BsonObjectId value)
        {
            WriteInt32(value.Part1);
            WriteInt32(value.Part2);
            WriteInt32(value.Part3);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUTCDateTime(DateTimeOffset datetime)
        {
            WriteInt64(datetime.ToUnixTimeMilliseconds());
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteGuidAsBytes(Guid guid)
        {
            const int guidSize = 16;

            if (guid.TryWriteBytes(_span))
            {
                Advance(guidSize);
                return;
            }
            Span<byte> buffer = stackalloc byte[guidSize];
            guid.TryWriteBytes(buffer);

            var rem = _span.Length;
            buffer.Slice(0, rem).CopyTo(_span);
            Advance(rem);
            buffer.Slice(rem).CopyTo(_span);
            Advance(guidSize - rem);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteGuidAsString(Guid guid)
        {
            _ = guid.TryFormat(Buffer, out var written);
            WriteString(Buffer);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBoolean(bool value)
        {
            WriteByte(value ? 1 : 0);
        }



        [ThreadStatic]
        private static char[]? _buffer;
        private static Span<char> Buffer => _buffer ??= new char[32];
    }
}
