using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Test
{
    [BsonSerializable]
    class Sample
    {
        public long LongValue { get; set; }
        public Guid GuidValue { get; set; }
        public string StringValue;
        public int IntValue;
        public double DoubleValue;
        public DateTimeOffset DateTimeValue;
        public BsonObjectId ObjectId;
        public bool BooleanValue { get; set; }
        BsonDocument BsonDocumentValue { get; set; }
    }

    [BsonSerializable]
    class GenericSample<T>
    {
        public T Value { get; set; }
        public Guid Id;
    }

    //[BsonSerializable]
    //class GenericDTO<T0, T1, T2>
    //{
    //    [BsonElementField(ElementName = "test test ()")]
    //    public GenericSample<T0> Value0;
    //    public T0 Value00;
    //    public GenericSample<T1> Value1;
    //    public T1 Value11;
    //    public GenericSample<T2> Value2;
    //    public T2 Value22;

    //    public List<int> GenericIntList;
    //    public List<double> GenericDoubleList;

    //    public List<DateTimeOffset> GenericDateTimeList;
    //    public List<Guid> GenericGuidList;

    //    public DateTimeOffset DateTime;

    //    public Guid Type { get; set; }
    //}
}
