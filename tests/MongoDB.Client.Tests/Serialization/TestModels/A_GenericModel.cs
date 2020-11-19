using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    /*
    [BsonSerializable]
    class A_GenericModel<T0, T1, T2>
    {
        public string StringName;
        public long LongValue;
        public T0 GenericVal0;
        public T1 GenericVal1;
        public List<T2> GenericList;
    }
    */
    [BsonSerializable]
    public class A_GenericModel
    {
        public string StringName;
        public long LongValue;
        public List<List<long>> GenericArray;
        public List<double> Doubles { get; set; }
        public List<string> Strings { get; set; }
        public List<BsonDocument> Documents { get; set; }
        public List<BsonObjectId> BsonObjectIds { get; set; }
        public List<bool> Bools;
        public List<int> Ints { get; set; }
        public List<long> Longs { get; set; }
        [BsonConstructor]
        public A_GenericModel(string StringName, long LongValue)
        {
            var temp = new A_GenericModel(StringName: StringName, LongValue: LongValue);
        }
    }
}
