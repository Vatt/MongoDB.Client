using System;
using System.Collections.Generic;
using System.Buffers.Binary;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;

namespace MongoDB.Client.Messages
{
    public partial class CursorResult<T> : IBsonSerializer<MongoDB.Client.Messages.CursorResult<T>>
    {
        private static ReadOnlySpan<byte> CursorResultcursor => "cursor"u8;
        private static ReadOnlySpan<byte> CursorResultok => "ok"u8;
        private static ReadOnlySpan<byte> CursorResulterrmsg => "errmsg"u8;
        private static ReadOnlySpan<byte> CursorResultcode => "code"u8;
        private static ReadOnlySpan<byte> CursorResultcodeName => "codeName"u8;
        private static ReadOnlySpan<byte> CursorResult_clusterTime => "$clusterTime"u8;
        private static ReadOnlySpan<byte> CursorResultoperationTime => "operationTime"u8;

        public static bool TryParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, out MongoDB.Client.Messages.CursorResult<T> message)
        {
            message = default;
            MongoDB.Client.Messages.MongoCursor<T> MongoCursorMongoCursor = default;
            double DoubleOk = default;
            string StringErrorMessage = default;
            int Int32Code = default;
            string StringCodeName = default;
            MongoDB.Client.Messages.MongoClusterTime MongoClusterTimeClusterTime = default;
            MongoDB.Client.Bson.Document.BsonTimestamp? NullableOperationTime = default;
            if (!reader.TryGetInt32(out int docLength))
            {
                return false;
            }

            var unreaded = reader.Remaining + sizeof(int);
            while (unreaded - reader.Remaining < docLength - 1)
            {
                if (!reader.TryGetByte(out var bsonType))
                {
                    return false;
                }

                if (!reader.TryGetCStringAsSpan(out var bsonName))
                {
                    return false;
                }

                if (bsonType == 10)
                {
                    continue;
                }

                var bsonNameLength = bsonName.Length;
                switch (System.Runtime.InteropServices.MemoryMarshal.GetReference(bsonName))
                {
                    case 101:
                        {
                            if (bsonName.SequenceEqual5(CursorResulterrmsg))
                            {
                                if (!reader.TryGetString(out StringErrorMessage))
                                {
                                    return false;
                                }

                                continue;
                            }

                            break;
                        }

                    case 36:
                        {
                            if (bsonName.SequenceEqual9(CursorResult_clusterTime))
                            {
                                if (!MongoDB.Client.Messages.MongoClusterTime.TryParseBson(ref reader, out MongoClusterTimeClusterTime))
                                {
                                    return false;
                                }

                                continue;
                            }

                            break;
                        }

                    case 99:
                        {
                            if (bsonNameLength < 1)
                            {
                                break;
                            }

                            switch (System.Runtime.CompilerServices.Unsafe.Add(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(bsonName), (nint)1))
                            {
                                case 117:
                                    {
                                        if (bsonName.SequenceEqual5(CursorResultcursor))
                                        {
                                            var state = new MongoCursor<T>.CursorState();
                                            if (!MongoDB.Client.Messages.MongoCursor<T>.TryParseBson(ref reader, ref state))
                                            {
                                                return false;
                                            }

                                            continue;
                                        }

                                        break;
                                    }

                                case 111:
                                    {
                                        if (bsonName.SequenceEqual4(CursorResultcode))
                                        {
                                            if (!reader.TryGetInt32(out Int32Code))
                                            {
                                                return false;
                                            }

                                            continue;
                                        }

                                        if (bsonName.SequenceEqual8(CursorResultcodeName))
                                        {
                                            if (!reader.TryGetString(out StringCodeName))
                                            {
                                                return false;
                                            }

                                            continue;
                                        }

                                        break;
                                    }
                            }

                            break;
                        }

                    case 111:
                        {
                            if (bsonNameLength < 1)
                            {
                                break;
                            }

                            switch (System.Runtime.CompilerServices.Unsafe.Add(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(bsonName), (nint)1))
                            {
                                case 107:
                                    {
                                        if (bsonName.SequenceEqual2(CursorResultok))
                                        {
                                            if (!reader.TryGetDouble(out DoubleOk))
                                            {
                                                return false;
                                            }

                                            continue;
                                        }

                                        break;
                                    }

                                case 112:
                                    {
                                        if (bsonName.SequenceEqual9(CursorResultoperationTime))
                                        {
                                            if (!reader.TryGetTimestamp(out NullableOperationTime))
                                            {
                                                return false;
                                            }

                                            continue;
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                }

                if (!reader.TrySkip(bsonType))
                {
                    return false;
                }
            }

            if (!reader.TryGetByte(out var endMarker))
            {
                return false;
            }

            if (endMarker != 0)
            {
                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(MongoDB.Client.Messages.CursorResult<T>), endMarker);
            }

            message = new MongoDB.Client.Messages.CursorResult<T>(mongoCursor: MongoCursorMongoCursor, ok: DoubleOk, errorMessage: StringErrorMessage, code: Int32Code, codeName: StringCodeName, clusterTime: MongoClusterTimeClusterTime, operationTime: NullableOperationTime);
            return true;
        }

        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in MongoDB.Client.Messages.CursorResult<T> message) => throw new NotImplementedException();
    }
}
