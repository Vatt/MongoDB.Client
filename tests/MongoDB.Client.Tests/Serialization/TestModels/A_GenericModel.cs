using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    class A_GenericModel<T0, T1, T2>
    {
        public string StringName;
        public long LongValue;
        public List<List<long>> GenericArray;
    }
}
