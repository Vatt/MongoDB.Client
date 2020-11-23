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
            public void Write(byte source)
            {
                Debug.Assert(_reserved2 == null);
                _reserved1[0] = source;
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
        public void WriteBytes(ReadOnlySpan<byte> source)
        {
            if (source.TryCopyTo(_span))
            {
                Advance(source.Length);
                return;
            }
            
            SlowWriteBytes(source);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SlowWriteBytes(ReadOnlySpan<byte> source)
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
        public void WriteInt32(int value)
        {
            if (BinaryPrimitives.TryWriteInt32LittleEndian(_span, value))
            {
                Advance(sizeof(int));
                return;
            }

            SlowWriteInt32(value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void SlowWriteInt32(int value)
        {
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

            SlowWriteInt64(value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void SlowWriteInt64(long value)
        {
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
            var count = Encoding.UTF8.GetByteCount(value);
            if (count <= _span.Length)
            {
                var written = Encoding.UTF8.GetBytes(value, _span);
                Advance(written);
                WriteByte(EndMarker);
                return;
            }

            SlowWriteCString(value);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void SlowWriteCString(ReadOnlySpan<char> chars)
        {
            var encoder = Encoding.UTF8.GetEncoder();
            do
            {
                encoder.Convert(chars, _span, true, out var charsUsedJustNow, out var bytesWrittenJustNow, out _);

                chars = chars.Slice(charsUsedJustNow);
                Advance(bytesWrittenJustNow);
            } while (!chars.IsEmpty);
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(ReadOnlySpan<char> value)
        {
            var count = Encoding.UTF8.GetByteCount(value);
            WriteInt32(count + 1);
            if (count <= _span.Length)
            {
                var written = Encoding.UTF8.GetBytes(value, _span);
                Advance(written);
                WriteByte(EndMarker);
                return;
            }

            SlowWriteString(value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void SlowWriteString(ReadOnlySpan<char> value)
        {
            var encoder = Encoding.UTF8.GetEncoder();
            do
            {
                encoder.Convert(value, _span, true, out var charsUsedJustNow, out var bytesWrittenJustNow, out _);

                value = value.Slice(charsUsedJustNow);
                Advance(bytesWrittenJustNow);
            } while (!value.IsEmpty);
            WriteByte(EndMarker);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteObjectId(in BsonObjectId value)
        {
            const int oidSize = 12;
            if (value.TryWriteBytes(_span))
            {
                Advance(oidSize);
                return;
            }
            SlowWriteObjectId(value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SlowWriteObjectId(in BsonObjectId value)
        {
            const int oidSize = 12;
            Span<byte> buffer = stackalloc byte[oidSize];
            value.TryWriteBytes(buffer);

            var rem = _span.Length;
            buffer.Slice(0, rem).CopyTo(_span);
            Advance(rem);
            buffer.Slice(rem).CopyTo(_span);
            Advance(oidSize - rem);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtcDateTime(in DateTimeOffset datetime)
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
            SlowWriteGuidAsBytes(guid);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void SlowWriteGuidAsBytes(Guid guid)
        {
            const int guidSize = 16;

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
            var buffer = ArrayPool<char>.Shared.Rent(32);
            try
            {
                _ = guid.TryFormat(buffer, out var written);
                WriteString(buffer.AsSpan(0,32));
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBoolean(bool value)
        {
            WriteByte(value ? 1 : 0);
        }
    }
}
