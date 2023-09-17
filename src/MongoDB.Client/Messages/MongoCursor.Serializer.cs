using System;
using System.Collections.Generic;
using System.Buffers.Binary;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Exceptions;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
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
            public int BatchRemaining;
            public int DocLength;
            public SequencePosition Position;

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

                    goto case State.MainLoop;
                case State.MainLoop:
                    message.State = State.MainLoop;

                    if (TryParseMainLoop(ref reader, ref message) is false)
                    {
                        return false;
                    }

                    goto case State.Epilogue;
                case State.FirstBatch:
                    var checkpoint = reader.BytesConsumed;
                    if (TryParseFirstBatch(ref reader, ref message) is false)
                    {
                        message.DocLength -= (int)(reader.BytesConsumed - checkpoint);
                        return false;
                    }
                    message.DocLength -= (int)(reader.BytesConsumed - checkpoint);
                    goto case State.MainLoop;
                case State.NextBatch:
                    if (TryParseNextBatch(ref reader, ref message))
                    {
                        return false;
                    }

                    goto case State.MainLoop;
                case State.Epilogue:
                    message.State = State.Epilogue;

                    if (!reader.TryGetByte(out var endMarker))
                    {
                        return false;
                    }
                    message.DocLength -= sizeof(byte);

                    Debug.Assert(message.DocLength is 0);

                    if (endMarker != 0)
                    {
                        throw new SerializerEndMarkerException(nameof(MongoCursor<T>), endMarker);
                    }

                    break;
                default:
                    throw new InvalidOperationException($"Undefined State value in {nameof(MongoCursor<T>)}");
            }

            return true;
        }


        private static bool TryParseMainLoop(ref BsonReader reader, ref CursorState message)
        {
            while (message.DocLength > 1)
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
                    message.DocLength -= (int)(reader.BytesConsumed - checkpoint);
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

                                message.DocLength -= (int)(reader.BytesConsumed - checkpoint);
                                continue;
                            }

                            break;
                        }

                    case 102:
                        {
                            if (bsonName.SequenceEqual9(MongoCursorfirstBatch))
                            {
                                message.FirstBatch = new();

                                if (!TryParseFirstBatch(ref reader, ref message))
                                {
                                    message.DocLength -= (int)(reader.BytesConsumed - checkpoint);
                                    return false;
                                }
                                message.DocLength -= (int)(reader.BytesConsumed - checkpoint);
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

                                            message.DocLength -= (int)(reader.BytesConsumed - checkpoint);
                                            continue;
                                        }

                                        break;
                                    }

                                case 101:
                                    {
                                        if (bsonName.SequenceEqual9(MongoCursornextBatch))
                                        {
                                            message.NextBatch = new();

                                            if (!TryParseNextBatch(ref reader, ref message))
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
                    message.DocLength -= (int)(reader.BytesConsumed - checkpoint);
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

            if (!reader.TryGetInt32(out state.BatchRemaining))
            {
                return false;
            }
            state.State = State.FirstBatch;
            state.BatchRemaining -= sizeof(int);
        ELEMENTS:
            var checkpoint = reader.BytesConsumed;

            var isComplete = TryParseElements(ref reader, internalList, ref state.BatchRemaining, out state.Position);

            if (isComplete is false)
            {
                return false;
            }

            if (!reader.TryGetByte(out var listEndMarker))
            {
                return false;
            }

            state.BatchRemaining -= 1;

            Debug.Assert(state.BatchRemaining is 0);

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

            if (!reader.TryGetInt32(out state.BatchRemaining))
            {
                return false;
            }
            state.State = State.NextBatch;
            state.BatchRemaining -= sizeof(int);
            state.DocLength -= sizeof(int);
        ELEMENTS:
            var checkpoint = reader.BytesConsumed;

            var isComplete = TryParseElements(ref reader, internalList, ref state.BatchRemaining, out state.Position);

            state.DocLength -= (int)(reader.BytesConsumed - checkpoint);

            if (isComplete is false)
            {

                return false;
            }

            if (!reader.TryGetByte(out var listEndMarker))
            {
                return false;
            }

            state.BatchRemaining -= 1;
            state.DocLength -= 1;

            Debug.Assert(state.BatchRemaining is 0);

            if (listEndMarker != 0)
            {
                throw new SerializerEndMarkerException(nameof(MongoCursor<T>), listEndMarker);
            }

            return true;
        }
        private static bool TryParseElements(ref BsonReader reader, List<T> list, ref int listRemaining, out SequencePosition position)
        {
            position = reader.Position;
            var internalList = list;

            while (listRemaining > 1)
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
                    listRemaining -= (int)(reader.BytesConsumed - checkpoint);
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
                    listRemaining -= (int)(reader.BytesConsumed - checkpoint);

                    internalList.Add(temp);
                    continue;
                }

            }

            position = reader.Position;
            return true;
        }
    }
}
