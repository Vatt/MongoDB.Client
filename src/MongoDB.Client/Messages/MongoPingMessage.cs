using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Exceptions;

namespace MongoDB.Client.Messages
{
    public static class DnsEndPointSerializer
    {
        private static readonly byte ColonChar = (byte)':';
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
    public static class MongoSignatureHashSerializer
    {
        public static bool TryParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, out byte[] message)
        {
            message = default;
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
                message = (byte[])temp.Value;
                return true;
            }
        }
        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in byte[] message)
        {
            writer.WriteBinaryData(BsonBinaryData.Create(message));
        }
    }

    [BsonSerializable]
    public partial class MongoClusterTime
    {
        [BsonElement("clusterTime")]
        public BsonTimestamp ClusterTime { get; }
        
        [BsonElement("signature")]
        public MongoSignature MongoSignature { get; }

        public MongoClusterTime(BsonTimestamp ClusterTime, MongoSignature MongoSignature)
        {
            this.ClusterTime = ClusterTime;
            this.MongoSignature = MongoSignature;
        }
    }
    
    [BsonSerializable]
    public partial class MongoSignature
    {
        [BsonElement("hash")]
        [BsonSerializer(typeof(MongoSignatureHashSerializer))]
        public byte[] Hash { get; }
        
        [BsonElement("keyId")]
        public long KeyId { get; }
        public MongoSignature(byte[] Hash, long KeyId)
        {
            this.Hash = Hash;
            this.KeyId = KeyId;
        }
    }
    
    [BsonSerializable]
    public partial class MongoPingMessage
    {
        [BsonElement("hosts")]
        [BsonSerializer(typeof(DnsEndPointSerializer))]
        public List<EndPoint> Hosts { get; }

        [BsonElement("setName")]
        public string SetName { get; }

        [BsonElement("me")]
        [BsonSerializer(typeof(DnsEndPointSerializer))]
        public EndPoint Me { get; }

        [BsonElement("primary")]
        [BsonSerializer(typeof(DnsEndPointSerializer))]
        public EndPoint? Primary { get; }

        [BsonElement("$clusterTime")]
        public MongoClusterTime ClusterTime { get; }
        
        [BsonElement("ismaster")]
        public bool IsMaster { get; }

        [BsonElement("secondary")]
        public bool IsSecondary { get; }
        public MongoPingMessage(List<EndPoint> Hosts, string SetName, EndPoint Me, EndPoint Primary, MongoClusterTime ClusterTime, bool IsMaster, bool IsSecondary)
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
