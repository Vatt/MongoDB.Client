using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Benchmarks.Serialization.Models
{
    [BsonSerializable]
    public partial record AnotherNonGenericModel0(int A, int B, int C);

    [BsonSerializable]
    public partial record AnotherNonGenericModel1(string A, string B, string C);
    

    [BsonSerializable]
    public partial class NonGenericDocument
    {
        public double Field0 { get; set; }
        public string Field1 { get; set; }
        public BsonDocument Field2 { get; set; }
        public ObjectId Field3 { get; set; }
        public int Field4 { get; set; }
        public long Field5 { get; set; }
        public DateTimeOffset Field6 { get; set; }
        public Guid Field7 { get; set; }
        public AnotherNonGenericModel0 Field8 { get; set; }
        public AnotherNonGenericModel1 Field9 { get; set; }
        public List<double> List0 { get; set; }
        public List<string> List1 { get; set; }
        public List<BsonDocument> List2 { get; set; }
        public List<ObjectId> List3 { get; set; }
        public List<int> List4 { get; set; }
        public List<long> List5 { get; set; }
        public List<DateTimeOffset> List6 { get; set; }
        public List<Guid> List7 { get; set; }
        public List<AnotherNonGenericModel0> List8 { get; set; }
        public List<AnotherNonGenericModel1> List9 { get; set; }
    }
    
    [BsonSerializable]
    public partial record AnotherGenericModel<T>(T A, T B, T C);
    
    
    [BsonSerializable]
    public partial class GenericDocument<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        // public static GenericDocument<double, string, BsonDocument, ObjectId, int, long, DateTimeOffset, Guid,
        //     AnotherGenericModel<int>, AnotherGenericModel<string>> Create()
        //     => new();
        public T0 Field0 { get; set; }
        public T1 Field1 { get; set; }
        public T2 Field2 { get; set; }
        public T3 Field3 { get; set; }
        public T4 Field4 { get; set; }
        public T5 Field5 { get; set; }
        public T6 Field6 { get; set; }
        public T7 Field7 { get; set; }
        public T8 Field8 { get; set; }
        public T9 Field9 { get; set; }
        public List<T0> List0 { get; set; }
        public List<T1> List1 { get; set; }
        public List<T2> List2 { get; set; }
        public List<T3> List3 { get; set; }
        public List<T4> List4 { get; set; }
        public List<T5> List5 { get; set; }
        public List<T6> List6 { get; set; }
        public List<T7> List7 { get; set; }
        public List<T8> List8 { get; set; }
        public List<T9> List9 { get; set; }
    }
}