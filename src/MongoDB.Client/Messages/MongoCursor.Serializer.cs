using System;
using MongoDB.Client.Bson.Reader;

namespace MongoDB.Client.Messages
{
    public partial class MongoCursor<T>
    {
        private static ReadOnlySpan<byte> MongoCursorid => new byte[2] { 105, 100 };
        private static ReadOnlySpan<byte> MongoCursorns => new byte[2] { 110, 115 };
        private static ReadOnlySpan<byte> MongoCursorfirstBatch => new byte[10] { 102, 105, 114, 115, 116, 66, 97, 116, 99, 104 };
        private static ReadOnlySpan<byte> MongoCursornextBatch => new byte[9] { 110, 101, 120, 116, 66, 97, 116, 99, 104 };
        public static bool TryParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, System.Collections.Generic.List<T> first, System.Collections.Generic.List<T> next, out MongoDB.Client.Messages.MongoCursor<T> message)
        {
            message = default;
            long Int64Id = default;
            string StringNamespace = default;
            System.Collections.Generic.List<T> ListFirstBatch = first;
            System.Collections.Generic.List<T> ListNextBatch = next;
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

                switch (System.Runtime.InteropServices.MemoryMarshal.GetReference(bsonName))
                {
                    case 105:
                        {
                            if (bsonName.SequenceEqual2(MongoCursorid))
                            {
                                if (!reader.TryGetInt64(out Int64Id))
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
                                if (!TryParseListT(ref reader, ListFirstBatch))
                                {
                                    return false;
                                }

                                continue;
                            }

                            break;
                        }

                    case 110:
                        {
                            if (bsonName.Length < 1)
                            {
                                break;
                            }

                            switch (bsonName[1])
                            {
                                case 115:
                                    {
                                        if (bsonName.SequenceEqual2(MongoCursorns))
                                        {
                                            if (!reader.TryGetString(out StringNamespace))
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
                                            if (!TryParseListT(ref reader, ListNextBatch))
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
                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(MongoDB.Client.Messages.MongoCursor<T>), endMarker);
            }

            message = new MongoDB.Client.Messages.MongoCursor<T>(id: Int64Id, _namespace: StringNamespace, firstBatch: ListFirstBatch, nextBatch: ListNextBatch);
            return true;
        }

        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in MongoDB.Client.Messages.MongoCursor<T> message)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            writer.Write_Type_Name_Value(MongoCursorid, message.Id);
            if (message.Namespace == null)
            {
                writer.WriteBsonNull(MongoCursorns);
            }
            else
            {
                writer.Write_Type_Name_Value(MongoCursorns, message.Namespace);
            }

            if (message.FirstBatch == null)
            {
                writer.WriteBsonNull(MongoCursorfirstBatch);
            }
            else
            {
                writer.Write_Type_Name(4, MongoCursorfirstBatch);
                WriteListT(ref writer, message.FirstBatch);
            }

            if (message.NextBatch == null)
            {
                writer.WriteBsonNull(MongoCursornextBatch);
            }
            else
            {
                writer.Write_Type_Name(4, MongoCursornextBatch);
                WriteListT(ref writer, message.NextBatch);
            }

            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            reserved.Write(docLength);
            writer.Commit();
        }

        private static bool TryParseListT(ref MongoDB.Client.Bson.Reader.BsonReader reader, System.Collections.Generic.List<T>? list)
        {
            //list = default;
            var internalList = list;
            if (!reader.TryGetInt32(out int listDocLength))
            {
                return false;
            }

            var listUnreaded = reader.Remaining + sizeof(int);
            while (listUnreaded - reader.Remaining < listDocLength - 1)
            {
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

                if (!MongoDB.Client.Messages.CursorItemSerializer.TryParseBson(ref reader, out T temp))
                {
                    return false;
                }
                else
                {
                    internalList.Add(temp);
                    continue;
                }
            }

            if (!reader.TryGetByte(out var listEndMarker))
            {
                return false;
            }

            if (listEndMarker != 0)
            {
                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(MongoDB.Client.Messages.MongoCursor<T>), listEndMarker);
            }

            list = internalList;
            return true;
        }

        private static void WriteListT(ref MongoDB.Client.Bson.Writer.BsonWriter writer, System.Collections.Generic.List<T> array)
        {
            int index = 0;
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            for (; index < array.Count; index++)
            {
                var item = array[index];
                var bsonTypeResereved30784 = writer.Reserve(1);
                writer.WriteName(index);
                MongoDB.Client.Messages.CursorItemSerializer.WriteBson(ref writer, item, out var bsonTypeTemp30785);
                bsonTypeResereved30784.WriteByte(bsonTypeTemp30785);
            }

            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            reserved.Write(docLength);
            writer.Commit();
        }
    }
}
