using MongoDB.Client.Bson.Document;
using System;
using System.Collections.Generic;
using GenericDocument = MongoDB.Client.Tests.Models.GenericDocument<double, string, MongoDB.Client.Bson.Document.BsonDocument, MongoDB.Client.Bson.Document.BsonObjectId, int, long,
                                                                    System.DateTimeOffset, System.Guid, MongoDB.Client.Tests.Models.AnotherGenericModel<int>, MongoDB.Client.Tests.Models.AnotherGenericModel<string>>;
using SmallGenericDocument = MongoDB.Client.Tests.Models.SmallGenericDocument<MongoDB.Client.Tests.Models.AnotherGenericModel<int>, MongoDB.Client.Tests.Models.AnotherGenericModel<string>>;
namespace MongoDB.Client.Benchmarks.Serialization.Models
{

    public class GenericDatabaseSeeder
    {
        public IEnumerable<Tests.Models.NonGenericDocument> GenerateSeed(int count = 500)
        {
            Console.WriteLine("Seeding a database for experiment....");
            yield return CreateTestDocument(count);
        }
        public IEnumerable<Tests.Models.SmallNonGenericDocument> GenerateSmallSeed(int count = 500)
        {
            Console.WriteLine("Seeding a database for experiment....");
            yield return GenerateSmallNonGenericDocument();
        }
        public IEnumerable<GenericDocument> GenerateGenericSeed(int count = 500)
        {
            Console.WriteLine("Seeding a database for experiment....");
            yield return CreateTestGenericDocument(count);
        }
        public IEnumerable<SmallGenericDocument> GenerateSmallGenericSeed(int count = 500)
        {
            Console.WriteLine("Seeding a database for experiment....");
            yield return GenerateSmallGenericDocument();
        }
        private Tests.Models.NonGenericDocument CreateTestDocument(int itemsInList)
        {
            Tests.Models.NonGenericDocument doc = new()
            {
                Field0 = 100500,
                Field1 = "100500",
                Field2 = new BsonDocument("SomeElement", "SomeElementValue"),
                Field3 = BsonObjectId.NewObjectId(),
                Field4 = 42,
                Field5 = 42,
                Field6 = DateTimeOffset.UtcNow,
                Field7 = Guid.NewGuid(),
                Field8 = new Tests.Models.AnotherNonGenericModel0(42, 42, 42),
                Field9 = new Tests.Models.AnotherNonGenericModel1("42", "42", "42"),
                List0 = new(),
                List1 = new(),
                List2 = new(),
                List3 = new(),
                List4 = new(),
                List5 = new(),
                List6 = new(),
                List7 = new(),
                List8 = new(),
                List9 = new()
            };

            for (int i = 0; i < itemsInList; i++)
            {
                doc.List0.Add(42);
                doc.List1.Add("****42****");
                doc.List2.Add(new BsonDocument("SomeElement", "SomeElementValue"));
                doc.List3.Add(BsonObjectId.NewObjectId());
                doc.List4.Add(42);
                doc.List5.Add(42);
                doc.List6.Add(DateTimeOffset.UtcNow);
                doc.List7.Add(Guid.NewGuid());
                doc.List8.Add(new Tests.Models.AnotherNonGenericModel0(42, 42, 42));
                doc.List9.Add(new Tests.Models.AnotherNonGenericModel1("42", "42", "42"));
            }
            return doc;
        }
        private GenericDocument CreateTestGenericDocument(int itemsInList)
        {
            GenericDocument doc = new()
            {
                Field0 = 100500,
                Field1 = "100500",
                Field2 = new BsonDocument("SomeElement", "SomeElementValue"),
                Field3 = BsonObjectId.NewObjectId(),
                Field4 = 42,
                Field5 = 42,
                Field6 = DateTimeOffset.UtcNow,
                Field7 = Guid.NewGuid(),
                Field8 = new Tests.Models.AnotherGenericModel<int>(42, 42, 42),
                Field9 = new Tests.Models.AnotherGenericModel<string>("42", "42", "42"),
                List0 = new(),
                List1 = new(),
                List2 = new(),
                List3 = new(),
                List4 = new(),
                List5 = new(),
                List6 = new(),
                List7 = new(),
                List8 = new(),
                List9 = new()
            };

            for (int i = 0; i < itemsInList; i++)
            {
                doc.List0.Add(42);
                doc.List1.Add("****42****");
                doc.List2.Add(new BsonDocument("SomeElement", "SomeElementValue"));
                doc.List3.Add(BsonObjectId.NewObjectId());
                doc.List4.Add(42);
                doc.List5.Add(42);
                doc.List6.Add(DateTimeOffset.UtcNow);
                doc.List7.Add(Guid.NewGuid());
                doc.List8.Add(new Tests.Models.AnotherGenericModel<int>(42, 42, 42));
                doc.List9.Add(new Tests.Models.AnotherGenericModel<string>("42", "42", "42"));
            }
            return doc;
        }
        private SmallGenericDocument GenerateSmallGenericDocument()
        {
            return new SmallGenericDocument()
            {
                Field0 = 100500,
                Field1 = "100500",
                Field2 = new BsonDocument("SomeElement", "SomeElementValue"),
                Field3 = BsonObjectId.NewObjectId(),
                Field4 = 42,
                Field5 = 42,
                Field6 = DateTimeOffset.UtcNow,
                Field7 = Guid.NewGuid(),
                Field8 = new Tests.Models.AnotherGenericModel<int>(42, 42, 42),
                Field9 = new Tests.Models.AnotherGenericModel<string>("42", "42", "42"),
            };
        }
        private Tests.Models.SmallNonGenericDocument GenerateSmallNonGenericDocument()
        {
            return new Tests.Models.SmallNonGenericDocument()
            {
                Field0 = 100500,
                Field1 = "100500",
                Field2 = new BsonDocument("SomeElement", "SomeElementValue"),
                Field3 = BsonObjectId.NewObjectId(),
                Field4 = 42,
                Field5 = 42,
                Field6 = DateTimeOffset.UtcNow,
                Field7 = Guid.NewGuid(),
                Field8 = new Tests.Models.AnotherNonGenericModel0(42, 42, 42),
                Field9 = new Tests.Models.AnotherNonGenericModel1("42", "42", "42"),
            };
        }
    }
}