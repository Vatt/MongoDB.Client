using System;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Exceptions;

namespace MongoDB.Client.Bson.Writer
{

    public ref partial struct BsonWriter
    {
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowSerializerNotFound(string typeName)
        {
            throw new SerializerNotFoundException(typeName);
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowSerializerIsNull(string typeName)
        {
            throw new SerializerIsNullException(typeName);
        }


        public unsafe void WriteGeneric<T>(T genericValue, ref Reserved typeReserved)
        {
            if (genericValue == null)
            {
                typeReserved.WriteByte(10);
                return;
            }
            //if (SerializerFnPtrProvider<T>.IsSerializable)
            //{
            //    goto SERIALIZABLE;
            //}
            switch (genericValue)
            {
                case double value:
                    WriteDouble(value);
                    typeReserved.WriteByte(1);
                    return;
                case string value:
                    WriteString(value);
                    typeReserved.WriteByte(2);
                    return;
                case BsonArray value:
                    WriteDocument(value);
                    typeReserved.WriteByte(4);
                    return;
                case BsonDocument value:
                    WriteDocument(value);
                    typeReserved.WriteByte(3);
                    return;
                case Guid value:
                    WriteGuidAsBinaryData(value);
                    typeReserved.WriteByte(5);
                    return;
                case BsonObjectId value:
                    WriteObjectId(value);
                    typeReserved.WriteByte(7);
                    return;
                case bool value:
                    WriteBoolean(value);
                    typeReserved.WriteByte(8);
                    return;
                case DateTimeOffset value:
                    WriteUtcDateTime(value);
                    typeReserved.WriteByte(9);
                    return;
                case int value:
                    WriteInt32(value);
                    typeReserved.WriteByte(16);
                    return;
                case long value:
                    WriteInt64(value);
                    typeReserved.WriteByte(18);
                    return;
                    //default:
                    //    System.Diagnostics.Debugger.Break();
                    //    break;
            }

            //SERIALIZABLE:
            var writer = SerializerFnPtrProvider<T>.WriteFnPtr;
            if (writer != default)
            {
                writer(ref this, genericValue);
            }
            else
            {
                if (!SerializersMap.TryGetSerializer<T>(out var serializer))
                {
                    ThrowSerializerNotFound(typeof(T).Name);
                }
                if (serializer is null)
                {
                    ThrowSerializerIsNull(typeof(T).Name);
                }
                typeReserved.WriteByte(3);
                serializer.WriteBson(ref this, genericValue);
            }
        }


        private void WriteGuidAsBinaryData(Guid value)
        {
            const int guidSize = 16;
            WriteInt32(guidSize);
            WriteByte((byte)BsonBinaryDataType.UUID);
            WriteGuidAsBytes(value);
        }
        private void WriteBinaryData(byte subtype, byte[] value)
        {
            WriteInt32(value.Length);
            WriteByte(subtype);
            WriteBytes(value);
        }
        private void WriteBinaryData(byte subtype, Memory<byte> value)
        {
            WriteInt32(value.Length);
            WriteByte(subtype);
            WriteBytes(value.Span);
        }


        public void WriteString(ReadOnlySpan<byte> value)
        {
            var count = value.Length;
            WriteInt32(count + 1);
            if (count <= _span.Length)
            {
                value.CopyTo(_span);
                Advance(count);
                WriteByte(EndMarker);
                return;
            }

            SlowWriteString(value);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public void SlowWriteString(ReadOnlySpan<byte> value)
        {
            do
            {
                var count = Math.Min(_span.Length, value.Length);
                value.Slice(0, count).CopyTo(_span);
                value = value.Slice(count);
                Advance(count);
            } while (!value.IsEmpty);
            WriteByte(EndMarker);
        }



        public void WriteCString(ReadOnlySpan<byte> value)
        {
            var count = value.Length;
            if (count <= _span.Length)
            {
                value.CopyTo(_span);
                Advance(count);
                WriteByte(EndMarker);
                return;
            }

            SlowWriteCString(value);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public void SlowWriteCString(ReadOnlySpan<byte> value)
        {
            do
            {
                var count = Math.Min(_span.Length, value.Length);
                value.Slice(0, count).CopyTo(_span);
                value = value.Slice(count);
                Advance(count);
            } while (!value.IsEmpty);
            WriteByte(EndMarker);
        }



        public void WriteBsonNull(ReadOnlySpan<byte> name)
        {
            WriteByte(10);
            WriteCString(name);
        }



        public void WriteBsonNull(int intName)
        {
            WriteByte(10);
            WriteIntIndex(intName);
        }



        public void Write_Type_Name(byte type, ReadOnlySpan<byte> name)
        {
            WriteByte(type);
            WriteCString(name);
        }



        public void Write_Type_Name(byte type, int intName)
        {
            WriteByte(type);
            WriteIntIndex(intName);
        }

        public void WriteName(ReadOnlySpan<byte> name)
        {
            WriteCString(name);
        }
        public void WriteName(int intName)
        {
            WriteIntIndex(intName);
        }
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, double value)
        {
            WriteByte(1);
            WriteCString(name);
            WriteDouble(value);
        }


        public void Write_Type_Name_Value(string name, string value)
        {
            WriteByte(1);
            WriteCString(name);
            WriteString(value);
        }



        public void Write_Type_Name_Value(int intName, double value)
        {
            WriteByte(1);
            WriteIntIndex(intName);
            WriteDouble(value);
        }


        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonBinaryData value)
        {
            WriteByte(5);
            WriteCString(name);
            WriteBinaryData(value);
        }



        public void Write_Type_Name_Value(int intName, BsonBinaryData value)
        {
            WriteByte(5);
            WriteIntIndex(intName);
            WriteBinaryData(value);
        }



        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, string value)
        {
            WriteByte(2);
            WriteCString(name);
            WriteString(value);
        }



        public void Write_Type_Name_Value(int intName, string value)
        {
            WriteByte(2);
            WriteIntIndex(intName);
            WriteString(value);
        }



        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonDocument value)
        {
            WriteByte(3);
            WriteCString(name);
            WriteDocument(value);
        }



        public void Write_Type_Name_Value(int intName, BsonDocument value)
        {
            WriteByte(3);
            WriteIntIndex(intName);
            WriteDocument(value);
        }



        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonArray value)
        {
            WriteByte(4);
            WriteCString(name);
            WriteDocument(value);
        }



        public void Write_Type_Name_Value(int intName, BsonArray value)
        {
            WriteByte(4);
            WriteIntIndex(intName);
            WriteDocument(value);
        }



        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonObjectId value)
        {
            WriteByte(7);
            WriteCString(name);
            WriteObjectId(value);
        }



        public void Write_Type_Name_Value(int intName, BsonObjectId value)
        {
            WriteByte(7);
            WriteIntIndex(intName);
            WriteObjectId(value);
        }



        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, bool value)
        {
            WriteByte(8);
            WriteCString(name);
            WriteBoolean(value);
        }



        public void Write_Type_Name_Value(int intName, bool value)
        {
            WriteByte(8);
            WriteIntIndex(intName);
            WriteBoolean(value);
        }



        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, Guid value)
        {
            WriteByte(5);
            WriteCString(name);
            WriteGuidAsBinaryData(value);
        }


        public void Write_Type_Name_Value(ReadOnlySpan<char> name, Guid value)
        {
            WriteByte(5);
            WriteCString(name);
            WriteGuidAsBinaryData(value);
        }


        public void Write_Type_Name_Value(int intName, Guid value)
        {
            WriteByte(5);
            WriteIntIndex(intName);
            WriteGuidAsBinaryData(value);
        }

        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, byte subtype, byte[] value)
        {
            WriteByte(5);
            WriteCString(name);
            WriteBinaryData(subtype, value);
        }

        public void Write_Type_Name_Value(int intName, byte subtype, byte[] value)
        {
            WriteByte(5);
            WriteIntIndex(intName);
            WriteBinaryData(subtype, value);
        }
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, byte subtype, Memory<byte> value)
        {
            WriteByte(5);
            WriteCString(name);
            WriteBinaryData(subtype, value);
        }

        public void Write_Type_Name_Value(int intName, byte subtype, Memory<byte> value)
        {
            WriteByte(5);
            WriteIntIndex(intName);
            WriteBinaryData(subtype, value);
        }

        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, DateTimeOffset value)
        {
            WriteByte(9);
            WriteCString(name);
            WriteUtcDateTime(value);
        }



        public void Write_Type_Name_Value(int intName, DateTimeOffset value)
        {
            WriteByte(9);
            WriteIntIndex(intName);
            WriteUtcDateTime(value);
        }


        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonTimestamp value)
        {
            WriteByte(17);
            WriteCString(name);
            WriteTimestamp(value);
        }



        public void Write_Type_Name_Value(int intName, BsonTimestamp value)
        {
            WriteByte(17);
            WriteIntIndex(intName);
            WriteTimestamp(value);
        }




        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, int value)
        {
            WriteByte(16);
            WriteCString(name);
            WriteInt32(value);
        }



        public void Write_Type_Name_Value(int intName, int value)
        {
            WriteByte(16);
            WriteIntIndex(intName);
            WriteInt32(value);
        }



        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, long value)
        {
            WriteByte(18);
            WriteCString(name);
            WriteInt64(value);
        }



        public void Write_Type_Name_Value(int intName, long value)
        {
            WriteByte(18);
            WriteIntIndex(intName);
            WriteInt64(value);
        }


        private void WriteIntIndex(int index)
        {
            if (index < 10)
            {
                _span[0] = (byte)('0' + index);
                Advance(1);
                WriteByte(EndMarker);
            }
            else if (index < 100 && _span.Length > 2)
            {
                var remainder = Math.DivRem(index, 10, out var result);
                _span[0] = (byte)('0' + result);
                _span[1] = (byte)('0' + remainder);
                Advance(2);
                WriteByte(EndMarker);
            }
            else
            {
                WriteIntIndex2(index);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void WriteIntIndex2(int index)
        {
            if (Utf8Formatter.TryFormat(index, _span, out int written))
            {
                Advance(written);
                WriteByte(EndMarker);
            }
            else
            {
                WriteIntIndexSlow(index);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void WriteIntIndexSlow(int index)
        {
            Span<byte> span = stackalloc byte[10];
            Utf8Formatter.TryFormat(index, span, out int written);
            var count = Math.Min(_span.Length, written);
            span = span.Slice(0, written);
            span.Slice(0, count).CopyTo(_span);
            Advance(count);
            span.Slice(count).CopyTo(_span);
            Advance(written - count);
            WriteByte(EndMarker);
        }
    }
}
