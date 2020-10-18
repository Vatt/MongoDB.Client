using MongoDB.Client.Bson.Document;
using System;
using System.Buffers;
using System.Text;

namespace MongoDB.Client.Bson.Reader
{
    public ref struct MongoDBBsonReader
    {
        private SequenceReader<byte> _input;

        public long BytesConsumed => _input.Consumed;
        public readonly ReadOnlySpan<byte> UnreadSpan => _input.UnreadSpan;
        public MongoDBBsonReader(ReadOnlyMemory<byte> memory)
        {
            var ros = new ReadOnlySequence<byte>(memory);
            _input = new SequenceReader<byte>(ros);
        }
        public MongoDBBsonReader(ReadOnlySequence<byte> sequence)
        {
            _input = new SequenceReader<byte>(sequence);
        }
        public bool TryGetByte(out byte value)
        {
            return _input.TryRead(out value);
        }
        public bool TryGetInt16(out short value)
        {
            return _input.TryReadLittleEndian(out value);
        }
        public bool TryGetInt32(out int value)
        {
            return _input.TryReadLittleEndian(out value);
        }
        public bool TryGetInt64(out long value)
        {
            return _input.TryReadLittleEndian(out value);
        }
        public bool TryGetCString(out string value)
        {
            value = default;
            if (!_input.TryReadTo(out ReadOnlySpan<byte> data, (byte)'\x00'))
            {
                return false;
            }
            value = Encoding.UTF8.GetString(data);
            return true;
        }
        public bool TryGetDouble(out double value)
        {
            value = default;
            if(!TryGetInt64(out var temp)) { return false; }
            value =  BitConverter.Int64BitsToDouble(temp);
            return true;
            
        }
        public bool TryGetString(out string value)
        {
            value = default;
            if(!TryGetInt32(out var length)){ return false; }
            if (_input.UnreadSpan.Length < length)
            {
                return false;
            }
            var data = _input.UnreadSpan.Slice(0, length - 1);
            _input.Advance(length);
            value = Encoding.UTF8.GetString(data);
            return true;
        }
        public bool TryGetObjectId(out BsonObjectId value)
        {
            value = default;
            if (_input.UnreadSpan.Length < 12)
            {
                return false;
            }
            TryGetInt32(out var p1);
            TryGetInt32(out var p2);
            TryGetInt32(out var p3);
            value = new BsonObjectId(p1, p2, p3);
            return true;

        }
        public bool TryGetBinaryData(out BsonBinaryData value)
        {
            value = default;
            if(!TryGetInt32(out var len)) { return false; }
            if(!TryGetByte(out var subtype)) { return false; }
            if (UnreadSpan.Length < len) { return false; }
            switch (subtype)
            {
                case 4:
                    {
                        value = BsonBinaryData.Create(new Guid(UnreadSpan.Slice(0, len)));
                        _input.Advance(len);
                        return true;
                    }
                default:
                    {
                        throw new ArgumentException($"{nameof(MongoDBBsonReader)}.{nameof(TryGetBinaryData)}  with subtype {subtype}");
                    }
            }

        }
        public bool TryGetUTCDatetime(out DateTimeOffset value)
        {
            value = default;
            if(!TryGetInt64(out var data)) { return false; }
            value = DateTimeOffset.FromUnixTimeMilliseconds(data);
            return true;
        }
        public bool TryGetBoolean(out bool value)
        {
            value = default;
            if(!TryGetByte(out var boolean)) { return false; }
            value = boolean == 1 ? true:false;
            return true;
        }
        public bool TryGetArray(out byte[] value)
        {
            value = default;
            return false;
        }
        public bool TryParseElement(BsonDocument parent, out BsonElement element)
        {
            element = default;
            if(!TryGetByte(out var type)) { return false; }
            if(!TryGetCString(out var name)) { return false; }
            switch (type)
            {
                case 1:
                    {
                        if(!TryGetDouble(out var doubleVal)) { return false; }
                        element = BsonElement.Create(parent, name, doubleVal);
                        return true;
                    }
                case 2:
                    {
                        if(!TryGetString(out var stringValue)) { return false; }
                        element = BsonElement.Create(parent, name, stringValue);
                        return true;
                    }
                case 3:
                    {
                        if (!TryParseDocument(parent, out var docValue)) { return false; }
                        element = BsonElement.Create(parent, name, docValue);
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
                        throw new ArgumentException($"{nameof(MongoDBBsonReader)}.{nameof(TryParseElement)}  with type {type}");
                    }
            }
        }
        public bool TryParseDocument(BsonDocument parent, out BsonDocument document)
        {
            document = new BsonDocument();
            if(!TryGetInt32(out var docLength)) { return false; }
            var unreaded = _input.Remaining + sizeof(int);
            while(unreaded - _input.Remaining  < docLength - 1)
            {
                TryParseElement(document, out var element);
                document.Elements.Add(element);
            }
            TryGetByte(out var endDocumentMarker);
            if (endDocumentMarker != '\x00')
            {
                throw new ArgumentException($"{nameof(MongoDBBsonReader)}.{nameof(TryParseDocument)} End document marker missmatch");
            }
            return true;
        }
    }
}
