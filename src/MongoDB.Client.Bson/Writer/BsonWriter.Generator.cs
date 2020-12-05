using MongoDB.Client.Bson.Document;
using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Exceptions;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Bson.Writer
{

    public ref partial struct BsonWriter
    {
        private delegate void WriteDelegate<T>(ref BsonWriter write, in T message);
        private static void ThrowSerializerNotFound(string typeName)
        {
            throw new SerializerNotFoundException(typeName);
        }


        private static void ThrowSerializerIsNull(string typeName)
        {
            throw new SerializerIsNullException(typeName);
        }


        public unsafe void WriteGeneric<T>(T genericValue, ref Reserved typeReserved)
        {
            if (genericValue == null)
            {
                typeReserved.Write(10);
                return;
            }
            switch (genericValue)
            {
                case double value:
                    WriteDouble(value);
                    typeReserved.Write(1);
                    return;
                case string value:
                    WriteString(value);
                    typeReserved.Write(2);
                    return;
                case BsonArray value:
                    WriteDocument(value);
                    typeReserved.Write(4);
                    return;
                case BsonDocument value:
                    WriteDocument(value);
                    typeReserved.Write(3);
                    return;
                case Guid value:
                    WriteGuidAsBinaryData(value);
                    typeReserved.Write(5);
                    return;
                case BsonObjectId value:
                    WriteObjectId(value);
                    typeReserved.Write(7);
                    return;
                case bool value:
                    WriteBoolean(value);
                    typeReserved.Write(8);
                    return;
                case DateTimeOffset value:
                    WriteUtcDateTime(value);
                    typeReserved.Write(9);
                    return;
                case int value:
                    WriteInt32(value);
                    typeReserved.Write(16);
                    return;
                case long value:
                    WriteInt64(value);
                    typeReserved.Write(18);
                    return;
            }


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
                typeReserved.Write(3);
                serializer.Write(ref this, genericValue);
            }
        }


        private void WriteGuidAsBinaryData(Guid value)
        {
            const int guidSize = 16;
            WriteInt32(guidSize);
            WriteByte((byte)BsonBinaryDataType.UUID);
            WriteGuidAsBytes(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBsonNull(ReadOnlySpan<byte> name)
        {
            WriteByte(10);
            WriteCString(name);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBsonNull(int intName)
        {
            WriteByte(10);
            WriteIntIndex(intName);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name(byte type, ReadOnlySpan<byte> name)
        {
            WriteByte(type);
            WriteCString(name);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name(byte type, int intName)
        {
            WriteByte(type);
            WriteIntIndex(intName);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, double value)
        {
            WriteByte(1);
            WriteCString(name);
            WriteDouble(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(int intName, double value)
        {
            WriteByte(1);
            WriteIntIndex(intName);
            WriteDouble(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, string value)
        {
            WriteByte(2);
            WriteCString(name);
            WriteString(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(int intName, string value)
        {
            WriteByte(2);
            WriteIntIndex(intName);
            WriteString(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonDocument value)
        {
            WriteByte(3);
            WriteCString(name);
            WriteDocument(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(int intName, BsonDocument value)
        {
            WriteByte(3);
            WriteIntIndex(intName);
            WriteDocument(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonArray value)
        {
            WriteByte(4);
            WriteCString(name);
            WriteDocument(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(int intName, BsonArray value)
        {
            WriteByte(4);
            WriteIntIndex(intName);
            WriteDocument(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonObjectId value)
        {
            WriteByte(7);
            WriteCString(name);
            WriteObjectId(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(int intName, BsonObjectId value)
        {
            WriteByte(7);
            WriteIntIndex(intName);
            WriteObjectId(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, bool value)
        {
            WriteByte(8);
            WriteCString(name);
            WriteBoolean(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(int intName, bool value)
        {
            WriteByte(8);
            WriteIntIndex(intName);
            WriteBoolean(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, Guid value)
        {
            WriteByte(5);
            WriteCString(name);
            WriteGuidAsBinaryData(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, DateTimeOffset value)
        {
            WriteByte(9);
            WriteCString(name);
            WriteUtcDateTime(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(int intName, DateTimeOffset value)
        {
            WriteByte(9);
            WriteIntIndex(intName);
            WriteUtcDateTime(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, int value)
        {
            WriteByte(16);
            WriteCString(name);
            WriteInt32(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(int intName, int value)
        {
            WriteByte(16);
            WriteIntIndex(intName);
            WriteInt32(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, long value)
        {
            WriteByte(18);
            WriteCString(name);
            WriteInt64(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(int intName, long value)
        {
            WriteByte(18);
            WriteIntIndex(intName);
            WriteInt64(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteIntIndex(int index)
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
            Utf8Formatter.TryFormat(index, span, out int written2);
            var count = Math.Min(_span.Length, written2);
            span = span.Slice(0, written2);
            span.Slice(0, count).CopyTo(_span);
            Advance(count);
            span.Slice(count).CopyTo(_span);
            Advance(written2 - count);
            WriteByte(EndMarker);
        }
    }
}
