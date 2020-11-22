using MongoDB.Client.Bson.Document;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Exceptions;
using MongoDB.Client.Bson.Utils;

namespace MongoDB.Client.Bson.Writer
{
    public ref partial struct BsonWriter
    {
        private static void ThrowSerializerNotFound(string typeName)
        {
            throw new SerializerNotFoundException(typeName);
        }
        private static void ThrowSerializerIsNull(string typeName)
        {
            throw new SerializerIsNullException(typeName);
        }
        public void WriteGeneric<T>(T genericValue, ref Reserved typeReserved)
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
            Commit();
            _output.Write(value);;
            Advance(value.Length);
            GetNextSpanWithoutCommit();
            WriteByte(EndMarker);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCString(ReadOnlySpan<byte> value)
        {
            Commit();
            _output.Write(value);
            Advance(value.Length);
            GetNextSpanWithoutCommit();
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
            var buffer = ArrayPool<byte>.Shared.Rent(10);
            try
            {
                _ = Utf8Formatter.TryFormat(intName, buffer, out int written);
                WriteByte(10);
                WriteCString(buffer.AsSpan(0, written));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
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
            var buffer = ArrayPool<byte>.Shared.Rent(10);
            try
            {
                _ = Utf8Formatter.TryFormat(intName, buffer, out int written);
                WriteByte(type);
                WriteCString(buffer.AsSpan(0, written));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
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
            var buffer = ArrayPool<byte>.Shared.Rent(10);
            try
            {
                _ = Utf8Formatter.TryFormat(intName, buffer, out int written);
                WriteByte(1);
                WriteCString(buffer.AsSpan(0, written));
                WriteDouble(value);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
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
            var buffer = ArrayPool<byte>.Shared.Rent(10);
            try
            {
                _ = Utf8Formatter.TryFormat(intName, buffer, out int written);
                WriteByte(2);
                WriteCString(buffer.AsSpan(0, written));
                WriteString(value);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
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
            var buffer = ArrayPool<byte>.Shared.Rent(10);
            try
            {
                _ = Utf8Formatter.TryFormat(intName, buffer, out int written);
                WriteByte(3);
                WriteCString(buffer.AsSpan(0, written));
                WriteDocument(value);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
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
            var buffer = ArrayPool<byte>.Shared.Rent(10);
            try
            {
                _ = Utf8Formatter.TryFormat(intName, buffer, out int written);
                WriteByte(4);
                WriteCString(buffer.AsSpan(0, written));
                WriteDocument(value);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
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
            var buffer = ArrayPool<byte>.Shared.Rent(10);
            try
            {
                _ = Utf8Formatter.TryFormat(intName, buffer, out int written);
                WriteByte(7);
                WriteCString(buffer.AsSpan(0, written));
                WriteObjectId(value);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
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
            var buffer = ArrayPool<byte>.Shared.Rent(10);
            try
            {
                _ = Utf8Formatter.TryFormat(intName, buffer, out int written);
                WriteByte(8);
                WriteCString(buffer.AsSpan(0, written));
                WriteBoolean(value);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
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
            var buffer = ArrayPool<byte>.Shared.Rent(10);
            try
            {
                _ = Utf8Formatter.TryFormat(intName, buffer, out int written);
                WriteByte(9);
                WriteCString(buffer.AsSpan(0, written));
                WriteUtcDateTime(value);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
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
            var buffer = ArrayPool<byte>.Shared.Rent(10);
            try
            {
                _ = Utf8Formatter.TryFormat(intName, buffer, out int written);
                WriteByte(16);
                WriteCString(buffer.AsSpan(0, written));
                WriteInt32(value);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

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
            var buffer = ArrayPool<byte>.Shared.Rent(10);
            try
            {
                _ = Utf8Formatter.TryFormat(intName, buffer, out int written);
                WriteByte(18);
                WriteCString(buffer.AsSpan(0, written));
                WriteInt64(value);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

        }

    }
}
