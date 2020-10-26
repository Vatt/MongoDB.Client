using MongoDB.Client.Bson.Document;
using System;
using System.Buffers.Binary;

namespace MongoDB.Client.Bson.Writer
{
    public ref partial struct BsonWriter
    {
        public void WriteElement(BsonElement element)
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
                case 7:
                    {
                        WriteObjectId((BsonObjectId)element.Value);
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
            var reserved = Reserve(4);
            var checkpoint = _written;
            for (var i = 0; i < document.Elements.Count; i++)
            {
                WriteElement(document.Elements[i]);
            }
            WriteByte(EndMarker);
            Span<byte> sizeSpan = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(sizeSpan, _written - checkpoint);
            reserved.Write(sizeSpan);
        }
    }
}
