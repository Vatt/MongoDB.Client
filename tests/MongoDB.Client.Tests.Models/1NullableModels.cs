using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class EnumNullable
    {
        public enum IntEnum
        {
            One, Two
        }
        public enum LongEnum
        {
            One, Two
        }
        public enum StringEnum
        {
            [BsonElement("StringField1Alias")]
            StringField1,
            [BsonElement("StringField2Alias")]
            StringFeild2
        }
        public IntEnum? IntEnum1 { get; set; }
        public IntEnum? IntEnum2 { get; set; }
        public IntEnum? IntEnum3 { get; set; }

        [BsonEnum(EnumRepresentation.Int64)]
        public LongEnum? LongEnum1 { get; set; }

        [BsonEnum(EnumRepresentation.Int64)]
        public LongEnum? LongEnum2 { get; set; }   
        [BsonEnum(EnumRepresentation.Int64)]
        public LongEnum? LongEnum3 { get; set; }

        [BsonEnum(EnumRepresentation.String)]
        public StringEnum? StringEnum1 { get; set; }

        [BsonEnum(EnumRepresentation.String)]
        public StringEnum? StringEnum2 { get; set; }

        [BsonEnum(EnumRepresentation.String)]
        public StringEnum? StringEnum3 { get; set; }

        public EnumNullable Create() => new EnumNullable
        {
            IntEnum1 = IntEnum.One,
            IntEnum2 = IntEnum.Two,
            IntEnum3 = null,
            LongEnum1 = LongEnum.Two,
            LongEnum2 = LongEnum.One,
            LongEnum3 = null,
            StringEnum1 = StringEnum.StringField1,
            StringEnum2 = StringEnum.StringFeild2,
            StringEnum3 = null,
        };

    }

    [BsonSerializable]
    public partial class GenericWithNulalbleListTest<T, TT> : IEquatable<GenericWithNulalbleListTest<T, TT>>
    {
        [BsonSerializable]
        public partial struct InnerGenericStruct<TTT>
        {
            public TTT A, B, C;
            public InnerGenericStruct(TTT a, TTT b, TTT c)
            {
                A = a; B = b; C = c;
            }
        }
        public List<T?> List1 { get; set; }
        public List<TT>? List2 { get; set; }
        public List<InnerGenericStruct<T>?>? List3 { get; set; }
        public List<InnerGenericStruct<T>>? List4 { get; set; }
        public static GenericWithNulalbleListTest<T, TT> Create(T le1, T le2, T le3, TT le4, TT le5, TT le6)
        {
            return new()
            {
                List1 = new() { le1, le2, le3 },
                List2 = new() { le4, le5, le6 },
                List3 = new() { new InnerGenericStruct<T>(le1, le2, le3), null, new InnerGenericStruct<T>(le1, le2, le3) },
                List4 = null,
            };
        }

        public bool Equals(GenericWithNulalbleListTest<T, TT>? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return List1.SequenceEqual(other.List1) && List2!.SequenceEqual(other!.List2!);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GenericWithNulalbleListTest<T, TT>)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(List1, List2);
        }
    }


    [BsonSerializable]
    public partial class GenericNullable : IEquatable<GenericNullable>
    {
        [BsonSerializable]
        public partial class GenericClass<T, TT> : IEquatable<GenericClass<T, TT>>
        {
            public T? TProp { get; set; }
            public TT? TTProp { get; set; }

            public bool Equals(GenericClass<T, TT>? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return EqualityComparer<T?>.Default.Equals(TProp, other.TProp) && EqualityComparer<TT?>.Default.Equals(TTProp, other.TTProp);
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((GenericClass<T, TT>)obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(TProp, TTProp);
            }
        }
        [BsonSerializable]
        public partial struct GenericStruct<T, TT> : IEquatable<GenericStruct<T, TT>>
        {
            public T TProp { get; set; }
            public TT TTProp { get; set; }

            public bool Equals(GenericStruct<T, TT> other)
            {
                return EqualityComparer<T?>.Default.Equals(TProp, other.TProp) && EqualityComparer<TT?>.Default.Equals(TTProp, other.TTProp);
            }

            public override bool Equals(object? obj)
            {
                return obj is GenericStruct<T, TT> other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(TProp, TTProp);
            }
        }
        [BsonSerializable]
        public partial record GenericRecord<T, TT>(T TProp, TT TTProp);

        public GenericClass<int, string> ClassProp { get; set; }
        public GenericStruct<int, string> StructProp { get; set; }
        public GenericRecord<int, string> RecordProp { get; set; }

        public bool Equals(GenericNullable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(ClassProp, other.ClassProp) && StructProp.Equals(other.StructProp) && Equals(RecordProp, other.RecordProp);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GenericNullable)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClassProp, StructProp, RecordProp);
        }

        public static GenericNullable Create()
        {
            return new()
            {
                ClassProp = new GenericClass<int, string>() { TProp = default, TTProp = "42" },
                RecordProp = new GenericRecord<int, string>(42, null!),
                StructProp = new GenericStruct<int, string>() { TProp = 42, TTProp = string.Empty }
            };
        }
    }


    [BsonSerializable]
    public partial class IntNullable : IEquatable<IntNullable>
    {
        public int? Prop { get; set; }
        public int? Field;

        public bool Equals(IntNullable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Field == other.Field && Prop == other.Prop;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IntNullable)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
        public static IntNullable Create() => new IntNullable() { Prop = 42, Field = null };
    }
    
    [BsonSerializable]
    public partial class DoubleNullable : IEquatable<DoubleNullable>
    {
        public double? Prop { get; set; }
        public double? Field;

        public bool Equals(DoubleNullable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nullable.Equals(Field, other.Field) && Nullable.Equals(Prop, other.Prop);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DoubleNullable)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
        public static DoubleNullable Create() => new DoubleNullable() { Prop = 42, Field = null };
    }
    
    [BsonSerializable]
    public partial class LongNullable : IEquatable<LongNullable>
    {
        public long? Prop { get; set; }
        public long? Field;

        public bool Equals(LongNullable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Field == other.Field && Prop == other.Prop;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LongNullable)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
        public static LongNullable Create() => new LongNullable() { Prop = 42, Field = null };
    }

    [BsonSerializable]
    public partial class StringNullable : IEquatable<StringNullable>
    {
        public string? Prop { get; set; }
        public string? Field;

        public bool Equals(StringNullable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Field == other.Field && Prop == other.Prop;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StringNullable)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }

        public static StringNullable Create() => new StringNullable() { Prop = "42", Field = "42" };
    }
    
    [BsonSerializable]
    public partial class DateTimeOffsetNullable : IEquatable<DateTimeOffsetNullable>
    {
        public DateTimeOffset? Prop { get; set; }
        public DateTimeOffset? Field;

        public bool Equals(DateTimeOffsetNullable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nullable.Equals(Field, other.Field) && Nullable.Equals(Prop, other.Prop);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DateTimeOffsetNullable)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
        public static DateTimeOffsetNullable Create() => new DateTimeOffsetNullable() 
        { 
            Prop = new DateTimeOffset(2021, 01, 01, 5, 30, 0, TimeSpan.Zero), 
            Field = null 
        };
    }
    
    [BsonSerializable]
    public partial class GuidNullable : IEquatable<GuidNullable>
    {
        public Guid? Prop { get; set; }
        public Guid? Field;

        public bool Equals(GuidNullable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nullable.Equals(Field, other.Field) && Nullable.Equals(Prop, other.Prop);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GuidNullable)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }

        public static GuidNullable Create() => new() { Field = Guid.NewGuid(), Prop = null };
    }
    
    [BsonSerializable]
    public partial class BsonObjectIdNullable : IEquatable<BsonObjectIdNullable>
    {
        public BsonObjectId? Prop { get; set; }
        public BsonObjectId? Field;

        public bool Equals(BsonObjectIdNullable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nullable.Equals(Field, other.Field) && Nullable.Equals(Prop, other.Prop);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BsonObjectIdNullable)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }

        public static BsonObjectIdNullable Create() => new() { Prop = BsonObjectId.NewObjectId(), Field = null };
    }

    [BsonSerializable]
    public partial class RecordNullable : IEquatable<RecordNullable>
    {
        [BsonSerializable]
        public partial record NullableRecord(int? A, long? B, double? C);
        public NullableRecord? Prop { get; set; }
        public NullableRecord? Field;
        public static RecordNullable Create() => new RecordNullable { Prop = new NullableRecord(42, null, 42), Field = new NullableRecord(null, null, null) };

        public bool Equals(RecordNullable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Field, other.Field) && Equals(Prop, other.Prop);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RecordNullable)obj);
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

            public override bool Equals(object? obj)
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

        public bool Equals(StructNullable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nullable.Equals(Field, other.Field) && Nullable.Equals(Prop, other.Prop);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StructNullable)obj);
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

            public bool Equals(NullableClass? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return A == other.A && B == other.B && Nullable.Equals(C, other.C);
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((NullableClass)obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(A, B, C);
            }
        };
        public NullableClass? Prop { get; set; }
        public NullableClass? Field;
        public static ClassNullable Create() => new ClassNullable { Prop = new NullableClass { A = 42, B = null, C = 42 }, Field = new NullableClass { A = null, B = null, C = null } };

        public bool Equals(ClassNullable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Field, other.Field) && Equals(Prop, other.Prop);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ClassNullable)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }
    }

    [BsonSerializable]
    public partial class ListNullable : IEquatable<ListNullable>
    {
        public List<int>? Prop { get; set; }
        public List<string>? Field;

        public bool Equals(ListNullable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Field!.SequenceEqual(other!.Field!) && Prop!.SequenceEqual(other!.Prop!);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ListNullable)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Prop);
        }

        public static ListNullable Create() => new() { Field = new() { "42", "42", "42" }, Prop = new() { 42, 42, 42 } };
    }

    [BsonSerializable]
    public partial class ListElementNullable : IEquatable<ListElementNullable>
    {
        public List<int?>? NullableInts { get; set; }
        public List<string?>? NullableStrings { get; set; }
        public List<long?>? NullableLongs { get; set; }
        public List<Guid?>? NullableGuids { get; set; }
        public List<DateTimeOffset?>? NullableDates { get; set; }
        public List<BsonObjectId?>? NullableBsonObjectId { get; set; }
        public List<double?>? NullableDoubles { get; set; }
        public List<RecordNullable?>? NullableRecords { get; set; }
        public List<ClassNullable?>? NullableClasses { get; set; }
        public List<StructNullable?>? NullableStructures { get; set; }
        public List<List<StructNullable?>?>? NullList { get; set; }

        public bool Equals(ListElementNullable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return NullableInts!.SequenceEqual(other.NullableInts!) && NullableStrings!.SequenceEqual(other.NullableStrings!) &&
                   NullableLongs!.SequenceEqual(other.NullableLongs!) && NullableGuids!.SequenceEqual(other.NullableGuids!) &&
                   NullableDates!.SequenceEqual(other.NullableDates!) && NullableBsonObjectId!.SequenceEqual(other.NullableBsonObjectId!) &&
                   NullableDoubles!.SequenceEqual(other.NullableDoubles!) && NullableRecords!.SequenceEqual(other.NullableRecords!) &&
                   NullableClasses!.SequenceEqual(other.NullableClasses!) && NullableStructures!.SequenceEqual(other.NullableStructures!);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ListElementNullable)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(NullableInts);
            hashCode.Add(NullableStrings);
            hashCode.Add(NullableLongs);
            hashCode.Add(NullableGuids);
            hashCode.Add(NullableDates);
            hashCode.Add(NullableBsonObjectId);
            hashCode.Add(NullableDoubles);
            hashCode.Add(NullableRecords);
            hashCode.Add(NullableClasses);
            hashCode.Add(NullableStructures);
            return hashCode.ToHashCode();
        }

        public static ListElementNullable Create()
        {
            return new()
            {
                NullableRecords = new() { RecordNullable.Create(), null, RecordNullable.Create() },
                NullableStructures = new() { StructNullable.Create(), null, StructNullable.Create() },
                NullableClasses = new() { ClassNullable.Create(), null, null },
                NullableDates = new()
                {
                    new DateTimeOffset(2021, 01, 01, 5, 30, 0, TimeSpan.Zero),
                    null,
                    new DateTimeOffset(2021, 01, 01, 5, 30, 0, TimeSpan.Zero)
                },
                NullableDoubles = new() { 42, null, 42 },
                NullableGuids = new() { Guid.NewGuid(), null, Guid.NewGuid() },
                NullableInts = new() { 42, null, 42 },
                NullableLongs = new() { 42, null, 42 },
                NullableStrings = new() { "42", null, "42" },
                NullableBsonObjectId = new() { BsonObjectId.NewObjectId(), null, BsonObjectId.NewObjectId() },
                NullList = null
            };
        }
    }

    [BsonSerializable]
    public partial struct OtherModelsNullable : IEquatable<OtherModelsNullable>
    {
        public ListElementNullable? Prop1 { get; set; }
        public ListNullable? Prop2 { get; set; }
        public ClassNullable? Prop3 { get; set; }

        public StructNullable? Prop4;
        public RecordNullable? Prop5 { get; set; }
        public BsonObjectIdNullable? Prop6 { get; set; }
        public GuidNullable? Prop7 { get; set; }
        public DateTimeOffsetNullable? Prop8 { get; set; }
        public StringNullable? Prop9 { get; set; }
        public LongNullable? Prop10 { get; set; }
        public DoubleNullable? Prop11 { get; set; }
        public IntNullable? Prop12 { get; set; }
        public GenericNullable? Prop13 { get; set; }
        public GenericWithNulalbleListTest<GenericNullable, RecordNullable>? Prop14 { get; set; }

        public static OtherModelsNullable Create()
        {
            return new()
            {
                Prop1 = ListElementNullable.Create(),
                Prop2 = ListNullable.Create(),
                Prop3 = ClassNullable.Create(),
                Prop4 = StructNullable.Create(),
                Prop5 = RecordNullable.Create(),
                Prop6 = BsonObjectIdNullable.Create(),
                Prop7 = GuidNullable.Create(),
                Prop8 = DateTimeOffsetNullable.Create(),
                Prop9 = StringNullable.Create(),
                Prop10 = LongNullable.Create(),
                Prop11 = DoubleNullable.Create(),
                Prop12 = IntNullable.Create(),
                Prop13 = GenericNullable.Create(),
                Prop14 = GenericWithNulalbleListTest<GenericNullable, RecordNullable>.Create(
                    GenericNullable.Create(), null!, GenericNullable.Create(),
                    RecordNullable.Create(), null!, RecordNullable.Create()),

            };
        }

        public bool Equals(OtherModelsNullable other)
        {
            return Equals(Prop4, other.Prop4) && Equals(Prop1, other.Prop1) && Equals(Prop2, other.Prop2) && Equals(Prop3, other.Prop3) && Equals(Prop5, other.Prop5) && 
                   Equals(Prop6, other.Prop6) && Equals(Prop7, other.Prop7) && Equals(Prop8, other.Prop8) && Equals(Prop9, other.Prop9) && Equals(Prop10, other.Prop10) && 
                   Equals(Prop11, other.Prop11) && Equals(Prop12, other.Prop12) && Equals(Prop13, other.Prop13) && Equals(Prop14, other.Prop14);
        }

        public override bool Equals(object? obj)
        {
            return obj is OtherModelsNullable other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Prop4);
            hashCode.Add(Prop1);
            hashCode.Add(Prop2);
            hashCode.Add(Prop3);
            hashCode.Add(Prop5);
            hashCode.Add(Prop6);
            hashCode.Add(Prop7);
            hashCode.Add(Prop8);
            hashCode.Add(Prop9);
            hashCode.Add(Prop10);
            hashCode.Add(Prop11);
            hashCode.Add(Prop12);
            hashCode.Add(Prop13);
            hashCode.Add(Prop14);
            return hashCode.ToHashCode();
        }
    }
}
