//using System;
//using MongoDB.Client.Bson.Reader;

//namespace MongoDB.Client.Messages
//{
//    public struct MongoCursorState<T>
//    {
//        public struct ListTState
//        {
//            public int? ListTDocLen;
//            public long Consumed;
//            public bool EndMarker;
//            public System.Collections.Generic.List<T> list;
//        };
//        public int? DocLen;
//        public long Consumed;
//        public long Int64Id;
//        public string StringNamespace;
//        public ListTState ListFirstBatch;
//        public bool InListFirstBatch;
//        public ListTState ListNextBatch;
//        public bool InListNextBatch;
//        public MongoDB.Client.Messages.MongoCursor<T> message;
//        public bool EndMarker;
//    }
//    public partial class MongoCursor<T>
//    {
//        private static ReadOnlySpan<byte> MongoCursorid => new byte[2] { 105, 100 };
//        private static ReadOnlySpan<byte> MongoCursorns => new byte[2] { 110, 115 };
//        private static ReadOnlySpan<byte> MongoCursorfirstBatch => new byte[10] { 102, 105, 114, 115, 116, 66, 97, 116, 99, 104 };
//        private static ReadOnlySpan<byte> MongoCursornextBatch => new byte[9] { 110, 101, 120, 116, 66, 97, 116, 99, 104 };
//        public static bool TryParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, out MongoCursorState<T> state, out SequencePosition position)
//        {
//            state = new MongoCursorState<T>();
//            state.EndMarker = false;
//            position = reader.Position;
//            var startCheckpoint = reader.BytesConsumed;
//            state.EndMarker = false;
//            position = reader.Position;
//            if (!reader.TryGetInt32(out int docLength))
//            {
//                return false;
//            }
//            state.DocLen = docLength;
//            var loopCheckpoint = reader.Remaining;
//            //while (startCheckpoint - reader.Remaining < docLength - 1)
//            while (reader.BytesConsumed - startCheckpoint < docLength - 1)
//            {
//                position = reader.Position;
//                loopCheckpoint = reader.BytesConsumed;
//                if (!reader.TryGetByte(out var bsonType))
//                {
//                    state.Consumed = loopCheckpoint - startCheckpoint;
//                    return false;
//                }

//                if (!reader.TryGetCStringAsSpan(out var bsonName))
//                {
//                    state.Consumed = loopCheckpoint - startCheckpoint;
//                    return false;
//                }

//                if (bsonType == 10)
//                {
//                    continue;
//                }

//                switch (System.Runtime.InteropServices.MemoryMarshal.GetReference(bsonName))
//                {
//                    case 105:
//                        {
//                            if (bsonName.SequenceEqual2(MongoCursorid))
//                            {
//                                if (!reader.TryGetInt64(out state.Int64Id))
//                                {
//                                    state.Consumed = loopCheckpoint - startCheckpoint;
//                                    return false;
//                                }

//                                continue;
//                            }

//                            break;
//                        }

//                    case 102:
//                        {
//                            if (bsonName.SequenceEqual9(MongoCursorfirstBatch))
//                            {
//                                state.ListFirstBatch.list = new();
//                                state.ListFirstBatch.EndMarker = false;
//                                if (!TryParseListT(ref reader, ref state.ListFirstBatch, out position))
//                                {
//                                    state.Consumed = reader.BytesConsumed - startCheckpoint;
//                                    state.InListFirstBatch = true;
//                                    return false;
//                                }

//                                continue;
//                            }

//                            break;
//                        }

//                    case 110:
//                        {
//                            if (bsonName.Length < 1)
//                            {
//                                break;
//                            }

//                            switch (bsonName[1])
//                            {
//                                case 115:
//                                    {
//                                        if (bsonName.SequenceEqual2(MongoCursorns))
//                                        {
//                                            if (!reader.TryGetString(out state.StringNamespace))
//                                            {
//                                                state.Consumed = loopCheckpoint - startCheckpoint;
//                                                return false;
//                                            }

//                                            continue;
//                                        }

//                                        break;
//                                    }

//                                case 101:
//                                    {
//                                        if (bsonName.SequenceEqual9(MongoCursornextBatch))
//                                        {
//                                            state.Consumed = loopCheckpoint - startCheckpoint;
//                                            state.ListNextBatch.list = new();
//                                            state.ListNextBatch.EndMarker = false;
//                                            if (!TryParseListT(ref reader, ref state.ListNextBatch, out position))
//                                            {
//                                                state.InListNextBatch = true;
//                                                return false;
//                                            }

//                                            continue;
//                                        }

//                                        break;
//                                    }
//                            }

//                            break;
//                        }
//                }

//                if (!reader.TrySkip(bsonType))
//                {
//                    state.Consumed = loopCheckpoint - startCheckpoint;
//                    return false;
//                }
//            }

//            if (!reader.TryGetByte(out var endMarker))
//            {
//                state.Consumed = startCheckpoint - position.GetInteger();
//                position = reader.Position;
//                state.EndMarker = true;
//                return false;
//            }

//            if (endMarker != 0)
//            {
//                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(MongoDB.Client.Messages.MongoCursor<T>), endMarker);
//            }
//            state.Consumed = startCheckpoint - loopCheckpoint + 1;
//            position = reader.Position;  
//            state.message = new MongoDB.Client.Messages.MongoCursor<T>(id: state.Int64Id, _namespace: state.StringNamespace, firstBatch: state.ListFirstBatch.list, nextBatch: state.ListNextBatch.list);
//            return true;
//        }
//        public static bool TryContinueParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, ref MongoCursorState<T> state, out SequencePosition position)
//        {
//            position = reader.Position;
//            var startCheckpoint = reader.BytesConsumed;
//            var consumed = state.Consumed;
//            long localConsumed = 0;
//            if (state.EndMarker)
//            {
//                goto END_MARKER;
//            }
//            if(state.InListFirstBatch)
//            {
//                if (!TryParseListT(ref reader, ref state.ListFirstBatch, out position))
//                {
//                    state.InListFirstBatch = true;
//                    return false;
//                }
//                else
//                {
//                    localConsumed = reader.BytesConsumed - startCheckpoint;
//                    state.InListFirstBatch = false;
//                }
//            }
//            if (state.InListNextBatch)
//            {
//                if (!TryParseListT(ref reader, ref state.ListNextBatch, out position))
//                {
//                    state.InListNextBatch = true;
//                    return false;
//                }
//                else
//                {
//                    localConsumed = reader.BytesConsumed - startCheckpoint;
//                    state.InListNextBatch = false;
//                }
//            }
//            var docLength = state.DocLen;
//            consumed = state.Consumed;
//            while (consumed + localConsumed < docLength - 1)
//            {
//                position = reader.Position;
//                var loopCheckpoint = reader.BytesConsumed;
//                if (!reader.TryGetByte(out var bsonType))
//                {
//                    state.Consumed = loopCheckpoint - startCheckpoint;
//                    return false;
//                }

//                if (!reader.TryGetCStringAsSpan(out var bsonName))
//                {
//                    state.Consumed = loopCheckpoint - startCheckpoint;
//                    return false;
//                }

//                if (bsonType == 10)
//                {
//                    continue;
//                }

//                switch (System.Runtime.InteropServices.MemoryMarshal.GetReference(bsonName))
//                {
//                    case 105:
//                        {
//                            if (bsonName.SequenceEqual2(MongoCursorid))
//                            {
//                                if (!reader.TryGetInt64(out state.Int64Id))
//                                {
//                                    state.Consumed = loopCheckpoint - startCheckpoint;
//                                    return false;
//                                }
//                                localConsumed = reader.BytesConsumed - startCheckpoint;
//                                continue;
//                            }

//                            break;
//                        }

//                    case 102:
//                        {
//                            if (bsonName.SequenceEqual9(MongoCursorfirstBatch))
//                            {
//                                if (!TryParseListT(ref reader, ref state.ListFirstBatch, out position))
//                                {
//                                    state.Consumed = loopCheckpoint - startCheckpoint;
//                                    state.InListFirstBatch = true;
//                                    return false;
//                                }
//                                localConsumed = reader.BytesConsumed - startCheckpoint;
//                                continue;
//                            }

//                            break;
//                        }

//                    case 110:
//                        {
//                            if (bsonName.Length < 1)
//                            {
//                                break;
//                            }

//                            switch (bsonName[1])
//                            {
//                                case 115:
//                                    {
//                                        if (bsonName.SequenceEqual2(MongoCursorns))
//                                        {
//                                            if (!reader.TryGetString(out state.StringNamespace))
//                                            {
//                                                state.Consumed = loopCheckpoint - startCheckpoint;
//                                                return false;
//                                            }
//                                            localConsumed = reader.BytesConsumed - startCheckpoint;
//                                            continue;
//                                        }

//                                        break;
//                                    }

//                                case 101:
//                                    {
//                                        if (bsonName.SequenceEqual9(MongoCursornextBatch))
//                                        {
//                                            if (!TryParseListT(ref reader, ref state.ListNextBatch, out position))
//                                            {
//                                                state.Consumed = loopCheckpoint - startCheckpoint;
//                                                state.InListNextBatch = true;
//                                                return false;
//                                            }
//                                            localConsumed = reader.BytesConsumed - startCheckpoint;
//                                            continue;
//                                        }

//                                        break;
//                                    }
//                            }

//                            break;
//                        }
//                }

//                if (!reader.TrySkip(bsonType))
//                {
//                    state.Consumed = loopCheckpoint - startCheckpoint;
//                    return false;
//                }
//            }
//            END_MARKER:
//            position = reader.Position;
//            if (!reader.TryGetByte(out var endMarker))
//            {
//                state.Consumed = startCheckpoint - position.GetInteger();
//                state.EndMarker = true;
//                return false;
//            }
            
//            if (endMarker != 0)
//            {
//                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(MongoDB.Client.Messages.MongoCursor<T>), endMarker);
//            }
//            position = reader.Position;
//            state.Consumed = consumed + localConsumed + 1;
//            state.message = new MongoDB.Client.Messages.MongoCursor<T>(id: state.Int64Id, _namespace: state.StringNamespace, firstBatch: state.ListFirstBatch.list, nextBatch: state.ListNextBatch.list);
//            return true;
//        }
//        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in MongoDB.Client.Messages.MongoCursor<T> message)
//        {
//            var checkpoint = writer.Written;
//            var reserved = writer.Reserve(4);
//            writer.Write_Type_Name_Value(MongoCursorid, message.Id);
//            if (message.Namespace == null)
//            {
//                writer.WriteBsonNull(MongoCursorns);
//            }
//            else
//            {
//                writer.Write_Type_Name_Value(MongoCursorns, message.Namespace);
//            }

//            if (message.FirstBatch == null)
//            {
//                writer.WriteBsonNull(MongoCursorfirstBatch);
//            }
//            else
//            {
//                writer.Write_Type_Name(4, MongoCursorfirstBatch);
//                WriteListT(ref writer, message.FirstBatch);
//            }

//            if (message.NextBatch == null)
//            {
//                writer.WriteBsonNull(MongoCursornextBatch);
//            }
//            else
//            {
//                writer.Write_Type_Name(4, MongoCursornextBatch);
//                WriteListT(ref writer, message.NextBatch);
//            }

//            writer.WriteByte(0);
//            var docLength = writer.Written - checkpoint;
//            reserved.Write(docLength);
//            writer.Commit();
//        }

//        private static bool TryParseListT(ref MongoDB.Client.Bson.Reader.BsonReader reader, ref MongoCursorState<T>.ListTState state, out SequencePosition position)
//        {
//            position = reader.Position;
//            var checkpoint = reader.Remaining;
//            long consumed = state.Consumed;
//            long localConsumed = 0;
//            if (state.EndMarker)
//            {
//                goto END_MARKER;
//            }
//            if(state.ListTDocLen.HasValue)
//            {
//                goto START_LOOP;
//            }
//            if (!reader.TryGetInt32(out state.ListTDocLen))
//            {
//                return false;
//            }

//        START_LOOP:
//            var listDocLength = state.ListTDocLen;
//            var list = state.list;
//            while (consumed + localConsumed < listDocLength - 1)
//            {
//                position = reader.Position;
                
//                if (!reader.TryGetByte(out var listBsonType))
//                {
//                    state.Consumed = consumed;
//                    return false;
//                }

//                if (!reader.TrySkipCString())
//                {
//                    state.Consumed = consumed + localConsumed;
//                    return false;
//                }

//                if (listBsonType == 10)
//                {
//                    list.Add(default!);
//                    state.Consumed = consumed + localConsumed;
//                    continue;
//                }

//                if (!MongoDB.Client.Messages.CursorItemSerializer.TryParseBson(ref reader, out T temp))
//                {
//                    state.Consumed = consumed + localConsumed;
//                    return false;
//                }
//                else
//                {
//                    list.Add(temp);
//                    localConsumed = checkpoint - reader.Remaining;
//                    continue;
//                }
//            }
//        END_MARKER:
//            position = reader.Position;
//            if (!reader.TryGetByte(out var listEndMarker))
//            {
//                state.EndMarker = true;
//                return false;
//            }            
//            if (listEndMarker != 0)
//            {
//                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(MongoDB.Client.Messages.MongoCursor<T>), listEndMarker);
//            }
//            state.Consumed = localConsumed + consumed + 1;
//            position = reader.Position;
//            return true;
//        }

//        private static void WriteListT(ref MongoDB.Client.Bson.Writer.BsonWriter writer, System.Collections.Generic.List<T> array)
//        {
//            int index = 0;
//            var checkpoint = writer.Written;
//            var reserved = writer.Reserve(4);
//            for (; index < array.Count; index++)
//            {
//                var item = array[index];
//                var bsonTypeResereved30784 = writer.Reserve(1);
//                writer.WriteName(index);
//                MongoDB.Client.Messages.CursorItemSerializer.WriteBson(ref writer, item, out var bsonTypeTemp30785);
//                bsonTypeResereved30784.WriteByte(bsonTypeTemp30785);
//            }

//            writer.WriteByte(0);
//            var docLength = writer.Written - checkpoint;
//            reserved.Write(docLength);
//            writer.Commit();
//        }
//    }
//}
