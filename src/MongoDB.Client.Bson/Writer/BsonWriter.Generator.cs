using MongoDB.Client.Bson.Document;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace MongoDB.Client.Bson.Writer
{
    public ref partial struct BsonWriter
    {
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
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, double value)
        {
            WriteByte(1);
            WriteCString(name);
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
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, BsonDocument value)
        {
            WriteByte(3);
            WriteCString(name);
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
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, bool value) 
        {
            WriteByte(8);
            WriteCString(name);
            WriteBoolean(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, DateTimeOffset value)
        {
            WriteByte(9);
            WriteCString(name);
            WriteUTCDateTime(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, int value)
        {
            WriteByte(16);
            WriteCString(name);
            WriteInt32(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, long value)
        {
            WriteByte(18);
            WriteCString(name);
            WriteInt64(value);
        }       
    }
}
