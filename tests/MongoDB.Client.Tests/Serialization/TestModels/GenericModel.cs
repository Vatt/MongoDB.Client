using MongoDB.Client.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public partial class NonGenericModel
    {
        public int A;
        public long B;
        public double C;
    }
    [BsonSerializable]
    public partial class AnotherGenericModel<T>
    {
        public T GenericValue;
        public List<T> GenericList;

    }
    [BsonSerializable]
    public partial class GenericModel<T>
    {
        public T GenericValue;
        public List<T> GenericList;

    }

}