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
        private static void ThrowUnsupportedTypeType(string typeName)
        {
            throw new UnsupportedTypeException(typeName);
        }
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowSerializerIsNull(string typeName)
        {
            throw new SerializerIsNullException(typeName);
        }

        //TODO: support byte[] as BinaryData with check attribute representation


        public unsafe void WriteObject(object objectValue, ref Reserved typeReserved)
        {
            if (objectValue == null)
            {
                typeReserved.WriteBsonType(BsonType.Null);
                return;
            }
            switch (objectValue)
            {
                case double value:
                    WriteDouble(value);
                    typeReserved.WriteBsonType(BsonType.Double);
                    return;
                case string value:
                    WriteString(value);
                    typeReserved.WriteBsonType(BsonType.String);
                    return;
                case BsonArray value:
                    WriteDocument(value);
                    typeReserved.WriteBsonType(BsonType.Array);
                    return;
                case BsonDocument value:
                    WriteDocument(value);
                    typeReserved.WriteBsonType(BsonType.Document);
                    return;
                case Guid value:
                    WriteGuidAsBinaryData(value);
                    typeReserved.WriteBsonType(BsonType.BinaryData);
                    return;
                case BsonObjectId value:
                    WriteObjectId(value);
                    typeReserved.WriteBsonType(BsonType.ObjectId);
                    return;
                case bool value:
                    WriteBoolean(value);
                    typeReserved.WriteBsonType(BsonType.Boolean);
                    return;
                case DateTimeOffset value:
                    WriteUtcDateTime(value);
                    typeReserved.WriteBsonType(BsonType.UtcDateTime);
                    return;
                case BsonTimestamp value:
                    WriteTimestamp(value);
                    typeReserved.WriteBsonType(BsonType.Timestamp);
                    return;
                case int value:
                    WriteInt32(value);
                    typeReserved.WriteBsonType(BsonType.Int32);
                    return;
                case long value:
                    WriteInt64(value);
                    typeReserved.WriteBsonType(BsonType.Int64);
                    return;
                case decimal value:
                    WriteDecimal(value);
                    typeReserved.WriteBsonType(BsonType.Decimal);
                    return;
                    //default:
                    //    System.Diagnostics.Debugger.Break();
                    //    break; 
            }
            if (ReflectionHelper.TryGetSerializerMethods(objectValue, out var methods))
            {
                typeReserved.WriteBsonType(BsonType.Document);
                methods.WriteFnPtr(ref this, objectValue);
            }
            else
            {
                ThrowUnsupportedTypeType(objectValue.GetType().Name);
            }
        }
        public unsafe void WriteGeneric<T>(T genericValue, ref Reserved typeReserved)
        {
            if (genericValue == null)
            {
                typeReserved.WriteBsonType(BsonType.Null);
                return;
            }
            switch (genericValue)
            {
                case double value:
                    WriteDouble(value);
                    typeReserved.WriteBsonType(BsonType.Double);
                    return;
                case string value:
                    WriteString(value);
                    typeReserved.WriteBsonType(BsonType.String);
                    return;
                case BsonArray value:
                    WriteDocument(value);
                    typeReserved.WriteBsonType(BsonType.Array);
                    return;
                case BsonDocument value:
                    WriteDocument(value);
                    typeReserved.WriteBsonType(BsonType.Document);
                    return;
                case Guid value:
                    WriteGuidAsBinaryData(value);
                    typeReserved.WriteBsonType(BsonType.BinaryData);
                    return;
                case BsonObjectId value:
                    WriteObjectId(value);
                    typeReserved.WriteBsonType(BsonType.ObjectId);
                    return;
                case BsonTimestamp value:
                    WriteTimestamp(value);
                    typeReserved.WriteBsonType(BsonType.Timestamp);
                    return;
                case bool value:
                    WriteBoolean(value);
                    typeReserved.WriteBsonType(BsonType.Boolean);
                    return;
                case DateTimeOffset value:
                    WriteUtcDateTime(value);
                    typeReserved.WriteBsonType(BsonType.UtcDateTime);
                    return;
                case int value:
                    WriteInt32(value);
                    typeReserved.WriteBsonType(BsonType.Int32);
                    return;
                case long value:
                    WriteInt64(value);
                    typeReserved.WriteBsonType(BsonType.Int64);
                    return;
                case decimal value:
                    WriteDecimal(value);
                    typeReserved.WriteBsonType(BsonType.Decimal);
                    return;
                    //default:
                    //    System.Diagnostics.Debugger.Break();
                    //    break;
            }
            var writer = SerializerFnPtrProvider<T>.WriteFnPtr;
            if (writer != default)
            {
                typeReserved.WriteBsonType(BsonType.Document);
                writer(ref this, genericValue);
            }
            else
            {
                //if (!SerializersMap.TryGetSerializer<T>(out var serializer))
                //{
                //    ThrowSerializerNotFound(typeof(T).Name);
                //}
                //if (serializer is null)
                //{
                //    ThrowSerializerIsNull(typeof(T).Name);
                //}
                //typeReserved.WriteByte(3);
                //serializer.WriteBson(ref this, genericValue);
                ThrowSerializerNotFound(typeof(T).Name);
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
            WriteBsonType(BsonType.Null);
            WriteCString(name);
        }

        public void WriteBsonNull(string name)
        {
            WriteBsonType(BsonType.Null);
            WriteCString(name);
        }

        public void WriteBsonNull(int intName)
        {
            WriteBsonType(BsonType.Null);
            WriteIntIndex(intName);
        }



        public void Write_Type_Name(byte type, ReadOnlySpan<byte> name)
        {
            WriteByte(type);
            WriteCString(name);
        }
        public void Write_Type_Name(byte type, string name)
        {
            WriteByte(type);
            WriteCString(name);
        }
        public void Write_Type_Name(byte type, int intName)
        {
            WriteByte(type);
            WriteIntIndex(intName);
        }

        public void WriteName(string name)
        {
            WriteCString(name);
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
            WriteBsonType(BsonType.Double);
            WriteCString(name);
            WriteDouble(value);
        }
        public void Write_Type_Name_Value(string name, double value)
        {
            WriteBsonType(BsonType.Double);
            WriteCString(name);
            WriteDouble(value);
        }
        public void Write_Type_Name_Value(int intName, double value)
        {
            WriteBsonType(BsonType.Double);
            WriteIntIndex(intName);
            WriteDouble(value);
        }
        public void Write_Type_Name_Value(string name, decimal value)
        {
            WriteBsonType(BsonType.Decimal);
            WriteCString(name);
            WriteDecimal(value);
        }
        public void Write_Type_Name_Value(int intName, decimal value)
        {
            WriteBsonType(BsonType.Decimal);
            WriteIntIndex(intName);
            WriteDecimal(value);
        }

        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonBinaryData value)
        {
            WriteBsonType(BsonType.BinaryData);
            WriteCString(name);
            WriteBinaryData(value);
        }
        public void Write_Type_Name_Value(string name, BsonBinaryData value)
        {
            WriteBsonType(BsonType.BinaryData);
            WriteCString(name);
            WriteBinaryData(value);
        }
        public void Write_Type_Name_Value(int intName, BsonBinaryData value)
        {
            WriteBsonType(BsonType.BinaryData);
            WriteIntIndex(intName);
            WriteBinaryData(value);
        }



        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, string value)
        {
            WriteBsonType(BsonType.String);
            WriteCString(name);
            WriteString(value);
        }

        public void Write_Type_Name_Value(int intName, string value)
        {
            WriteBsonType(BsonType.String);
            WriteIntIndex(intName);
            WriteString(value);
        }
        public void Write_Type_Name_Value(string name, string value)
        {
            WriteBsonType(BsonType.String);
            WriteCString(name);
            WriteString(value);
        }


        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonDocument value)
        {
            WriteBsonType(BsonType.Document);
            WriteCString(name);
            WriteDocument(value);
        }
        public void Write_Type_Name_Value(string name, BsonDocument value)
        {
            WriteBsonType(BsonType.Document);
            WriteCString(name);
            WriteDocument(value);
        }
        public void Write_Type_Name_Value(int intName, BsonDocument value)
        {
            WriteBsonType(BsonType.Document);
            WriteIntIndex(intName);
            WriteDocument(value);
        }



        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonArray value)
        {
            WriteBsonType(BsonType.Array);
            WriteCString(name);
            WriteDocument(value);
        }
        public void Write_Type_Name_Value(string name, BsonArray value)
        {
            WriteBsonType(BsonType.Array);
            WriteCString(name);
            WriteDocument(value);
        }
        public void Write_Type_Name_Value(int intName, BsonArray value)
        {
            WriteBsonType(BsonType.Array);
            WriteIntIndex(intName);
            WriteDocument(value);
        }



        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonObjectId value)
        {
            WriteBsonType(BsonType.ObjectId);
            WriteCString(name);
            WriteObjectId(value);
        }
        public void Write_Type_Name_Value(string name, BsonObjectId value)
        {
            WriteBsonType(BsonType.ObjectId);
            WriteCString(name);
            WriteObjectId(value);
        }
        public void Write_Type_Name_Value(int intName, BsonObjectId value)
        {
            WriteBsonType(BsonType.ObjectId);
            WriteIntIndex(intName);
            WriteObjectId(value);
        }



        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, bool value)
        {
            WriteBsonType(BsonType.Boolean);
            WriteCString(name);
            WriteBoolean(value);
        }
        public void Write_Type_Name_Value(string name, bool value)
        {
            WriteBsonType(BsonType.Boolean);
            WriteCString(name);
            WriteBoolean(value);
        }
        public void Write_Type_Name_Value(int intName, bool value)
        {
            WriteBsonType(BsonType.Boolean);
            WriteIntIndex(intName);
            WriteBoolean(value);
        }



        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, Guid value)
        {
            WriteBsonType(BsonType.BinaryData);
            WriteCString(name);
            WriteGuidAsBinaryData(value);
        }
        public void Write_Type_Name_Value(string name, Guid value)
        {
            WriteBsonType(BsonType.BinaryData);
            WriteCString(name);
            WriteGuidAsBinaryData(value);
        }
        public void Write_Type_Name_Value(int intName, Guid value)
        {
            WriteBsonType(BsonType.BinaryData);
            WriteIntIndex(intName);
            WriteGuidAsBinaryData(value);
        }

        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, byte subtype, byte[] value)
        {
            WriteBsonType(BsonType.BinaryData);
            WriteCString(name);
            WriteBinaryData(subtype, value);
        }
        public void Write_Type_Name_Value(string name, byte subtype, byte[] value)
        {
            WriteBsonType(BsonType.BinaryData);
            WriteCString(name);
            WriteBinaryData(subtype, value);
        }
        public void Write_Type_Name_Value(int intName, byte subtype, byte[] value)
        {
            WriteBsonType(BsonType.BinaryData);
            WriteIntIndex(intName);
            WriteBinaryData(subtype, value);
        }
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, byte subtype, Memory<byte> value)
        {
            WriteBsonType(BsonType.BinaryData);
            WriteCString(name);
            WriteBinaryData(subtype, value);
        }
        public void Write_Type_Name_Value(string name, byte subtype, Memory<byte> value)
        {
            WriteBsonType(BsonType.BinaryData);
            WriteCString(name);
            WriteBinaryData(subtype, value);
        }
        public void Write_Type_Name_Value(int intName, byte subtype, Memory<byte> value)
        {
            WriteBsonType(BsonType.BinaryData);
            WriteIntIndex(intName);
            WriteBinaryData(subtype, value);
        }

        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, DateTimeOffset value)
        {
            WriteBsonType(BsonType.UtcDateTime);
            WriteCString(name);
            WriteUtcDateTime(value);
        }
        public void Write_Type_Name_Value(string name, DateTimeOffset value)
        {
            WriteBsonType(BsonType.UtcDateTime);
            WriteCString(name);
            WriteUtcDateTime(value);
        }
        public void Write_Type_Name_Value(int intName, DateTimeOffset value)
        {
            WriteBsonType(BsonType.UtcDateTime);
            WriteIntIndex(intName);
            WriteUtcDateTime(value);
        }
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, decimal value)
        {
            WriteBsonType(BsonType.Decimal);
            WriteCString(name);
            WriteDecimal(value);
        }

        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonTimestamp value)
        {
            WriteBsonType(BsonType.Timestamp);
            WriteCString(name);
            WriteTimestamp(value);
        }
        public void Write_Type_Name_Value(string name, BsonTimestamp value)
        {
            WriteBsonType(BsonType.Timestamp);
            WriteCString(name);
            WriteTimestamp(value);
        }
        public void Write_Type_Name_Value(int intName, BsonTimestamp value)
        {
            WriteBsonType(BsonType.Timestamp);
            WriteIntIndex(intName);
            WriteTimestamp(value);
        }




        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, int value)
        {
            WriteBsonType(BsonType.Int32);
            WriteCString(name);
            WriteInt32(value);
        }
        public void Write_Type_Name_Value(int intName, int value)
        {
            WriteBsonType(BsonType.Int32);
            WriteIntIndex(intName);
            WriteInt32(value);
        }
        public void Write_Type_Name_Value(string name, int value)
        {
            WriteBsonType(BsonType.Int32);
            WriteCString(name);
            WriteInt32(value);
        }


        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, long value)
        {
            WriteBsonType(BsonType.Int64);
            WriteCString(name);
            WriteInt64(value);
        }
        public void Write_Type_Name_Value(string name, long value)
        {
            WriteBsonType(BsonType.Int64);
            WriteCString(name);
            WriteInt64(value);
        }
        public void Write_Type_Name_Value(int intName, long value)
        {
            WriteBsonType(BsonType.Int64);
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
