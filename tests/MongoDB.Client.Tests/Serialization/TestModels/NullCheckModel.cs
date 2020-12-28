using System.Collections.Generic;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public partial class NullCheckData
    {
        public int A;

        [BsonConstructor]
        public NullCheckData(int A)
        {
            this.A = A;
        }
        public override bool Equals(object obj)
        {
            return obj is NullCheckData other && other.A == A;
        }
    }
    [BsonSerializable]
    public partial class NullCheckModel
    {
        public NullCheckData Data;
        public List<NullCheckData> DataArray;
        [BsonConstructor]
        public NullCheckModel(NullCheckData Data, List<NullCheckData> DataArray)
        {
            this.Data = Data;
            this.DataArray = DataArray;
        }
        public override bool Equals(object obj)
        {
            if (obj is not NullCheckModel other)
            {
                return false;
            }
            if ( (other.Data is null && Data is not null) || (Data is null && other.Data is not null) )
            {
                return false;
            }
            if ((other.DataArray is null && DataArray is not null) || (DataArray is null && other.DataArray is not null))
            {
                return false;
            }
            if (Data is null && other.Data is null && DataArray.SequentialEquals(other.DataArray))
            {
                return true;
            }
            if (DataArray is null && other.DataArray is null && Data.Equals(other.Data))
            {
                return true;
            }
            return Data.Equals(other.Data) && DataArray.SequentialEquals(other.DataArray);
        }
    }
}