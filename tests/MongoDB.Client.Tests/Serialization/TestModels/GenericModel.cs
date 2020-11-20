using MongoDB.Client.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public class NonGenericModel
    {
        public int A;
        public long B;
        public double C;
    }
    [BsonSerializable]
    public class GenericModel<T>
    {
        public T GenericValue;
        public List<T> GenericList;

    }
}