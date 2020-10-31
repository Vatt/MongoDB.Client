using System;
using System.Collections.Generic;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.ConsoleApp
{
    [BsonSerializable]
    public class Data : IEquatable<Data>
    {
        [BsonElementField(ElementName = "_id")]
        public BsonObjectId Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public Data InnerData { get; set; }
        public override bool Equals(object? obj)
        {
            return obj is Data data && Equals(data);
        }

        public bool Equals(Data? other)
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
            if ( (InnerData != null && other.InnerData == null ) || (InnerData == null && other.InnerData != null))
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
