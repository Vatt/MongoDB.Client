using MongoDB.Client.Bson.Document;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Text;

namespace MongoDB.Client.Bson.Writer
{
    public ref struct MongoDBBsonWriter
    {
        private IBufferWriter<byte> _output;
        private Span<byte> _span;
        private int _buffered;
        private int _written;

        internal readonly ref struct Reserved
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
        internal Reserved Reserve(int length)
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

        public MongoDBBsonWriter(IBufferWriter<byte> output)
        {
            _output = output;
            _span = _output.GetSpan();
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
        private void GetNextSpan()
        {
            if (_buffered > 0)
            {
                Commit();
            }
            _span = _output.GetSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            _buffered += count;
            _written += count;
            _span = _span.Slice(count);
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
            if ( _span.Length == 0 )
            {
                GetNextSpan();
            }
            _span[0] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            if ( _span.Length == 1 )
            {
                _span[0] = (byte)value;
                GetNextSpan();
                _span[0] = (byte)(value << 8 );
                Advance(1);
                return;
            }
            if ( _span.Length == 0 )
            {
                GetNextSpan();
            }
            BinaryPrimitives.WriteInt16LittleEndian(_span, value);
            Advance(2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(int value)
        {
            if (_span.Length == 0)
            {
                GetNextSpan();
            }            
            if (_span.Length == 1)
            {
                _span[0] = (byte)value;
                GetNextSpan();
                _span[0] = (byte)(value << 24);
                _span[1] = (byte)(value << 16);
                _span[3] = (byte)(value << 8);
                Advance(3);
                return;
            }
            if(_span.Length == 2)
            {
                _span[0] = (byte)value;                
                _span[1] = (byte)(value << 24);
                GetNextSpan();
                _span[0] = (byte)(value << 16);
                _span[1] = (byte)(value << 8);
                Advance(2);
                return;
            }
            if (_span.Length == 3)
            {
                _span[0] = (byte)value;
                _span[1] = (byte)(value << 24);                
                _span[2] = (byte)(value << 16);
                GetNextSpan();
                _span[3] = (byte)(value << 8);
                Advance(1);
                return;
            }
            BinaryPrimitives.WriteInt32LittleEndian(_span, value);
            Advance(sizeof(int));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            if (_span.Length == 0)
            {
                GetNextSpan();
            }
            if (_span.Length == 1)
            {
                _span[0] = (byte)value;
                GetNextSpan();
                _span[0] = (byte)(value << 56);
                _span[1] = (byte)(value << 48);
                _span[2] = (byte)(value << 40);
                _span[3] = (byte)(value << 32);
                _span[4] = (byte)(value << 24);
                _span[5] = (byte)(value << 16);
                _span[6] = (byte)(value << 8);
                Advance(7);
                return;
            }
            if (_span.Length == 2)
            {
                _span[0] = (byte)value;                
                _span[1] = (byte)(value << 56);
                GetNextSpan();
                _span[0] = (byte)(value << 48);
                _span[1] = (byte)(value << 40);
                _span[2] = (byte)(value << 32);
                _span[3] = (byte)(value << 24);
                _span[4] = (byte)(value << 16);
                _span[5] = (byte)(value << 8);
                Advance(6);
                return;
            }
            if (_span.Length == 3)
            {
                _span[0] = (byte)value;
                _span[1] = (byte)(value << 56);                
                _span[2] = (byte)(value << 48);
                GetNextSpan();
                _span[0] = (byte)(value << 40);
                _span[1] = (byte)(value << 32);
                _span[2] = (byte)(value << 24);
                _span[3] = (byte)(value << 16);
                _span[4] = (byte)(value << 8);
                Advance(5);
                return;
            }
            if (_span.Length == 4)
            {
                _span[0] = (byte)value;
                _span[1] = (byte)(value << 56);
                _span[2] = (byte)(value << 48);                
                _span[3] = (byte)(value << 40);
                GetNextSpan();
                _span[0] = (byte)(value << 32);
                _span[1] = (byte)(value << 24);
                _span[2] = (byte)(value << 16);
                _span[3] = (byte)(value << 8);
                Advance(4);
                return;
            }
            if (_span.Length == 5)
            {
                _span[0] = (byte)value;
                _span[1] = (byte)(value << 56);
                _span[2] = (byte)(value << 48);
                _span[3] = (byte)(value << 40);                
                _span[4] = (byte)(value << 32);
                GetNextSpan();
                _span[0] = (byte)(value << 24);
                _span[1] = (byte)(value << 16);
                _span[2] = (byte)(value << 8);
                Advance(3);
                return;
            }
            if (_span.Length == 6)
            {
                _span[0] = (byte)value;
                _span[1] = (byte)(value << 56);
                _span[2] = (byte)(value << 48);
                _span[3] = (byte)(value << 40);
                _span[4] = (byte)(value << 32);                
                _span[5] = (byte)(value << 24);
                GetNextSpan();
                _span[0] = (byte)(value << 16);
                _span[1] = (byte)(value << 8);
                Advance(2);
                return;
            }
            if (_span.Length == 7)
            {
                _span[0] = (byte)value;
                _span[1] = (byte)(value << 56);
                _span[2] = (byte)(value << 48);
                _span[3] = (byte)(value << 40);
                _span[4] = (byte)(value << 32);
                _span[5] = (byte)(value << 24);                
                _span[6] = (byte)(value << 16);
                GetNextSpan();
                _span[0] = (byte)(value << 8);
                Advance(1);
                return;
            }
            BinaryPrimitives.WriteInt64LittleEndian(_span, value);
            Advance(sizeof(int));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDouble(double value)
        {
            unsafe
            {
                long longValue = *(long*)&value;
                WriteInt64(longValue);
            }
          
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCString(string value)
        {
            var span = Encoding.UTF8.GetBytes(value);           
            WriteBytes(span);
            WriteByte((byte)'\x00');
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(string value)
        {
            //var len = Encoding.UTF8.GetByteCount(value);
            var span = Encoding.UTF8.GetBytes(value);
            WriteInt32(span.Length);
            WriteBytes(span);
            WriteByte((byte)'\x00');
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
        public void WriteBoolean(bool value)
        {
            WriteByte(value ? 1 : 0);
        }
        public void WriteElement(BsonElement element)
        {
            WriteByte((byte)element.Type);
            WriteCString(element.Name);
            switch((byte)element.Type)
            {
                case 1:
                    {
                        WriteDouble((double)element.Value);
                        break;
                    }
                case 2:
                    {
                        WriteString((string)element.Value);
                        break;
                    }
                case 7:
                    {
                        WriteObjectId((BsonObjectId)element.Value);
                        break;
                    }
                case 16:
                    {
                        WriteInt32((int)element.Value);
                        break;
                    }
                case 18:
                    {
                        WriteInt64((long)element.Value);
                        break;
                    }
                default:
                    {
                        throw new ArgumentException($"{nameof(MongoDBBsonWriter)}.{nameof(WriteElement)}  with type {(byte)element.Type}");
                    }
            }
        }
        public void WriteDocument(BsonDocument document)
        {
            var reserved = Reserve(4);
            var checkpoint = _written;
            for (var i = 0; i < document.Elements.Count; i++)
            {
                WriteElement(document.Elements[i]);
            }
            WriteByte((byte)'\x00');
            Span<byte> sizeSpan = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(sizeSpan, _written - checkpoint);
            reserved.Write(sizeSpan);

        }
    }

}
