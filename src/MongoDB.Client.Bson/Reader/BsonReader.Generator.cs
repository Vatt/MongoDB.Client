using System;
using System.Runtime.CompilerServices;

namespace MongoDB.Client.Bson.Reader
{
    public static class MongoDBBsonReaderExt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetGuidWithBsonType(this ref BsonReader reader, int bsonType, out Guid value)
        {
            value = default;
            
            if (bsonType == 5)
            {
                return reader.TryGetBinaryDataGuid(out value);
            }
            if (bsonType == 2)
            {
                return reader.TryGetGuidFromString(out value);                
            }
            throw new ArgumentException("Unsupported Guid type");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetDateTimeWithBsonType(this ref BsonReader reader, int bsonType, out DateTimeOffset value)
        {
            value = default;

            if (bsonType == 3)
            {
                return reader.TryGetDatetimeFromDocument(out value);
            }
            if (bsonType == 9)
            {
                return reader.TryGetUTCDatetime(out value);
            }
            if (bsonType == 18)
            {
                return reader.TryGetUTCDatetime(out value);
            }
            throw new ArgumentException("Unsupported DateTime type");
        }
    }
}
