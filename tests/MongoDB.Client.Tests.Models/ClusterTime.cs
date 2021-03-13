using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    public static class MongoSignatureHashSerializer
    {
        public static bool TryParseBson(ref Bson.Reader.BsonReader reader, [MaybeNullWhen(false)] out byte[]? message)
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
                    throw new Exception($"{nameof(MongoSignatureHashSerializer)}.{nameof(TryParseBson)}");
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
    public partial class MongoClusterTime : IEquatable<MongoClusterTime>
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

        public bool Equals(MongoClusterTime? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ClusterTime.Equals(other.ClusterTime) && MongoSignature.Equals(other.MongoSignature);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((MongoClusterTime) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClusterTime, MongoSignature);
        }
    }
    
    [BsonSerializable]
    public partial class MongoSignature : IEquatable<MongoSignature>
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

        public bool Equals(MongoSignature? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ReferenceEquals(this, other) ? true : Hash.SequenceEqual(other.Hash) && KeyId == other.KeyId;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((MongoSignature) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Hash, KeyId);
        }
    }
}
