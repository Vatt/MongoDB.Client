using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Exceptions;

#nullable disable

namespace MongoDB.Client.Messages
{
    public partial class CursorResult<T> where T : IBsonSerializer<T>
    {
        public enum State
        {
            Prologue,
            MainLoop,
            MongoCursor,
            Epilogue
        }
        public class CursorResultState
        {
            public State State;
            public SequencePosition Position;
            public MongoCursor<T>.CursorState CursorState;
            public int DocLength;
            public int DocReadded;
            public double Ok;
            public string ErrorMessage;
            public int Code;
            public string CodeName;
            public MongoClusterTime ClusterTime;
            public BsonTimestamp? OperationTime;
            public CursorResultState()
            {
                CursorState = new();
                State = State.Prologue;
            }
            public CursorResult<T> CreateMessage() => new CursorResult<T>(CursorState.CreateCursor(), Ok, ErrorMessage, Code, CodeName, ClusterTime, OperationTime);
        }
        private static ReadOnlySpan<byte> CursorResultcursor => "cursor"u8;
        private static ReadOnlySpan<byte> CursorResultok => "ok"u8;
        private static ReadOnlySpan<byte> CursorResulterrmsg => "errmsg"u8;
        private static ReadOnlySpan<byte> CursorResultcode => "code"u8;
        private static ReadOnlySpan<byte> CursorResultcodeName => "codeName"u8;
        private static ReadOnlySpan<byte> CursorResult_clusterTime => "$clusterTime"u8;
        private static ReadOnlySpan<byte> CursorResultoperationTime => "operationTime"u8;

        public static bool TryParseBson(ref BsonReader reader, CursorResultState message)
        {
            switch (message.State)
            {
                case State.Prologue:
                    if (!reader.TryGetInt32(out message.DocLength))
                    {
                        return false;
                    }

                    message.DocReadded += sizeof(int);

                    goto case State.MainLoop;
                case State.MainLoop:
                    message.State = State.MainLoop;

                    if (TryParseMainLoop(ref reader, message) is false)
                    {
                        return false;
                    }

                    goto case State.Epilogue;
                case State.MongoCursor:
                    var checkpoint = message.CursorState.DocReadded;
                    var isCursorComplete = MongoCursor<T>.TryParseBson(ref reader, ref message.CursorState);

                    message.Position = message.CursorState.Position;
                    message.DocReadded += message.CursorState.DocReadded - checkpoint;

                    if (isCursorComplete is false)
                    {
   
                        return false;
                    }

                    goto case State.MainLoop;
                case State.Epilogue:
                    message.State = State.Epilogue;
                    message.Position = reader.Position;

                    if (!reader.TryGetByte(out var endMarker))
                    {
                        return false;
                    }

                    message.DocReadded += sizeof(byte);

                    if (endMarker != 0)
                    {
                        throw new SerializerEndMarkerException(nameof(CursorResult<T>), endMarker);
                    }

                    Debug.Assert(message.DocLength - message.DocReadded is 0);

                    break;
                default:
                    throw new InvalidOperationException($"Undefined State value in {nameof(CursorResult<T>)}");
            }

            message.Position = reader.Position;
            return true;
        }
        public static bool TryParseMainLoop(ref BsonReader reader, CursorResultState message)
        {
            var docLength = message.DocLength;
            while (docLength - message.DocReadded > 1)
            {
                message.Position = reader.Position;
                var checkpoint = reader.BytesConsumed;

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
                    message.DocReadded += (int)(reader.BytesConsumed - checkpoint);
                    continue;
                }

                var bsonNameLength = bsonName.Length;
                switch (System.Runtime.InteropServices.MemoryMarshal.GetReference(bsonName))
                {
                    case 101:
                        {
                            if (bsonName.SequenceEqual5(CursorResulterrmsg))
                            {
                                if (!reader.TryGetString(out message.ErrorMessage))
                                {
                                    return false;
                                }

                                message.DocReadded += (int)(reader.BytesConsumed - checkpoint);

                                continue;
                            }

                            break;
                        }

                    case 36:
                        {
                            if (bsonName.SequenceEqual9(CursorResult_clusterTime))
                            {
                                if (!MongoClusterTime.TryParseBson(ref reader, out message.ClusterTime))
                                {
                                    return false;
                                }

                                message.DocReadded += (int)(reader.BytesConsumed - checkpoint);

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
                                            var beforeCursor = (int)(reader.BytesConsumed - checkpoint);
                                            var isCursorComplete = MongoCursor<T>.TryParseBson(ref reader, ref message.CursorState);
                                            message.DocReadded += message.CursorState.DocReadded + beforeCursor;

                                            if (!isCursorComplete)
                                            {
                                                message.Position = message.CursorState.Position;
                                                message.State = State.MongoCursor;
                                                

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
                                            if (!reader.TryGetInt32(out message.Code))
                                            {
                                                return false;
                                            }

                                            continue;
                                        }

                                        if (bsonName.SequenceEqual8(CursorResultcodeName))
                                        {
                                            if (!reader.TryGetString(out message.CodeName))
                                            {
                                                return false;
                                            }

                                            message.DocReadded += (int)(reader.BytesConsumed - checkpoint);
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
                                            if (!reader.TryGetDouble(out message.Ok))
                                            {
                                                return false;
                                            }

                                            message.DocReadded += (int)(reader.BytesConsumed - checkpoint);
                                            continue;
                                        }

                                        break;
                                    }

                                case 112:
                                    {
                                        if (bsonName.SequenceEqual9(CursorResultoperationTime))
                                        {
                                            if (!reader.TryGetTimestamp(out message.OperationTime))
                                            {
                                                return false;
                                            }

                                            message.DocReadded += (int)(reader.BytesConsumed - checkpoint);
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
                else
                {
                    message.DocReadded += (int)(reader.BytesConsumed - checkpoint);
                }
            }

            return true;
        }

    }
}
