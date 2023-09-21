using System.Diagnostics;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Exceptions;

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
        public class CursorState
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

        public static bool TryParseBson(ref BsonReader reader, ref CursorState message, out SequencePosition position)
        {

            position = reader.Position;

            switch (message.State)
            {
                case State.Prologue:
                    position = reader.Position;
                    if (!reader.TryGetInt32(out message.DocLength))
                    {
                        return false;
                    }

                    message.DocReadded += sizeof(int);

                    goto case State.MainLoop;
                case State.MainLoop:
                    message.State = State.MainLoop;
                    position = reader.Position;
                    if (TryParseMainLoop(ref reader, ref message, out position) is false)
                    {
                        return false;
                    }

                    goto case State.Epilogue;
                case State.FirstBatch:
                    var checkpoint = message.BatchReadded;
                    position = reader.Position;
                    var isFirstBatchComplete = TryParseFirstBatch(ref reader, ref message, out position);

                    message.DocReadded += message.BatchReadded - checkpoint;

                    if (isFirstBatchComplete is false)
                    {
                        return false;
                    }

                    goto case State.MainLoop;
                case State.NextBatch:
                    checkpoint = message.BatchReadded;
                    position = reader.Position;
                    var isNextBatchComplete = TryParseNextBatch(ref reader, ref message, out position);

                    message.DocReadded += message.BatchReadded - checkpoint;

                    if (isNextBatchComplete is false)
                    {
                        return false;
                    }

                    goto case State.MainLoop;
                case State.Epilogue:
                    message.State = State.Epilogue;
                    position = reader.Position;

                    if (!reader.TryGetByte(out var endMarker))
                    {
                        return false;
                    }

                    message.DocReadded += sizeof(byte);

                    Debug.Assert(message.DocLength == message.DocReadded);

                    if (endMarker != 0)
                    {
                        throw new SerializerEndMarkerException(nameof(MongoCursor<T>), endMarker);
                    }

                    break;
                default:
                    throw new InvalidOperationException($"Undefined State value in {nameof(MongoCursor<T>)}");
            }

            position = reader.Position;

            return true;
        }


        private static bool TryParseMainLoop(ref BsonReader reader, ref CursorState message, out SequencePosition position)
        {
            var docLength = message.DocLength;
            position = reader.Position;
            while (docLength - message.DocReadded > 1)
            {
                var checkpoint = reader.BytesConsumed;
                position = reader.Position;

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

                                var beforeBatch = reader.BytesConsumed - checkpoint;

                                var isBatchComplete = TryParseFirstBatch(ref reader, ref message, out position);

                                message.DocReadded += (int)(message.BatchReadded + beforeBatch);

                                if (!isBatchComplete)
                                {
                                    return false;
                                }

                                message.State = State.MainLoop;

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

                                            var beforeBatch = reader.BytesConsumed - checkpoint;

                                            var isBatchComplete = TryParseNextBatch(ref reader, ref message, out position);

                                            message.DocReadded += (int)(message.BatchReadded + beforeBatch);

                                            if (!isBatchComplete)
                                            {
                                                return false;
                                            }
                                            
                                            message.State = State.MainLoop;

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

            position = reader.Position;

            message.State = State.Epilogue;

            return true;
        }

        private static bool TryParseFirstBatch(ref BsonReader reader, ref CursorState state, out SequencePosition position)
        {
            var internalList = state.FirstBatch;
            position = reader.Position;

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
            
            if (state.BatchLength - state.BatchReadded is 1)
            {
                goto EPILOGUE;
            }
        ELEMENTS:
            var isComplete = TryParseElements(ref reader, internalList, ref state.BatchLength, ref state.BatchReadded, out position);

            if (isComplete is false)
            {
                return false;
            }
        EPILOGUE:
            if (!reader.TryGetByte(out var listEndMarker))
            {
                return false;
            }

            state.BatchReadded += sizeof(byte);

            Debug.Assert(state.BatchLength == state.BatchReadded);

            if (listEndMarker != 0)
            {
                throw new SerializerEndMarkerException(nameof(MongoCursor<T>), listEndMarker);
            }

            position = reader.Position;

            return true;
        }
        private static bool TryParseNextBatch(ref BsonReader reader, ref CursorState state, out SequencePosition position)
        {
            var internalList = state.NextBatch;
            position = reader.Position;

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

            if (state.BatchLength - state.BatchReadded is 1)
            {
                goto EPILOGUE;
            }
        ELEMENTS:
            var isComplete = TryParseElements(ref reader, internalList, ref state.BatchLength, ref state.BatchReadded, out position);

            if (isComplete is false)
            {
                return false;
            }

        EPILOGUE:
            if (!reader.TryGetByte(out var listEndMarker))
            {
                return false;
            }

            state.BatchReadded += sizeof(byte);

            Debug.Assert(state.BatchLength == state.BatchReadded);

            if (listEndMarker != 0)
            {
                throw new SerializerEndMarkerException(nameof(MongoCursor<T>), listEndMarker);
            }

            position = reader.Position;

            return true;
        }
        private static bool TryParseElements(ref BsonReader reader, List<T> list, ref int batchLength, ref int batchReadded, out SequencePosition position)
        {
            var internalList = list;
            position = reader.Position;
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
