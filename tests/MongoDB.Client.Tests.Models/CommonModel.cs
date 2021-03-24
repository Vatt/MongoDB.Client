using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class CommonModel : IEquatable<CommonModel>
    {
        [BsonSerializable]
        public partial struct InnerStruct : IEquatable<InnerStruct>
        {
            public int A, B, C;

            public bool Equals(InnerStruct other)
            {
                return A == other.A && B == other.B && C == other.C;
            }

            public override bool Equals(object? obj)
            {
                return obj is InnerStruct other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(A, B, C);
            }
        }
        [BsonSerializable]
        public partial record InnerRecord(long A, long B, long C);
        public int IntProp { get; set; }
        public double DoubleProp { get; set; }
        public string StringField;
        public DateTimeOffset DateProp { get; set; }
        public BsonDocument BsonDocumentProp { get; set; }
        public BsonObjectId BsonObjectIdField;
        public long LongProp { get; set; }
        public Guid GuidProp { get; set; }

        //TODO: fix BsonArray parse call
        //public BsonArray BsonArrayProp { get; set; }
        public InnerStruct InnerStructProp { get; set; }
        public InnerRecord InnerRecordField;


        //TODO: Fix IList creations
        public IList<long> LongListProp { get; set; }
        public IList<int> IntListProp { get; set; }
        public List<string> StringListProp { get; set; }

        public ICollection<string> StringCollection { get; set; }
        public IReadOnlyCollection<string> StringReadOnlyCollection { get; set; }
        public IReadOnlyList<string> StringReadOnlyList { get; set; }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [BsonConstructor]
        public CommonModel(InnerStruct InnerStructProp, InnerRecord InnerRecordField)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            this.InnerStructProp = InnerStructProp;
            this.InnerRecordField = InnerRecordField;
        }
        public static CommonModel Create()
        {
            return new CommonModel(new InnerStruct { A = 42, B = 42, C = 42 }, new InnerRecord(42, 42, 42))
            {
                IntProp = 42,
                DoubleProp = 42.42,
                StringField = "42",
                DateProp = new DateTimeOffset(2021, 01, 01, 5, 30, 0, TimeSpan.Zero),
                BsonDocumentProp = new BsonDocument("BsonDoc", BsonObjectId.NewObjectId()),
                BsonObjectIdField = BsonObjectId.NewObjectId(),
                LongProp = 42,
                GuidProp = Guid.NewGuid(),
                StringListProp = new List<string> { "42", "42", "42" },
                LongListProp = new List<long> { 42, 42, 42 },
                IntListProp = new List<int> { 42, 42, 42 },
                StringCollection = new List<string> { "42", "42", "42" },
                StringReadOnlyCollection = new List<string> { "42", "42", "42" },
                StringReadOnlyList = new List<string> { "42", "42", "42" },
            };
        }

        public bool Equals(CommonModel? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return StringField == other.StringField && BsonObjectIdField.Equals(other.BsonObjectIdField) &&
                   InnerRecordField.Equals(other.InnerRecordField) && IntProp == other.IntProp &&
                   DoubleProp.Equals(other.DoubleProp) && DateProp.Equals(other.DateProp) &&
                   BsonDocumentProp.Equals(other.BsonDocumentProp) && LongProp == other.LongProp &&
                   GuidProp.Equals(other.GuidProp) && InnerStructProp.Equals(other.InnerStructProp) &&
                   LongListProp.SequenceEqual(other.LongListProp) &&
                   IntListProp.SequenceEqual(other.IntListProp) &&
                   StringListProp.SequenceEqual(other.StringListProp) &&
                   StringCollection.SequenceEqual(other.StringCollection) &&
                   StringReadOnlyCollection.SequenceEqual(other.StringReadOnlyCollection) &&
                   StringReadOnlyList.SequenceEqual(other.StringReadOnlyList);
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

            return Equals((CommonModel) obj);
        }
    }
}
