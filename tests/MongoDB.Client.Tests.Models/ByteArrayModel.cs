using System.Security.Cryptography;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable(GeneratorMode.ConstructorParameters | GeneratorMode.IfConditions)]
    public partial class ByteArrayModel : IEquatable<ByteArrayModel>

    {
        public byte[] ByteProp { get; }
        public Memory<byte> MemoryByteProp { get; }

        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public byte[] MD5ByteProp { get; }

        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Memory<byte>? MD5MemoryProp { get; }
        [BsonConstructor]
        //public ByteArrayModel(byte[] ByteProp, Memory<byte> MemoryByteProp, byte[] MD5ByteProp, Memory<byte>? MD5MemoryProp)
        public ByteArrayModel(byte[] ByteProp, Memory<byte> MemoryByteProp, byte[] MD5ByteProp, Memory<byte>? MD5MemoryProp)
        {
            this.ByteProp = ByteProp;
            this.MemoryByteProp = MemoryByteProp;
            this.MD5ByteProp = MD5ByteProp;
            this.MD5MemoryProp = MD5MemoryProp;
        }
        public ByteArrayModel(int a, int b, int c)
        {
            this.ByteProp = null!;
            this.MemoryByteProp = null;
            this.MD5ByteProp = null!;
            this.MD5MemoryProp = null;
        }
        public static ByteArrayModel Create()
        {
            return new ByteArrayModel(
                new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 },
                new byte[] { 0, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
                MD5.HashData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }),
                MD5.HashData(new byte[] { 0, 9, 8, 7, 6, 5, 4, 3, 2, 1 })
                );
        }

        public bool Equals(ByteArrayModel? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ByteProp.SequenceEqual(other.ByteProp) && MemoryByteProp.Span.SequenceEqual(other.MemoryByteProp.Span) &&
                   MD5ByteProp.SequenceEqual(other.MD5ByteProp) && MD5MemoryProp!.Value.Span.SequenceEqual(other.MD5MemoryProp!.Value.Span);
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

            return Equals((ByteArrayModel)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ByteProp, MemoryByteProp, MD5ByteProp, MD5MemoryProp);
        }
    }
}
