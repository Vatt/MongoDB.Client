using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Utils;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MongoDB.Client.Bson.Reader
{
    public ref partial struct BsonReader
    {
        public bool TryGetArray([NotNullWhen(true)] out BsonDocument? value)
        {
            value = default;
            var root = new BsonDocument();
            if (!TryGetInt32(out var docLength)) { return false; }
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


        public bool TryGetArrayAsDocumentList([NotNullWhen(true)] out List<BsonDocument>? value)
        {
            value = new List<BsonDocument>();
            if (!TryGetInt32(out var docLength)) { return false; }
            var unreaded = _input.Remaining + sizeof(int);
            while (unreaded - _input.Remaining < docLength - 1)
            {
                if (!TryGetByte(out var type)) { return false; }
                if (!TryGetCString(out var name)) { return false; }
                if (!TryParseDocument(null, out var element)) { return false; }
                value.Add(element);
            }
            TryGetByte(out var endDocumentMarker);
            if (endDocumentMarker != '\x00')
            {
                return ThrowHelper.MissedDocumentEndMarkerException<bool>();
            }
            return true;
        }


        public bool TryParseElement(BsonDocument parent, out BsonElement element)
        {
            element = default;
            if (!TryGetByte(out var type)) { return false; }
            if (!TryGetCString(out var name)) { return false; }
            switch (type)
            {
                case 1:
                    {
                        if (!TryGetDouble(out var doubleVal)) { return false; }
                        element = BsonElement.Create(parent, name, doubleVal);
                        return true;
                    }
                case 2:
                    {
                        if (!TryGetString(out var stringValue)) { return false; }
                        element = BsonElement.Create(parent, name, stringValue);
                        return true;
                    }
                case 3:
                    {
                        if (!TryParseDocument(parent, out var docValue)) { return false; }
                        element = BsonElement.Create(parent, name, docValue);
                        return true;
                    }
                case 4:
                    {
                        if (!TryGetArray(out var arrayDoc)) { return false; }
                        element = BsonElement.CreateArray(parent, name, arrayDoc);
                        return true;
                    }
                case 5:
                    {
                        if (!TryGetBinaryData(out var binary)) { return false; }
                        element = BsonElement.Create(parent, name, binary);
                        return true;
                    }
                case 7:
                    {
                        if (!TryGetObjectId(out var objectId)) { return false; }
                        element = BsonElement.Create(parent, name, objectId);
                        return true;
                    }
                case 8:
                    {
                        if (!TryGetBoolean(out var boolValue)) { return false; }
                        element = BsonElement.Create(parent, name, boolValue);
                        return true;
                    }
                case 9:
                    {
                        if (!TryGetUTCDatetime(out var datetime)) { return false; }
                        element = BsonElement.Create(parent, name, datetime);
                        return true;
                    }
                case 10:
                    {
                        element = BsonElement.Create(parent, name);
                        return true;
                    }
                case 16:
                    {
                        if (!TryGetInt32(out var intValue)) { return false; }
                        element = BsonElement.Create(parent, name, intValue);
                        return true;
                    }
                case 18:
                    {
                        if (!TryGetInt64(out var longValue)) { return false; }
                        element = BsonElement.Create(parent, name, longValue);
                        return true;
                    }
                default:
                    {
                        return ThrowHelper.UnknownTypeException<bool>(type);
                    }
            }
        }


        public bool TryParseDocument(BsonDocument? parent, out BsonDocument document)
        {
            document = new BsonDocument();
            if (!TryGetInt32(out var docLength)) { return false; }
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
