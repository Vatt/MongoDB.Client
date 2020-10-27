using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Document;
using System;
using System.Collections.Generic;
using MongoDB.Client;
using MongoDB.Client;

namespace MongoDB.Client.Bson.Serialization.Generated
{
    class MongoTopologyVersionSerializerGenerated : IGenericBsonSerializer<MongoTopologyVersion>
    {
        private static ReadOnlySpan<byte> MongoTopologyVersionprocessId => new byte[9] { 112, 114, 111, 99, 101, 115, 115, 73, 100 };
        private static ReadOnlySpan<byte> MongoTopologyVersioncounter => new byte[7] { 99, 111, 117, 110, 116, 101, 114 };
        bool IGenericBsonSerializer<MongoTopologyVersion>.TryParse(ref BsonReader reader, out MongoTopologyVersion message)
        {
            message = new MongoTopologyVersion();
            if (!reader.TryGetInt32(out var docLength))
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

                if (!bsonName.SequenceEqual(MongoTopologyVersionprocessId))
                {
                    if (bsonType == 10)
                    {
                        message.ProcesssId = default;
                        continue;
                    }

                    if (!reader.TryGetObjectId(out message.ProcesssId))
                    {
                        return false;
                    }

                    continue;
                }

                if (!bsonName.SequenceEqual(MongoTopologyVersioncounter))
                {
                    if (bsonType == 10)
                    {
                        message.Counter = default;
                        continue;
                    }

                    if (!reader.TryGetInt64(out var value))
                    {
                        return false;
                    }
                    message.Counter = value;
                    continue;
                }

                throw new ArgumentException($"MongoTopologyVersion.TryParse  with bson type number {bsonType}");
            }

            if (!reader.TryGetByte(out var endMarker))
            {
                return false;
            }

            if (endMarker != '\x00')
            {
                throw new ArgumentException("MongoTopologyVersionGeneratedSerializator.TryParse End document marker missmatch");
            }

            return true;
        }

        ;
    }
}