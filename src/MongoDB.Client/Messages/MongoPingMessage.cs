using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    public class DnsEndPointSerializer //: IBsonSerializerExtension<EndPoint>
    {
        private static readonly byte ColonChar = (byte)':';

        public static bool TryParseBson(ref Bson.Reader.BsonReader reader, [MaybeNullWhen(false)] out EndPoint message)
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

        public static void WriteBson(ref Bson.Writer.BsonWriter writer, in EndPoint message, out byte bsonType)
        {
            throw new NotSupportedException(nameof(DnsEndPointSerializer));
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
        public List<EndPoint>? Hosts { get; }

        [BsonElement("setName")]
        public string? SetName { get; }

        [BsonElement("msg")]
        public string? Message { get; }

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

        public MongoPingMessage(List<EndPoint> hosts, string setName, string message, EndPoint me, EndPoint primary,
            MongoClusterTime clusterTime, bool isMaster, bool isSecondary)
        {
            Hosts = hosts;
            SetName = setName;
            Me = me;
            Primary = primary;
            ClusterTime = clusterTime;
            IsMaster = isMaster;
            IsSecondary = isSecondary;
            Message = message;
        }
    }
}
