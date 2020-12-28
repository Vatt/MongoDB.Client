using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public partial class ShortModelForGenerated : IEquatable<ModelForGenerated>
    {
        [BsonSerializable]
        public partial class ShortListModel : IEquatable<ModelForGenerated.ListModel>
        {
            public List<double> Doubles { get; set; }
            public List<string> Strings { get; set; }
            public List<BsonDocument> Documents { get; set; }
            public List<BsonObjectId> BsonObjectIds { get; set; }
            public List<bool> Bools;
            public List<ShortListItem> Items;

            public bool Equals(ModelForGenerated.ListModel other)
            {
                if (Items.Count != other.Items.Count)
                {
                    return false;
                }
                for (var index = 0; index < other.Items.Count; index++)
                {
                    if (!Items[index].Equals(other.Items[index]))
                    {
                        return false;
                    }
                }
                return Bools.SequenceEqual(other.Bools) && 
                       Doubles.SequenceEqual(other.Doubles) && 
                       Strings.SequenceEqual(other.Strings) && 
                       Documents.SequenceEqual(other.Documents) && 
                       BsonObjectIds.SequenceEqual(other.BsonObjectIds);
            }
            
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ModelForGenerated.ListModel) obj);
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
                //hashCode.Add(Guids);
                //hashCode.Add(Dates);
                return hashCode.ToHashCode();
            }
        }

        [BsonSerializable]
        public partial class ShortListItem : IEquatable<ModelForGenerated.ListItem>
        {
            [BsonSerializable]
            public partial class ShortInnerItem : IEquatable<ModelForGenerated.ListItem.InnerItem>
            {
                public int A;
                public PlanetModel PlanetModel;
                public ShortInnerItem()
                {
                }

                public ShortInnerItem(int a)
                {
                    A = a;
                    PlanetModel = new PlanetModel
                    {
                        Name = "Some planet",
                        Type = AtmosphereType.NoAtmosphere,
                    };
                }

                public bool Equals(ModelForGenerated.ListItem.InnerItem other)
                {
                    if (ReferenceEquals(null, other)) return false;
                    if (ReferenceEquals(this, other)) return true;
                    return A == other.A && PlanetModel.Equals(other.PlanetModel);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != this.GetType()) return false;
                    return Equals((ModelForGenerated.ListItem.InnerItem) obj);
                }

                public override int GetHashCode()
                {
                    return HashCode.Combine(A);
                }
            }

            public ShortListItem()
            {
            }

            public ShortListItem(string nameList)
            {
                Inner = new ShortInnerItem(42);
            }
            public ShortInnerItem Inner { get; set; }

            public bool Equals(ModelForGenerated.ListItem other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Inner.Equals(other.Inner);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ModelForGenerated.ListItem) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Inner);
            }
        }

        public double DoubleValue { get; set; }
        public string StringValue { get; set; }
        public BsonDocument BsonDocumentValue { get; set; }
        public BsonObjectId BsonObjectIdValue;
        public int IntValue { get; set; }

        public ShortListModel List { get; set; }

        public bool Equals(ModelForGenerated other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return BsonObjectIdValue.Equals(other.BsonObjectIdValue) &&
                   DoubleValue.Equals(other.DoubleValue) && 
                   StringValue == other.StringValue && 
                   BsonDocumentValue.Equals(other.BsonDocumentValue) &&
                   IntValue == other.IntValue &&
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
            hashCode.Add(DoubleValue);
            hashCode.Add(StringValue);
            hashCode.Add(BsonDocumentValue);
            hashCode.Add(IntValue);
            hashCode.Add(List);
            return hashCode.ToHashCode();
        }
    }
}