using System;
using MongoDB.Client.Bson.Reader;

namespace MongoDB.Client.Messages
{
    public struct CursorResultState<T>
    {
        public int? DocLen;
        public SequencePosition Position;
        public MongoDB.Client.Messages.MongoCursor<T> MongoCursorMongoCursor;
        public double DoubleOk;
        public string StringErrorMessage;
        public MongoDB.Client.Messages.MongoClusterTime MongoClusterTimeClusterTime;
        public MongoDB.Client.Bson.Document.BsonTimestamp? NullableOperationTime;
        public int Int32Code;
        public string StringCodeName;
        public MongoDB.Client.Messages.CursorResult<T> message;
        public bool EndMarker;
    }
    public partial class CursorResult<T>
    {
        private static ReadOnlySpan<byte> CursorResultcursor => new byte[6] { 99, 117, 114, 115, 111, 114 };
        private static ReadOnlySpan<byte> CursorResultok => new byte[2] { 111, 107 };
        private static ReadOnlySpan<byte> CursorResulterrmsg => new byte[6] { 101, 114, 114, 109, 115, 103 };
        private static ReadOnlySpan<byte> CursorResultcode => new byte[4] { 99, 111, 100, 101 };
        private static ReadOnlySpan<byte> CursorResultcodeName => new byte[8] { 99, 111, 100, 101, 78, 97, 109, 101 };
        private static ReadOnlySpan<byte> CursorResult_clusterTime => new byte[12] { 36, 99, 108, 117, 115, 116, 101, 114, 84, 105, 109, 101 };
        private static ReadOnlySpan<byte> CursorResultoperationTime => new byte[13] { 111, 112, 101, 114, 97, 116, 105, 111, 110, 84, 105, 109, 101 };
        public static bool TryParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, out CursorResultState<T> state)
        {
            state = new CursorResultState<T>();
            state.EndMarker = false;
            if (!reader.TryGetInt32(out int docLength))
            {
                return false;
            }
            state.DocLen = docLength;
            var unreaded = reader.Remaining + sizeof(int);
            while (unreaded - reader.Remaining < docLength - 1)
            {
                state.Position = reader.Position;
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

                switch (System.Runtime.InteropServices.MemoryMarshal.GetReference(bsonName))
                {
                    case 101:
                        {
                            if (bsonName.SequenceEqual5(CursorResulterrmsg))
                            {
                                if (!reader.TryGetString(out state.StringErrorMessage))
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
                                if (!MongoDB.Client.Messages.MongoClusterTime.TryParseBson(ref reader, out state.MongoClusterTimeClusterTime))
                                {
                                    return false;
                                }

                                continue;
                            }

                            break;
                        }

                    case 99:
                        {
                            if (bsonName.Length < 1)
                            {
                                break;
                            }

                            switch (bsonName[1])
                            {
                                case 117:
                                    {
                                        if (bsonName.SequenceEqual5(CursorResultcursor))
                                        {
                                            if (!MongoDB.Client.Messages.MongoCursor<T>.TryParseBson(ref reader, out state.MongoCursorMongoCursor))
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
                                            if (!reader.TryGetInt32(out state.Int32Code))
                                            {
                                                return false;
                                            }

                                            continue;
                                        }

                                        if (bsonName.SequenceEqual8(CursorResultcodeName))
                                        {
                                            if (!reader.TryGetString(out state.StringCodeName))
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
                            if (bsonName.Length < 1)
                            {
                                break;
                            }

                            switch (bsonName[1])
                            {
                                case 107:
                                    {
                                        if (bsonName.SequenceEqual2(CursorResultok))
                                        {
                                            if (!reader.TryGetDouble(out state.DoubleOk))
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
                                            if (!reader.TryGetTimestamp(out state.NullableOperationTime))
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
                state.EndMarker = true;
                return false;
            }

            if (endMarker != 0)
            {
                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(MongoDB.Client.Messages.CursorResult<T>), endMarker);
            }

            state.message = new MongoDB.Client.Messages.CursorResult<T>(mongoCursor: state.MongoCursorMongoCursor, ok: state.DoubleOk, errorMessage: state.StringErrorMessage, code: state.Int32Code, codeName: state.StringCodeName, clusterTime: state.MongoClusterTimeClusterTime, operationTime: state.NullableOperationTime);
            return true;
        }
        public static bool TryContinueParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, ref CursorResultState<T> state)
        {
            if(state.EndMarker)
            {
                goto END_MARKER;
            }
            var docLength = state.DocLen!.Value;
            var unreaded = reader.Remaining + sizeof(int);
            while (unreaded - reader.Remaining < docLength - 1)
            {
                state.Position = reader.Position;
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

                switch (System.Runtime.InteropServices.MemoryMarshal.GetReference(bsonName))
                {
                    case 101:
                        {
                            if (bsonName.SequenceEqual5(CursorResulterrmsg))
                            {
                                if (!reader.TryGetString(out state.StringErrorMessage))
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
                                if (!MongoDB.Client.Messages.MongoClusterTime.TryParseBson(ref reader, out state.MongoClusterTimeClusterTime))
                                {
                                    return false;
                                }

                                continue;
                            }

                            break;
                        }

                    case 99:
                        {
                            if (bsonName.Length < 1)
                            {
                                break;
                            }

                            switch (bsonName[1])
                            {
                                case 117:
                                    {
                                        if (bsonName.SequenceEqual5(CursorResultcursor))
                                        {
                                            if (!MongoDB.Client.Messages.MongoCursor<T>.TryParseBson(ref reader, out state.MongoCursorMongoCursor))
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
                                            if (!reader.TryGetInt32(out state.Int32Code))
                                            {
                                                return false;
                                            }

                                            continue;
                                        }

                                        if (bsonName.SequenceEqual8(CursorResultcodeName))
                                        {
                                            if (!reader.TryGetString(out state.StringCodeName))
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
                            if (bsonName.Length < 1)
                            {
                                break;
                            }

                            switch (bsonName[1])
                            {
                                case 107:
                                    {
                                        if (bsonName.SequenceEqual2(CursorResultok))
                                        {
                                            if (!reader.TryGetDouble(out state.DoubleOk))
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
                                            if (!reader.TryGetTimestamp(out state.NullableOperationTime))
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
            END_MARKER:
            if (!reader.TryGetByte(out var endMarker))
            {
                state.EndMarker = true;
                return false;
            }

            if (endMarker != 0)
            {
                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(MongoDB.Client.Messages.CursorResult<T>), endMarker);
            }

            state.message = new MongoDB.Client.Messages.CursorResult<T>(mongoCursor: state.MongoCursorMongoCursor, ok: state.DoubleOk, errorMessage: state.StringErrorMessage, code: state.Int32Code, codeName: state.StringCodeName, clusterTime: state.MongoClusterTimeClusterTime, operationTime: state.NullableOperationTime);
            return true;
        }

        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in MongoDB.Client.Messages.CursorResult<T> message)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            if (message.MongoCursor == null)
            {
                writer.WriteBsonNull(CursorResultcursor);
            }
            else
            {
                writer.Write_Type_Name(3, CursorResultcursor);
                MongoDB.Client.Messages.MongoCursor<T>.WriteBson(ref writer, message.MongoCursor);
            }

            writer.Write_Type_Name_Value(CursorResultok, message.Ok);
            if (message.ErrorMessage == null)
            {
                writer.WriteBsonNull(CursorResulterrmsg);
            }
            else
            {
                writer.Write_Type_Name_Value(CursorResulterrmsg, message.ErrorMessage);
            }

            writer.Write_Type_Name_Value(CursorResultcode, message.Code);
            if (message.CodeName == null)
            {
                writer.WriteBsonNull(CursorResultcodeName);
            }
            else
            {
                writer.Write_Type_Name_Value(CursorResultcodeName, message.CodeName);
            }

            if (message.ClusterTime == null)
            {
                writer.WriteBsonNull(CursorResult_clusterTime);
            }
            else
            {
                writer.Write_Type_Name(3, CursorResult_clusterTime);
                MongoDB.Client.Messages.MongoClusterTime.WriteBson(ref writer, message.ClusterTime);
            }

            if (message.OperationTime.HasValue == false)
            {
                writer.WriteBsonNull(CursorResultoperationTime);
            }
            else
            {
                writer.Write_Type_Name_Value(CursorResultoperationTime, message.OperationTime.Value);
            }

            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            reserved.Write(docLength);
            writer.Commit();
        }
    }
}
