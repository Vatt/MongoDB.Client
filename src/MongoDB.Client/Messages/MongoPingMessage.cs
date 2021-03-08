using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Exceptions;
using System.Buffers.Binary;

namespace MongoDB.Client.Messages
{
    public static class DnsEndPointSerializer
    {
        private static readonly byte ColonChar = (byte) ':';

        public static bool TryParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, out EndPoint message)
        {
            message = default;
            if (!reader.TryGetStringAsSpan(out var temp))
            {
                return false;
            }
            else
            {
                ReadOnlySpan<byte> host;
                ReadOnlySpan<byte> port;
                var index = temp.LastIndexOf(ColonChar);
                host = temp.Slice(0, index);
                port = temp.Slice(index + 1);
                var hostStr = Encoding.UTF8.GetString(host);
                var portStr = Encoding.UTF8.GetString(port);
                message = new DnsEndPoint(hostStr, int.Parse(portStr));
                return true;
            }
        }

        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in EndPoint message)
        {
            throw new NotSupportedException(nameof(DnsEndPointSerializer));
        }
    }

    [BsonSerializable]
    public partial class MongoClusterTime
    {
        [BsonElement("clusterTime")] public BsonTimestamp ClusterTime { get; }

        [BsonElement("signature")] public MongoSignature MongoSignature { get; }

        public MongoClusterTime(BsonTimestamp ClusterTime, MongoSignature MongoSignature)
        {
            this.ClusterTime = ClusterTime;
            this.MongoSignature = MongoSignature;
        }
    }

    public class MongoSignature
    {
        [BsonElement("hash")] public byte[] Hash { get; }

        [BsonElement("keyId")] public long KeyId { get; }

        public MongoSignature(byte[] Hash, long KeyId)
        {
            this.Hash = Hash;
            this.KeyId = KeyId;
        }

        private static ReadOnlySpan<byte> MongoSignaturehash => new byte[4] {104, 97, 115, 104};
        private static ReadOnlySpan<byte> MongoSignaturekeyId => new byte[5] {107, 101, 121, 73, 100};

        public static bool TryParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, out MongoDB.Client.Messages.MongoSignature message)
        {
            message = default;
            byte[] Hash = default;
            long Int64KeyId = default;
            if (!reader.TryGetInt32(out int docLength))
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

                if (bsonType == 10)
                {
                    continue;
                }

                if (bsonName.SequenceEqual(MongoSignaturehash))
                {
                    if (!reader.TryGetBinaryData(out var temp))
                    {
                        return false;
                    }
                    else
                    {
                        if (temp.Type != BsonBinaryDataType.Generic)
                        {
                            ThrowHelper.UnsupportedTypeException(typeof(BsonBinaryData));
                        }

                        Hash = (byte[]) temp.Value;
                        continue;
                    }                    
                }

                if (bsonName.SequenceEqual(MongoSignaturekeyId))
                {
                    if (!reader.TryGetInt64(out Int64KeyId))
                    {
                        return false;
                    }

                    continue;
                }

                if (!reader.TrySkip(bsonType))
                {
                    return false;
                }
            }

            if (!reader.TryGetByte(out var endMarker))
            {
                return false;
            }

            if (endMarker != 0)
            {
                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(
                    nameof(MongoDB.Client.Messages.MongoSignature), endMarker);
            }

            message = new MongoDB.Client.Messages.MongoSignature(Hash: Hash, KeyId: Int64KeyId);
            return true;
        }

        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in MongoDB.Client.Messages.MongoSignature message)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            if (message.Hash == null)
            {
                writer.WriteBsonNull(MongoSignaturehash);
            }
            else
            {
                writer.Write_Type_Name_Value(MongoSignaturehash, BsonBinaryData.Create(message.Hash));
            }

            writer.Write_Type_Name_Value(MongoSignaturekeyId, message.KeyId);
            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            Span<byte> sizeSpan = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(sizeSpan, docLength);
            reserved.Write(sizeSpan);
            writer.Commit();
        }
    }

    [BsonSerializable]
    public partial class MongoPingMessage
    {
        [BsonElement("hosts")]
        [BsonSerializer(typeof(DnsEndPointSerializer))]
        public List<EndPoint> Hosts { get; }

        [BsonElement("setName")] public string SetName { get; }

        [BsonElement("me")]
        [BsonSerializer(typeof(DnsEndPointSerializer))]
        public EndPoint Me { get; }

        [BsonElement("primary")]
        [BsonSerializer(typeof(DnsEndPointSerializer))]
        public EndPoint? Primary { get; }

        [BsonElement("$clusterTime")] public MongoClusterTime ClusterTime { get; }

        [BsonElement("ismaster")] public bool IsMaster { get; }

        [BsonElement("secondary")] public bool IsSecondary { get; }

        public MongoPingMessage(List<EndPoint> Hosts, string SetName, EndPoint Me, EndPoint Primary,
            MongoClusterTime ClusterTime, bool IsMaster, bool IsSecondary)
        {
            this.Hosts = Hosts;
            this.SetName = SetName;
            this.Me = Me;
            this.Primary = Primary;
            this.ClusterTime = ClusterTime;
            this.IsMaster = IsMaster;
            this.IsSecondary = IsSecondary;
        }
    }
}
