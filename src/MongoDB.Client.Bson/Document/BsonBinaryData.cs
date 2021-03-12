using System;

namespace MongoDB.Client.Bson.Document
{
    public enum BsonBinaryDataType : byte
    {
        Generic = 0,
        BinaryOld = 2,
        UUIDOld = 3,
        UUID = 4,
        MD5 = 5,
        EncryptedBSONValue = 6,
        UserDefined = 0x80,
    }
    public readonly struct BsonBinaryData : IEquatable<BsonBinaryData>
    {
        public BsonBinaryDataType Type { get; }
        public object Value { get; }
        private BsonBinaryData(BsonBinaryDataType type, object value)
        {
            Type = type;
            Value = value;
        }

        public static BsonBinaryData Create(Guid guid)
        {
            return new BsonBinaryData(BsonBinaryDataType.UUID, guid);
        }

        public static BsonBinaryData Create(byte[] data)
        {
            return new BsonBinaryData(BsonBinaryDataType.Generic, data);
        }

        public bool Equals(BsonBinaryData other)
        {
            return Type == other.Type && Value.Equals(other.Value);
        }

        public override string ToString()
        {
            if (Value is null)
            {
                return "null";
            }

            switch (Type)
            {
                case BsonBinaryDataType.UUID:
                    var guid = (Guid)Value;
                    return $"CSUUID(\"{guid.ToString()}\")";
                case BsonBinaryDataType.Generic:
                case BsonBinaryDataType.BinaryOld:
                case BsonBinaryDataType.UUIDOld:
                case BsonBinaryDataType.MD5:
                case BsonBinaryDataType.EncryptedBSONValue:
                case BsonBinaryDataType.UserDefined:
                default:
                    var array = Value as byte[];
                    var base64 = Convert.ToBase64String(array);
                    var type = (int)Type;
                    return $"new BinData({type}, \"{base64}\")";
            }
        }
    }
}
