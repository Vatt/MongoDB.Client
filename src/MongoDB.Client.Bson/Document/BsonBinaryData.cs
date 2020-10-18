using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public readonly struct BsonBinaryData
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
    }
}
