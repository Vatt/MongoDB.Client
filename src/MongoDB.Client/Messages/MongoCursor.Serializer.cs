using System;
using System.Collections.Generic;
using System.Buffers.Binary;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Exceptions;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

#nullable disable

namespace MongoDB.Client.Messages
{
    public partial class MongoCursor<T>
    {
        public enum State
        {
            MainLoop,
            FirstBatch,
            NextBatch
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
                State = State.MainLoop;
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
                case State.MainLoop:
                    return TryParseMainLoop(ref reader, ref message);
                case State.FirstBatch:
                    return TryParseFirstBatch(ref reader, ref message);
                case State.NextBatch:
                    return TryParseNextBatch(ref reader, ref message);
                default:
                    throw new InvalidOperationException($"Undefined State value in {nameof(MongoCursor<T>)}");
            }
        }


        private static bool TryParseMainLoop(ref BsonReader reader, ref CursorState message)
        {
            if (!reader.TryGetInt32(out message.DocLength))
            {
                return false;
            }

            var unreaded = reader.Remaining + sizeof(int);
            var docLength = message.DocLength;
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
                    case 105:
                        {
                            if (bsonName.SequenceEqual2(MongoCursorid))
                            {
                                if (!reader.TryGetInt64(out message.Id))
                                {
                                    return false;
                                }

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
            }

            if (!reader.TryGetByte(out var endMarker))
            {
                return false;
            }

            if (endMarker != 0)
            {
                throw new SerializerEndMarkerException(nameof(MongoCursor<T>), endMarker);
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
            var checkpoint = reader.Position.GetInteger();

            if (TryParseElements(ref reader, internalList, state.BatchRemaining, out state.Position) is false)
            {
                return false;
            }

            state.BatchRemaining -= reader.Position.GetInteger() - checkpoint;

            if (!reader.TryGetByte(out var listEndMarker))
            {
                return false;
            }

            state.BatchRemaining = -1;

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
        ELEMENTS:
            var checkpoint = reader.Position.GetInteger();

            if (TryParseElements(ref reader, internalList, state.BatchRemaining, out state.Position) is false)
            {
                return false;
            }

            state.BatchRemaining -= reader.Position.GetInteger() - checkpoint;

            if (!reader.TryGetByte(out var listEndMarker))
            {
                return false;
            }

            state.BatchRemaining = -1;

            Debug.Assert(state.BatchRemaining is 0);

            if (listEndMarker != 0)
            {
                throw new SerializerEndMarkerException(nameof(MongoCursor<T>), listEndMarker);
            }

            return true;
        }
        private static bool TryParseElements(ref BsonReader reader, List<T> list, int listRemaining, out SequencePosition position)
        {
            position = reader.Position;
            var internalList = list;

            var listUnreaded = reader.Remaining + sizeof(int);
            while (listUnreaded - reader.Remaining < listRemaining - 1)
            {
                position = reader.Position;

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
                    internalList.Add(default);
                    continue;
                }

                if (!reader.TryReadGeneric(listBsonType, out T temp))
                {
                    return false;
                }
                else
                {
                    internalList.Add(temp);
                    continue;
                }
            }

            position = reader.Position;
            return true;
        }
    }
}
