using System.Collections.Generic;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public class NullCheckData
    {
        public int A;

        [BsonConstructor]
        public NullCheckData(int A)
        {
            this.A = A;
        }
    }
    [BsonSerializable]
    public class NullCheckModel
    {
        public NullCheckData Data;
        public List<NullCheckData> DataArray;
        [BsonConstructor]
        public NullCheckModel(NullCheckData Data, List<NullCheckData> DataArray)
        {
            this.Data = Data;
            this.DataArray = DataArray;
        }
    }
}