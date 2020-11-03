using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MongoDB.Client.Tests.Serialization.TestModels
{

    [BsonSerializable]
    public class TestData : IEquatable<TestData>
    {

        [BsonSerializable]
        public class InnerTestData
        {

            [BsonSerializable]
            public class ListItem
            {
                public int a;
                public int b;
                public int c;
                public bool Equals(ListItem other) => (a == other.a) && (b == other.b) && (c == other.c);
                public override bool Equals(object obj)
                {
                    if (obj == null)
                    {
                        return false;
                    }
                    return obj is ListItem item && item.Equals(item);
                }
            }
            public int Value0;
            public double Value1;
            public long Value2;
            public List<int> IntList;
            public List<double> DoubleList;
            public List<long> LongList;
            public List<BsonObjectId> BsonObjectIdList;
            public List<BsonDocument> BsonDocumentList;
            //public List<DateTimeOffset> DateTimeOffsetList;
            public List<bool> BoolList;
            public List<string> StringList;
            public List<ListItem> ItemList;
            public bool Equals(InnerTestData other)
            {
                if (other == null)
                {
                    return false;
                }
                return Value0 == other.Value0 && Value1 == other.Value1 && Value2 == other.Value2 &&
                       IntList.SequenceEqual(other.IntList) &&
                       DoubleList.SequenceEqual(other.DoubleList) &&
                       LongList.SequenceEqual(other.LongList) &&
                       BsonObjectIdList.SequenceEqual(other.BsonObjectIdList) &&
                       BsonDocumentList.SequenceEqual(other.BsonDocumentList) &&
                       //DateTimeOffsetList.SequenceEqual(other.DateTimeOffsetList, EqualityComparer<DateTimeOffset>.Default) &&
                       BoolList.SequenceEqual(other.BoolList) &&
                       StringList.SequenceEqual(other.StringList) &&
                       ItemList.SequenceEqual(other.ItemList);
            }
            public override bool Equals(object? obj)
            {                
                return obj is not null && obj is  InnerTestData inner && Equals(inner) ;
            }
        }


        [BsonElementField(ElementName = "_id")]
        public BsonObjectId Id { get; set; }

        public string Name { get; set; }
        public int Age { get; set; }

        public InnerTestData InnerData { get; set; }
        public override bool Equals(object? obj)
        {
            return obj is Data data && Equals(data);
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
