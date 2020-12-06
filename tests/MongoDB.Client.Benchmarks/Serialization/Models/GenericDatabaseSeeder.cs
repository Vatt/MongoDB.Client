using MongoDB.Client.Bson.Document;
using System;
using System.Collections.Generic;

namespace MongoDB.Client.Benchmarks.Serialization.Models
{
    public class GenericDatabaseSeeder
    {
        public IEnumerable<NonGenericDocument> GenerateSeed(int count = 500)
        {
            Console.WriteLine("Seeding a database for experiment....");
            yield return CreateTestDocument(count);
        }
        

        private NonGenericDocument CreateTestDocument(int itemsInList)
        {
            NonGenericDocument doc = new ()
            {
                Field0 = 100500, Field1 = "100500", Field2 = new BsonDocument("SomeElement", "SomeElementValue"),
                Field3 = BsonObjectId.NewObjectId(), Field4 = 42, Field5 = 42, Field6 = DateTimeOffset.UtcNow,
                Field7 = Guid.NewGuid(), Field8 = new AnotherNonGenericModel0(42, 42, 42),
                Field9 = new AnotherNonGenericModel1("42", "42", "42"),
                List0 = new(), List1 = new(), List2 = new(), List3 = new(), List4 = new(), List5 = new(), List6 = new(),
                List7 = new(), List8 = new(), List9 = new()
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
                doc.List8.Add(new AnotherNonGenericModel0(42, 42, 42));
                doc.List9.Add(new AnotherNonGenericModel1("42", "42", "42"));
            }
            return doc;
        }
    }
}
