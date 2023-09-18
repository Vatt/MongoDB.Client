using System.Diagnostics;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Exceptions;
using MongoDB.Client.Protocol.Readers;

#nullable disable

namespace MongoDB.Client.Messages
{
    public partial class MongoCursor<T> where T : IBsonSerializer<T>
    {
        public enum State
        {
            Prologue = 1,
            MainLoop,
            FirstBatch,
            NextBatch,
            Epilogue
        }
        public struct CursorState
        {
            public State State;
            public long Id;
            public string Namespace;
            public List<T> FirstBatch;
            public List<T> NextBatch;
            public int BatchLength;
            public int BatchReadded;
            public int DocLength;
            public int DocReadded;
            public SequencePosition Position;

            public MongoCursor<T> CreateCursor() => new MongoCursor<T>(Id, Namespace, FirstBatch, NextBatch);

            public CursorState()
            {
                State = State.Prologue;
            }
        }
        private static ReadOnlySpan<byte> MongoCursorid => "id"u8;
        private static ReadOnlySpan<byte> MongoCursorns => "ns"u8;
        private static ReadOnlySpan<byte> MongoCursorfirstBatch => "firstBatch"u8;
        private static ReadOnlySpan<byte> MongoCursornextBatch => "nextBatch"u8;

        public static bool TryParseBson(ref BsonReader reader, ref CursorState message)
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

                    if (TryParseMainLoop(ref reader, ref message) is false)
                    {
                        return false;
                    }

                    goto case State.Epilogue;
                case State.FirstBatch:
                    var checkpoint = message.BatchReadded;

                    var isFirstBatchComplete = TryParseFirstBatch(ref reader, ref message);

                    message.DocReadded += message.BatchReadded - checkpoint;

                    if (isFirstBatchComplete is false)
                    {
                        return false;
                    }                 

                    goto case State.MainLoop;
                case State.NextBatch:
                    checkpoint = message.BatchReadded;

                    var isNextBatchComplete = TryParseNextBatch(ref reader, ref message);

                    message.DocReadded += message.BatchReadded - checkpoint;

                    if (isNextBatchComplete is false)
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

                    Debug.Assert(message.DocLength - message.DocReadded is 0);

                    if (endMarker != 0)
                    {
                        throw new SerializerEndMarkerException(nameof(MongoCursor<T>), endMarker);
                    }

                    break;
                default:
                    throw new InvalidOperationException($"Undefined State value in {nameof(MongoCursor<T>)}");
            }

            message.Position = reader.Position;
            return true;
        }


        private static bool TryParseMainLoop(ref BsonReader reader, ref CursorState message)
        {
            var docLength = message.DocLength;
            while (docLength - message.DocReadded > 1)
            {
                var checkpoint = reader.BytesConsumed;
                message.Position = reader.Position;

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
                    case 105:
                        {
                            if (bsonName.SequenceEqual2(MongoCursorid))
                            {
                                if (!reader.TryGetInt64(out message.Id))
                                {
                                    return false;
                                }

                                message.DocReadded += (int)(reader.BytesConsumed - checkpoint);
                                continue;
                            }

                            break;
                        }

                    case 102:
                        {
                            if (bsonName.SequenceEqual9(MongoCursorfirstBatch))
                            {

                                message.FirstBatch = new();
                                var check = reader.BytesConsumed - checkpoint;

                                var isBatchComplete = TryParseFirstBatch(ref reader, ref message);

                                message.DocReadded += (int)(message.BatchReadded + check);

                                if (!isBatchComplete)
                                {
                                    return false;
                                }                               

                                continue;
                            }

                            break;
                        }

                    case 110:
                        {
                            if (bsonNameLength < 1)
                            {
                                break;
                            }

                            switch (System.Runtime.CompilerServices.Unsafe.Add(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(bsonName), (nint)1))
                            {
                                case 115:
                                    {
                                        if (bsonName.SequenceEqual2(MongoCursorns))
                                        {
                                            if (!reader.TryGetString(out message.Namespace))
                                            {
                                                return false;
                                            }

                                            message.DocReadded += (int)(reader.BytesConsumed - checkpoint);

                                            continue;
                                        }

                                        break;
                                    }

                                case 101:
                                    {
                                        if (bsonName.SequenceEqual9(MongoCursornextBatch))
                                        {
                                            message.NextBatch = new();
                                            var check = reader.BytesConsumed - checkpoint;

                                            var isBatchComplete = TryParseNextBatch(ref reader, ref message);

                                            message.DocReadded += (int)(message.BatchReadded + check);

                                            if (!isBatchComplete)
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
                else
                {
                    message.DocReadded += (int)(reader.BytesConsumed - checkpoint);
                }
            }

            return true;
        }

        private static bool TryParseFirstBatch(ref BsonReader reader, ref CursorState state)
        {
            var internalList = state.FirstBatch;

            if (state.State is State.FirstBatch)
            {
                goto ELEMENTS;
            }

            if (!reader.TryGetInt32(out state.BatchLength))
            {
                return false;
            }
            state.State = State.FirstBatch;
            state.BatchReadded += sizeof(int);
        ELEMENTS:
            var checkpoint = reader.BytesConsumed;

            var isComplete = TryParseElements(ref reader, internalList, ref state.BatchLength, ref state.BatchReadded, out state.Position);

            if (isComplete is false)
            {
                return false;
            }

            if (!reader.TryGetByte(out var listEndMarker))
            {
                return false;
            }

            state.BatchReadded += sizeof(byte);

            Debug.Assert(state.BatchLength - state.BatchReadded is 0);

            if (listEndMarker != 0)
            {
                throw new SerializerEndMarkerException(nameof(MongoCursor<T>), listEndMarker);
            }

            return true;
        }
        private static bool TryParseNextBatch(ref BsonReader reader, ref CursorState state)
        {
            var internalList = state.NextBatch;

            if (state.State is State.NextBatch)
            {
                goto ELEMENTS;
            }

            if (!reader.TryGetInt32(out state.BatchLength))
            {
                return false;
            }
            state.State = State.NextBatch;
            state.BatchReadded += sizeof(int);
        ELEMENTS:
            var checkpoint = reader.BytesConsumed;

            var isComplete = TryParseElements(ref reader, internalList, ref state.BatchLength, ref state.BatchReadded, out state.Position);

            if (isComplete is false)
            {
                return false;
            }

            if (!reader.TryGetByte(out var listEndMarker))
            {
                return false;
            }

            state.BatchReadded += sizeof(byte);

            Debug.Assert(state.BatchLength - state.BatchReadded is 0);

            if (listEndMarker != 0)
            {
                throw new SerializerEndMarkerException(nameof(MongoCursor<T>), listEndMarker);
            }

            return true;
        }
        private static bool TryParseElements(ref BsonReader reader, List<T> list, ref int batchLength, ref int batchReadded, out SequencePosition position)
        {
            position = reader.Position;
            var internalList = list;

            while (batchLength - batchReadded > 1)
            {
                position = reader.Position;
                var checkpoint = reader.BytesConsumed;
                if (!reader.TryGetByte(out var listBsonType))
                {
                    return false;
                }

                if (!reader.TrySkipCString())
                {
                    return false;
                }

                if (listBsonType == 10)
                {
                    batchReadded += (int)(reader.BytesConsumed - checkpoint);
                    internalList.Add(default);
                    continue;
                }
                if (reader.TryPeekInt32(out var nextElemSize) is false || reader.Remaining < nextElemSize)
                {
                    return false;
                }
                if (!T.TryParseBson(ref reader, out T temp))
                {
                    return false;
                }
                else
                {
                    batchReadded += (int)(reader.BytesConsumed - checkpoint);

                    internalList.Add(temp);
                    continue;
                }

            }

            position = reader.Position;
            return true;
        }
    }
}
