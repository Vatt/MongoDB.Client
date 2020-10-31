using MongoDB.Client.Bson.Document;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace MongoDB.Client.Bson.Writer
{
    public ref partial struct BsonWriter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value) => WriteDouble(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value) => WriteString(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BsonDocument value) => WriteDocument(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BsonObjectId value) => WriteObjectId(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value) => WriteBoolean(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(DateTimeOffset value) => WriteUtcDateTime(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value) => WriteInt32(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value) => WriteInt64(value);        
    }
}
