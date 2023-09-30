using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Utils;

namespace MongoDB.Client.Bson.Reader
{
    public ref partial struct BsonReader
    {
        public bool TryGetArray([NotNullWhen(true)] out BsonArray? value)
        {
            value = default;
            var root = new BsonArray();
            if (!TryGetInt32(out int docLength)) { return false; }
            var unreaded = _input.Remaining + sizeof(int);
            while (unreaded - _input.Remaining < docLength - 1)
            {
                if (!TryParseElement(root, out var element)) { return false; }
                root.Add(element);
            }
            TryGetByte(out var endDocumentMarker);
            if (endDocumentMarker != '\x00')
            {
                return ThrowHelper.MissedDocumentEndMarkerException<bool>();
            }
            value = root;
            return true;
        }
        public bool TryParseElement(BsonDocument parent, out BsonElement element)
        {
            element = default;
            if (!TryGetBsonType(out var type)) { return false; }
            if (!TryGetCString(out var name)) { return false; }
            switch (type)
            {
                case BsonType.Double:
                    {
                        if (!TryGetDouble(out double doubleVal)) { return false; }
                        element = BsonElement.Create(parent, name, doubleVal);
                        return true;
                    }
                case BsonType.String:
                    {
                        if (!TryGetString(out var stringValue)) { return false; }
                        element = BsonElement.Create(parent, name, stringValue);
                        return true;
                    }
                case BsonType.Document:
                    {
                        if (!TryParseDocument(parent, out var docValue)) { return false; }
                        element = BsonElement.Create(parent, name, docValue);
                        return true;
                    }
                case BsonType.Array:
                    {
                        if (!TryGetArray(out var arrayDoc)) { return false; }
                        element = BsonElement.CreateArray(parent, name, arrayDoc);
                        return true;
                    }
                case BsonType.BinaryData:
                    {
                        if (!TryGetBinaryData(out BsonBinaryData binary)) { return false; }
                        element = BsonElement.Create(parent, name, binary);
                        return true;
                    }
                case BsonType.ObjectId:
                    {
                        if (!TryGetObjectId(out BsonObjectId objectId)) { return false; }
                        element = BsonElement.Create(parent, name, objectId);
                        return true;
                    }
                case BsonType.Boolean:
                    {
                        if (!TryGetBoolean(out bool boolValue)) { return false; }
                        element = BsonElement.Create(parent, name, boolValue);
                        return true;
                    }
                case BsonType.UtcDateTime:
                    {
                        if (!TryGetUtcDatetime(out DateTimeOffset datetime)) { return false; }
                        element = BsonElement.Create(parent, name, datetime);
                        return true;
                    }
                case BsonType.Null:
                    {
                        element = BsonElement.Create(parent, name);
                        return true;
                    }
                case BsonType.Int32:
                    {
                        if (!TryGetInt32(out int intValue)) { return false; }
                        element = BsonElement.Create(parent, name, intValue);
                        return true;
                    }
                case BsonType.Timestamp:
                    {
                        if (!TryGetInt64(out long timestampValue)) { return false; }
                        element = BsonElement.Create(parent, name, new BsonTimestamp(timestampValue));
                        return true;
                    }
                case BsonType.Int64:
                    {
                        if (!TryGetInt64(out long longValue)) { return false; }
                        element = BsonElement.Create(parent, name, longValue);
                        return true;
                    }
                case BsonType.Decimal:
                    {
                        if (!TryGetDecimal(out decimal decimalValue)) { return false; }
                        element = BsonElement.Create(parent, name, decimalValue);
                        return true;
                    }
                default:
                    {
                        return ThrowHelper.UnknownTypeException<bool>((int)type);
                    }
            }
        }


        public bool TryParseDocument(BsonDocument? parent, out BsonDocument document)
        {
            document = new BsonDocument();
            if (!TryGetInt32(out int docLength)) { return false; }
            var unreaded = _input.Remaining + sizeof(int);
            while (unreaded - _input.Remaining < docLength - 1)
            {
                if (!TryParseElement(document, out var element)) { return false; }
                document.Add(element);
            }
            if (!TryGetByte(out var endDocumentMarker)) { return false; }
            if (endDocumentMarker != '\x00')
            {
                return ThrowHelper.MissedDocumentEndMarkerException<bool>();
            }

            return true;
        }

        public bool TryParseDocument(out BsonDocument document)
        {
            if (TryParseDocument(null, out document))
            {
                return true;
            }
            return false;
        }
    }
}
