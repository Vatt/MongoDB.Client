using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class IntNullable : IEquatable<IntNullable>
    {
        public int? Prop { get; set; }
        public int? Field;

        public bool Equals(IntNullable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Field == other.Field && Prop == other.Prop;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IntNullable) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
    }
    [BsonSerializable]
    public partial class DoubleNullable : IEquatable<DoubleNullable>
    {
        public double? Prop { get; set; }
        public double? Field;

        public bool Equals(DoubleNullable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nullable.Equals(Field, other.Field) && Nullable.Equals(Prop, other.Prop);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DoubleNullable) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
    }
    [BsonSerializable]
    public partial class LongNullable : IEquatable<LongNullable>
    {
        public long? Prop { get; set; }
        public long? Field;

        public bool Equals(LongNullable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Field == other.Field && Prop == other.Prop;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LongNullable) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
    }
    [BsonSerializable]
    public partial class StringNullable : IEquatable<StringNullable>
    {
        public string? Prop { get; set; }
        public string? Field;

        public bool Equals(StringNullable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Field == other.Field && Prop == other.Prop;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StringNullable) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
    }
    [BsonSerializable]
    public partial class DateTimeOffsetNullable : IEquatable<DateTimeOffsetNullable>
    {
        public DateTimeOffset? Prop { get; set; }
        public DateTimeOffset? Field;

        public bool Equals(DateTimeOffsetNullable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nullable.Equals(Field, other.Field) && Nullable.Equals(Prop, other.Prop);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DateTimeOffsetNullable) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
    }
    [BsonSerializable]
    public partial class GuidNullable : IEquatable<GuidNullable>
    {
        public Guid? Prop { get; set; }
        public Guid? Field;

        public bool Equals(GuidNullable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nullable.Equals(Field, other.Field) && Nullable.Equals(Prop, other.Prop);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GuidNullable) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
    }
    [BsonSerializable]
    public partial class BsonObjectIdNullable : IEquatable<BsonObjectIdNullable>
    {
        public BsonObjectId? Prop { get; set; }
        public BsonObjectId? Field;

        public bool Equals(BsonObjectIdNullable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nullable.Equals(Field, other.Field) && Nullable.Equals(Prop, other.Prop);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BsonObjectIdNullable) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
    }
    [BsonSerializable]
    public partial class RecordNullable : IEquatable<RecordNullable>
    {
        [BsonSerializable]
        public partial record NullableRecord(int? A, long? B, double? C);
        public NullableRecord? Prop { get; set; }
        public NullableRecord? Field;
        public static RecordNullable Create() => new RecordNullable { Prop = new NullableRecord(42, null, 42), Field = new NullableRecord(null, null, null) };

        public bool Equals(RecordNullable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Field, other.Field) && Equals(Prop, other.Prop);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RecordNullable) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
    }
    [BsonSerializable]
    public partial class StructNullable : IEquatable<StructNullable>
    {
        [BsonSerializable]
        public partial struct NullableStruct : IEquatable<NullableStruct>
        { 
            public int? A; 
            public long? B; 
            public double? C;

            public bool Equals(NullableStruct other)
            {
                return A == other.A && B == other.B && Nullable.Equals(C, other.C);
            }

            public override bool Equals(object obj)
            {
                return obj is NullableStruct other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(A, B, C);
            }
        };

        public NullableStruct? Prop { get; set; } = null;
        public NullableStruct? Field;
        public static StructNullable Create() => new StructNullable { Prop = new NullableStruct { A = 42, B = null, C = 42 }, Field = new NullableStruct { A = null, B = null, C = null } };

        public bool Equals(StructNullable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nullable.Equals(Field, other.Field) && Nullable.Equals(Prop, other.Prop);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StructNullable) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
    }

    [BsonSerializable]
    public partial class ClassNullable : IEquatable<ClassNullable>
    {
        [BsonSerializable]
        public partial class NullableClass : IEquatable<NullableClass>
        {
            public int? A;
            public long? B;
            public double? C;

            public bool Equals(NullableClass other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return A == other.A && B == other.B && Nullable.Equals(C, other.C);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((NullableClass) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(A, B, C);
            }
        };
        public NullableClass? Prop { get; set; }
        public NullableClass? Field;
        public static ClassNullable Create() => new ClassNullable { Prop = new NullableClass { A = 42, B = null, C = 42 }, Field = new NullableClass { A = null, B = null, C = null } };

        public bool Equals(ClassNullable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Field, other.Field) && Equals(Prop, other.Prop);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ClassNullable) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
    }
}
