using System;
using System.Linq;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class ByteArrayModel : IEquatable<ByteArrayModel>

    {
    public byte[] ByteProp { get; }
    public Memory<byte> MemoryByteProp { get; }

    public ByteArrayModel(byte[] ByteProp, Memory<byte> MemoryByteProp)
    {
        this.ByteProp = ByteProp;
        this.MemoryByteProp = MemoryByteProp;
    }

    public static ByteArrayModel Create()
    {
        return new ByteArrayModel(
            new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0},
            new byte[] {0, 9, 8, 7, 6, 5, 4, 3, 2, 1});
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

        return ByteProp.SequenceEqual(other.ByteProp) && MemoryByteProp.Span.SequenceEqual(other.MemoryByteProp.Span);
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

        return Equals((ByteArrayModel) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ByteProp, MemoryByteProp);
    }
    }
}
