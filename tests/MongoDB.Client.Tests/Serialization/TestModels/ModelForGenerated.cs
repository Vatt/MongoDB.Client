using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public class ModelForGenerated : IEquatable<ModelForGenerated>
    {
        [BsonSerializable]
        public class ListModel : IEquatable<ListModel>
        {
            public List<double> Doubles { get; set; }
            public List<string> Strings { get; set; }
            public List<BsonDocument> Documents { get; set; }
            public List<BsonObjectId> BsonObjectIds { get; set; }
            public List<bool> Bools;
            public List<int> Ints { get; set; }
            public List<long> Longs { get; set; }

            public List<ListItem> Items;
            //public List<Guid> Guids { get; set; }
            //public List<DateTimeOffset> Dates { get; set; }

            public bool Equals(ListModel other)
            {
                return Bools.SequenceEqual(other.Bools) && 
                       Items.SequenceEqual(other.Items) && 
                       Doubles.SequenceEqual(other.Doubles) && 
                       Strings.SequenceEqual(other.Strings) && 
                       Documents.SequenceEqual(other.Documents) && 
                       BsonObjectIds.SequenceEqual(other.BsonObjectIds) && 
                       Ints.SequenceEqual(other.Ints) && 
                       Longs.SequenceEqual(other.Longs) && 
                       //Guids.SequenceEqual(other.Guids) && 
                       //Dates.SequenceEquals(other.Dates)
                       Items.SequenceEqual(other.Items);
            }
            
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ListModel) obj);
            }

            public override int GetHashCode()
            {
                var hashCode = new HashCode();
                hashCode.Add(Bools);
                hashCode.Add(Items);
                hashCode.Add(Doubles);
                hashCode.Add(Strings);
                hashCode.Add(Documents);
                hashCode.Add(BsonObjectIds);
                hashCode.Add(Ints);
                hashCode.Add(Longs);
                //hashCode.Add(Guids);
                //hashCode.Add(Dates);
                return hashCode.ToHashCode();
            }
        }

        [BsonSerializable]
        public class ListItem: IEquatable<ListItem>
        {
            [BsonSerializable]
            public class InnerItem : IEquatable<InnerItem>
            {
                public int A;
                public int B;
                public int C;

                public InnerItem()
                {
                }

                public InnerItem(int a, int b, int c)
                {
                    A = a;
                    B = b;
                    C = c;
                }

                public bool Equals(InnerItem other)
                {
                    if (ReferenceEquals(null, other)) return false;
                    if (ReferenceEquals(this, other)) return true;
                    return A == other.A && B == other.B && C == other.C;
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != this.GetType()) return false;
                    return Equals((InnerItem) obj);
                }

                public override int GetHashCode()
                {
                    return HashCode.Combine(A, B, C);
                }
            }

            public ListItem()
            {
            }

            public ListItem(string nameList)
            {
                NameList = nameList;
                Inner = new InnerItem(42, 42, 42);
            }
            public string NameList { get; set; }
            public InnerItem Inner { get; set; }

            public bool Equals(ListItem other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return NameList == other.NameList && Equals(Inner, other.Inner);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ListItem) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(NameList, Inner);
            }
        }

        public double DoubleValue { get; set; }
        public string StringValue { get; set; }
        public BsonDocument BsonDocumentValue { get; set; }
        public BsonObjectId BsonObjectIdValue;

        public bool BooleanValue;

        //public DateTimeOffset DateTimeOffsetValue { get; set; }
        //public Guid GuidValue { get; set; }
        public int IntValue { get; set; }
        public long LongValue { get; set; }

        public ListModel List { get; set; }

        public bool Equals(ModelForGenerated other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return BsonObjectIdValue.Equals(other.BsonObjectIdValue) && 
                   BooleanValue == other.BooleanValue && 
                   DoubleValue.Equals(other.DoubleValue) && 
                   StringValue == other.StringValue && 
                   BsonDocumentValue.Equals(other.BsonDocumentValue) &&
                   //DateTimeOffsetValue.Equals(other.DateTimeOffsetValue) && 
                   //GuidValue.Equals(other.GuidValue) && 
                   IntValue == other.IntValue && 
                   LongValue == other.LongValue && 
                   List.Equals(other.List);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ModelForGenerated) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(BsonObjectIdValue);
            hashCode.Add(BooleanValue);
            hashCode.Add(DoubleValue);
            hashCode.Add(StringValue);
            hashCode.Add(BsonDocumentValue);
            //hashCode.Add(DateTimeOffsetValue);
            //hashCode.Add(GuidValue);
            hashCode.Add(IntValue);
            hashCode.Add(LongValue);
            hashCode.Add(List);
            return hashCode.ToHashCode();
        }
    }
}
