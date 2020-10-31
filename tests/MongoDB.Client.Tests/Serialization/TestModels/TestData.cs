using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public class TestData : IEquatable<TestData>
    {
        [BsonSerializable]
        public class InnerTestData
        {
            public int Value0;
            public int Value1;
            public int Value2;
            public bool Equals(InnerTestData other)
            {
                if (other == null)
                {
                    return false;
                }
                return Value0 == other.Value0 && Value1 == other.Value1 && Value2 == other.Value2;
            }
            public override bool Equals(object? obj)
            {                
                return obj is not null && obj is InnerTestData && Equals(obj);
            }
        }


        [BsonElementField(ElementName = "_id")]
        public BsonObjectId Id { get; set; }

        public string Name { get; set; }
        public int Age { get; set; }

        public InnerTestData InnerData { get; set; }
        public override bool Equals(object? obj)
        {
            return obj is TestData data && Equals(data);
        }

        public bool Equals(TestData? other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (InnerData == null && other.InnerData == null)
            {
                return true;
            }
            if ((InnerData != null && other.InnerData == null) || (InnerData == null && other.InnerData != null))
            {
                return false;
            }
            return EqualityComparer<BsonObjectId>.Default.Equals(Id, other.Id) && Name == other.Name && Age == other.Age && InnerData.Equals(other.InnerData);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Age);
        }
    }
}
