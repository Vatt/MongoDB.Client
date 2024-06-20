using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Utils;

namespace MongoDB.Client.Bson.Writer
{
    public ref partial struct BsonWriter
    {
        public void WriteElement(in BsonElement element)
        {
            WriteBsonType(element.Type);
            WriteCString(element.Name);
            switch ((byte)element.Type)
            {
                case 1:
                    {
                        WriteDouble((double)element.Value!);
                        break;
                    }
                case 2:
                    {
                        WriteString((string)element.Value!);
                        break;
                    }
                case 3:
                    {
                        WriteDocument((BsonDocument)element.Value!);
                        break;
                    }
                case 4:
                    {
                        WriteDocument((BsonDocument)element.Value!);
                        break;
                    }
                case 5:
                    {
                        WriteBinaryData((BsonBinaryData)element.Value!);
                        break;
                    }
                case 7:
                    {
                        WriteObjectId((BsonObjectId)element.Value!);
                        break;
                    }
                case 8:
                    {
                        WriteBoolean((bool)element.Value!);
                        break;
                    }
                case 9:
                    {
                        WriteUtcDateTime((DateTimeOffset)element.Value!);
                        break;
                    }
                case 10:
                    {
                        break;
                    }
                case 16:
                    {
                        WriteInt32((int)element.Value!);
                        break;
                    }
                case 17:
                    {
                        WriteTimestamp((BsonTimestamp)element.Value!);
                        break;
                    }
                case 18:
                    {
                        WriteInt64((long)element.Value!);
                        break;
                    }
                case 19:
                    {
                        WriteDecimal((decimal)element.Value!); 
                        break;
                    }
                default:
                    {
                        throw new ArgumentException($"{nameof(BsonWriter)}.{nameof(WriteElement)}  with type {(byte)element.Type}");
                    }
            }
        }

        public void WriteBinaryData(BsonBinaryData value)
        {
            switch (value.Type)
            {
                case BsonBinaryDataType.UUID:
                    const int guidSize = 16;
                    WriteInt32(guidSize);
                    WriteByte((byte)value.Type);
                    WriteGuidAsBytes((Guid)value.Value);
                    break;
                case BsonBinaryDataType.Generic:
                    var span = ((byte[])value.Value).AsSpan();
                    WriteInt32(span.Length);
                    WriteByte((byte)value.Type);
                    WriteBytes(span);
                    break;
                case BsonBinaryDataType.BinaryOld:
                case BsonBinaryDataType.UUIDOld:
                case BsonBinaryDataType.MD5:
                case BsonBinaryDataType.EncryptedBSONValue:
                case BsonBinaryDataType.UserDefined:
                default:
                    ThrowHelper.NotImplementedException<bool>(value.Type.ToString());
                    break;
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
            reserved.Write(docLength);

            Commit();
        }
    }
}
