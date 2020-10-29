using MongoDB.Client.Bson.Document;
using System;
using System.Buffers.Binary;

namespace MongoDB.Client.Bson.Writer
{
    public ref partial struct BsonWriter
    {
        public void WriteElement(in BsonElement element)
        {
            WriteByte((byte)element.Type);
            WriteCString(element.Name);
            switch ((byte)element.Type)
            {
                case 1:
                    {
                        WriteDouble((double)element.Value);
                        break;
                    }
                case 2:
                    {
                        WriteString((string)element.Value);
                        break;
                    }
                case 3:
                    {
                        WriteDocument((BsonDocument)element.Value);
                        break;
                    }
                case 4:
                    {
                        WriteDocument((BsonDocument)element.Value);
                        break;
                    }
                case 5:
                    {
                        // write binary data
                        break;
                    }
                case 7:
                    {
                        WriteObjectId((BsonObjectId)element.Value);
                        break;
                    }
                case 8:
                    {
                        WriteBoolean((bool)element.Value);
                        break;
                    }
                case 9:
                    {
                        WriteUTCDateTime((DateTimeOffset)element.Value);
                        break;
                    }
                case 10:
                    {
                        break;
                    }
                case 16:
                    {
                        WriteInt32((int)element.Value);
                        break;
                    }
                case 18:
                    {
                        WriteInt64((long)element.Value);
                        break;
                    }
                default:
                    {
                        throw new ArgumentException($"{nameof(BsonWriter)}.{nameof(WriteElement)}  with type {(byte)element.Type}");
                    }
            }
        }

        public void WriteDocument(BsonDocument document)
        {
            var docStartPoint = _written;
            var reserved = Reserve(4); 
            for (var i = 0; i < document.Count; i++)
            {
                WriteElement(document[i]);
            }
            WriteByte(EndMarker);
            var docLength = _written - docStartPoint;
            Span<byte> sizeSpan = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(sizeSpan, docLength);
            reserved.Write(sizeSpan);
            Commit();
        }
    }
}
