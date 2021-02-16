using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class CommonModel
    {
        [BsonSerializable]
        public partial struct InnerStruct
        {
            public int A, B, C;
        }
        [BsonSerializable]
        public partial record InnerRecord(long A, long B, long C);
        public int? IntProp { get; set; }
        public double DoubleProp { get; set; }
        public string? StringField;
        public DateTimeOffset DateProp { get; set; }
        public BsonDocument BsonDocumentProp { get; set; }
        public BsonObjectId BsonObjectIdField;
        public long LongProp { get; set; }
        public Guid GuidProp { get; set; }

        //TODO: fix BsonArray parse call
        //public BsonArray BsonArrayProp { get; set; }
        public InnerStruct InnerStructProp { get; set; }
        public InnerRecord InnerRecordField;


        //TODO: Fix IList creations
        //public IList<long> LongListProp { get; set; }
        //public IList<int> IntLoitProp { get; set; }
        //public IList<InnerStruct> InnerStructListProp { get; set; }
        //public IList<InnerRecord> InnerRecordListProp { get; set; }
        //public IList<IList<InnerRecord>> DoubleListInnerRecordProp { get; set; }
        //public List<IList<InnerStruct>> DoubleListInnerStructProp { get; set; }
        public List<string> StringListProp { get; set; }

        [BsonConstructor]
        public CommonModel(InnerStruct InnerStructProp, InnerRecord InnerRecordField)
        {
            this.InnerStructProp = InnerStructProp;
            this.InnerRecordField = InnerRecordField;
        }
        public static CommonModel Create()
        {
            return new CommonModel(new InnerStruct { A = 42, B = 42, C = 42 }, new InnerRecord(42, 42, 42))
            {
                IntProp = 42,
                DoubleProp = 42.42,
                StringField = "42",
                DateProp = DateTimeOffset.UtcNow,
                BsonDocumentProp = new BsonDocument("BsonDoc", BsonObjectId.NewObjectId()),
                BsonObjectIdField = BsonObjectId.NewObjectId(),
                LongProp = 42,
                GuidProp = Guid.NewGuid(),
                StringListProp = new List<string> { "42", "42", "42" }

            };
        }
    }
}
