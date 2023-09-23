using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Client
{
    [BsonSerializable]
    public partial class FirstDoc : IEquatable<FirstDoc>
    {
        [BsonId]
        public BsonObjectId Id { get; set; }
        public string TextFieldOne { get; set; }

        public string TextFieldTwo { get; set; }

        public string TextFieldThree { get; set; }
        public int IntField { get; set; }
        public List<SecondDoc> Documents { get; set; }

        public static FirstDoc Create()
        {
            List<SecondDoc> documents = new() { SecondDoc.Create(20), SecondDoc.Create(20), SecondDoc.Create(20) };
            return new()
            {
                Id = BsonObjectId.NewObjectId(),
                TextFieldOne = "One",
                TextFieldTwo = "Two",
                TextFieldThree = "Three",
                IntField = 42,
                Documents = documents
            };
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FirstDoc);
        }

        public bool Equals(FirstDoc other)
        {
            return other != null &&
                   Id.Equals(other.Id) &&
                   TextFieldOne == other.TextFieldOne &&
                   TextFieldTwo == other.TextFieldTwo &&
                   TextFieldThree == other.TextFieldThree &&
                   IntField == other.IntField &&
                   Documents.SequenceEqual(other.Documents);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, TextFieldOne, TextFieldTwo, TextFieldThree, IntField, Documents);
        }
    }

    [BsonSerializable]
    public partial class SecondDoc : IEquatable<SecondDoc>
    {
        public string TextField { get; set; }
        public int IntField { get; set; }
        public List<ThirdDoc> Documents { get; set; }

        public static SecondDoc Create(int value)
        {
            List<ThirdDoc> documents = new() { ThirdDoc.Create(3.0), ThirdDoc.Create(3.0), ThirdDoc.Create(3.0) };
            return new() { TextField = "SecodDocTextField", IntField = value, Documents = documents };
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SecondDoc);
        }

        public bool Equals(SecondDoc other)
        {
            return other != null &&
                   TextField == other.TextField &&
                   IntField == other.IntField &&
                   Documents.SequenceEqual(other.Documents);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TextField, IntField, Documents);
        }
    }

    [BsonSerializable]
    public partial class ThirdDoc : IEquatable<ThirdDoc>
    {
        public string TextField { get; set; }
        public double DoubleField { get; set; }

        public static ThirdDoc Create(double value)
        {
            return new() { TextField = "ThirdDocTextField", DoubleField = value };
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ThirdDoc);
        }

        public bool Equals(ThirdDoc other)
        {
            return other != null &&
                   TextField == other.TextField &&
                   DoubleField == other.DoubleField;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TextField, DoubleField);
        }
    }

    [BsonSerializable]
    public partial class UpdatedFirstDoc
    {
        public string TextFieldOne { get; set; }
        public int IntField { get; set; }
        public List<UpatedSecondDoc> Documents { get; set; }

        public static UpdatedFirstDoc Create()
        {
            List<UpatedSecondDoc> documents = new() { UpatedSecondDoc.Create(), UpatedSecondDoc.Create() };
            return new()
            {
                TextFieldOne = "UpdatedOne",
                IntField = 24,
                Documents = documents
            };
        }
    }

    [BsonSerializable]
    public partial class UpatedSecondDoc
    {
        public string TextField { get; set; }
        public List<UpdatedThirdDoc> Documents { get; set; }

        public static UpatedSecondDoc Create()
        {
            List<UpdatedThirdDoc> documents = new() { UpdatedThirdDoc.Create(), UpdatedThirdDoc.Create() };
            return new() { TextField = "UpdatedSecodDocTextField", Documents = documents };
        }
    }

    [BsonSerializable]
    public partial class UpdatedThirdDoc
    {
        public string TextField { get; set; }
        public static UpdatedThirdDoc Create()
        {
            return new() { TextField = "UpdatedThirdDocTextField" };
        }
    }
    public partial class ClientUpdateTestBase : ClientTestBase
    {

        public async Task UpdateOne_Set_UpdateDoc(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var update = Update.Set(UpdatedFirstDoc.Create());
            var filter = BsonDocument.Empty;
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateSetCollection" + DateTimeOffset.UtcNow);
            var (result, before, after) = await UpdateOneAsync(items, filter, update, collection);
            Assert.Equal(1, result.Ok);
            Assert.Equal(1, result.N);
            Assert.Equal(1, result.Modified);
            Assert.Equal(items.Length, after.Count);



            var first = after[0];
            Assert.True(first.Documents.Count == 2);
            Assert.Equal("UpdatedOne", first.TextFieldOne);
            Assert.Equal(24, first.IntField);

            Assert.Equal("Two", first.TextFieldTwo);
            Assert.Equal("Three", first.TextFieldThree);
            foreach (var second in first.Documents)
            {
                Assert.True(second.Documents.Count == 2);
                Assert.Equal("UpdatedSecodDocTextField", second.TextField);
                foreach (var third in second.Documents)
                {
                    Assert.Equal(0, third.DoubleField);
                    Assert.Equal("UpdatedThirdDocTextField", third.TextField);
                }
            }
            Assert.True(before[1].Equals(after[1]));
            await collection.DropAsync();
        }
        public async Task UpdateMany_Set_UpdateDoc(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var update = Update.Set(UpdatedFirstDoc.Create());
            var filter = BsonDocument.Empty;
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateSetCollection" + DateTimeOffset.UtcNow);
            var (result, before, after) = await UpdateManyAsync(items, filter, update, collection);
            Assert.Equal(1, result.Ok);
            Assert.Equal(2, result.N);
            Assert.Equal(2, result.Modified);
            foreach (var first in after)
            {
                Assert.True(first.Documents.Count == 2);
                Assert.Equal("UpdatedOne", first.TextFieldOne);
                Assert.Equal(24, first.IntField);

                Assert.Equal("Two", first.TextFieldTwo);
                Assert.Equal("Three", first.TextFieldThree);
                foreach (var second in first.Documents)
                {
                    Assert.True(second.Documents.Count == 2);
                    Assert.Equal("UpdatedSecodDocTextField", second.TextField);

                    Assert.Equal(0, second.IntField);
                    foreach (var third in second.Documents)
                    {
                        Assert.Equal(0, third.DoubleField);
                        Assert.Equal("UpdatedThirdDocTextField", third.TextField);
                    }
                }
            }
            await collection.DropAsync();
        }

        public async Task UpdateDocuments_Insert_Find_UpdateOneSet_SameModel(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateSetCollectionSameModel" + DateTimeOffset.UtcNow);
            var before = await InsertAsync(items, collection);
            UpdateFirstDocuments(before);
            foreach (var item in before)
            {
                var filter = new BsonDocument("_id", item.Id);
                await collection.UpdateOneAsync(filter, Update.Set(item));
            }
            var after = await collection.Find(BsonDocument.Empty).ToListAsync();
            Assert.True(items.Length == after.Count);
            foreach (var first in after)
            {
                Assert.True(first.Documents.Count == 3);
                Assert.Equal("UpdatedOne", first.TextFieldOne);
                Assert.Equal(24, first.IntField);

                Assert.Equal("Two", first.TextFieldTwo);
                Assert.Equal("Three", first.TextFieldThree);
                foreach (var second in first.Documents)
                {
                    Assert.True(second.Documents.Count == 3);
                    Assert.Equal("UpdatedSecodDocTextField", second.TextField);

                    Assert.Equal(20, second.IntField);

                    foreach (var third in second.Documents)
                    {
                        Assert.Equal(3.0, third.DoubleField);
                        Assert.Equal("UpdatedThirdDocTextField", third.TextField);
                    }
                }
            }
            await collection.DropAsync();
            void UpdateFirstDocuments(IEnumerable<FirstDoc> documents)
            {
                foreach (var first in documents)
                {
                    first.TextFieldOne = "UpdatedOne";
                    first.IntField = 24;
                    foreach (var second in first.Documents)
                    {
                        second.TextField = "UpdatedSecodDocTextField";
                        foreach (var third in second.Documents)
                        {
                            third.TextField = "UpdatedThirdDocTextField";
                        }
                    }
                }
            }
        }
        [BsonSerializable]
        public partial class UpdateFirstIncOrMulOrMaxOrMinDoc
        {
            public int IntField { get; set; }
            public UpdateFirstIncOrMulOrMaxOrMinDoc(int intField)
            {
                IntField = intField;
            }
        }
        public async Task UpdateOne_Inc(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateIncCollection" + DateTimeOffset.UtcNow);
            var update = Update.Inc(new UpdateFirstIncOrMulOrMaxOrMinDoc(2));
            var (result, before, after) = await UpdateOneAsync(items, BsonDocument.Empty, update, collection);
            Assert.True(items.Length == after.Count);

            var first = after[0];
            Assert.True(first.Documents.Count == 3);
            Assert.Equal("One", first.TextFieldOne);
            Assert.Equal(44, first.IntField);

            Assert.Equal("Two", first.TextFieldTwo);
            Assert.Equal("Three", first.TextFieldThree);
            foreach (var second in first.Documents)
            {
                Assert.True(second.Documents.Count == 3);
                Assert.Equal("SecodDocTextField", second.TextField);

                Assert.Equal(20, second.IntField);

                foreach (var third in second.Documents)
                {
                    Assert.Equal(3.0, third.DoubleField);
                    Assert.Equal("ThirdDocTextField", third.TextField);
                }
            }
            await collection.DropAsync();
        }
        public async Task UpdateMany_Inc(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateIncCollection" + DateTimeOffset.UtcNow);
            var update = Update.Inc(new UpdateFirstIncOrMulOrMaxOrMinDoc(2));
            var (result, before, after) = await UpdateManyAsync(items, BsonDocument.Empty, update, collection);
            Assert.True(items.Length == after.Count);
            foreach (var first in after)
            {
                Assert.True(first.Documents.Count == 3);
                Assert.Equal("One", first.TextFieldOne);
                Assert.Equal(44, first.IntField);

                Assert.Equal("Two", first.TextFieldTwo);
                Assert.Equal("Three", first.TextFieldThree);
                foreach (var second in first.Documents)
                {
                    Assert.True(second.Documents.Count == 3);
                    Assert.Equal("SecodDocTextField", second.TextField);

                    Assert.Equal(20, second.IntField);

                    foreach (var third in second.Documents)
                    {
                        Assert.Equal(3.0, third.DoubleField);
                        Assert.Equal("ThirdDocTextField", third.TextField);
                    }
                }
            }
            await collection.DropAsync();
        }
        public async Task UpdateOne_Mul(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateMulCollection" + DateTimeOffset.UtcNow);
            var update = Update.Mul(new UpdateFirstIncOrMulOrMaxOrMinDoc(2));
            var (result, before, after) = await UpdateOneAsync(items, BsonDocument.Empty, update, collection);
            Assert.True(items.Length == after.Count);

            var first = after[0];
            Assert.True(first.Documents.Count == 3);
            Assert.Equal("One", first.TextFieldOne);
            Assert.Equal(84, first.IntField);

            Assert.Equal("Two", first.TextFieldTwo);
            Assert.Equal("Three", first.TextFieldThree);
            foreach (var second in first.Documents)
            {
                Assert.True(second.Documents.Count == 3);
                Assert.Equal("SecodDocTextField", second.TextField);

                Assert.Equal(20, second.IntField);

                foreach (var third in second.Documents)
                {
                    Assert.Equal(3.0, third.DoubleField);
                    Assert.Equal("ThirdDocTextField", third.TextField);
                }
            }
            await collection.DropAsync();
        }
        public async Task UpdateMany_Mul(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateMulCollection" + DateTimeOffset.UtcNow);
            var update = Update.Mul(new UpdateFirstIncOrMulOrMaxOrMinDoc(2));
            var (result, before, after) = await UpdateManyAsync(items, BsonDocument.Empty, update, collection);
            Assert.True(items.Length == after.Count);
            foreach (var first in after)
            {
                Assert.True(first.Documents.Count == 3);
                Assert.Equal("One", first.TextFieldOne);
                Assert.Equal(84, first.IntField);

                Assert.Equal("Two", first.TextFieldTwo);
                Assert.Equal("Three", first.TextFieldThree);
                foreach (var second in first.Documents)
                {
                    Assert.True(second.Documents.Count == 3);
                    Assert.Equal("SecodDocTextField", second.TextField);

                    Assert.Equal(20, second.IntField);

                    foreach (var third in second.Documents)
                    {
                        Assert.Equal(3.0, third.DoubleField);
                        Assert.Equal("ThirdDocTextField", third.TextField);
                    }
                }
            }
            await collection.DropAsync();
        }
        public async Task UpdateOne_Max_Ok(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateMaxCollection" + DateTimeOffset.UtcNow);
            var update = Update.Max(new UpdateFirstIncOrMulOrMaxOrMinDoc(100500));
            var (result, before, after) = await UpdateOneAsync(items, BsonDocument.Empty, update, collection);
            Assert.True(items.Length == after.Count);

            var first = after[0];
            Assert.True(first.Documents.Count == 3);
            Assert.Equal("One", first.TextFieldOne);
            Assert.Equal(100500, first.IntField);

            Assert.Equal("Two", first.TextFieldTwo);
            Assert.Equal("Three", first.TextFieldThree);
            foreach (var second in first.Documents)
            {
                Assert.True(second.Documents.Count == 3);
                Assert.Equal("SecodDocTextField", second.TextField);

                Assert.Equal(20, second.IntField);

                foreach (var third in second.Documents)
                {
                    Assert.Equal(3.0, third.DoubleField);
                    Assert.Equal("ThirdDocTextField", third.TextField);
                }
            }
            await collection.DropAsync();
        }
        public async Task UpdateMany_Max_Ok(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateMaxCollection" + DateTimeOffset.UtcNow);
            var update = Update.Max(new UpdateFirstIncOrMulOrMaxOrMinDoc(100500));
            var (result, before, after) = await UpdateManyAsync(items, BsonDocument.Empty, update, collection);
            Assert.True(items.Length == after.Count);
            foreach (var first in after)
            {
                Assert.True(first.Documents.Count == 3);
                Assert.Equal("One", first.TextFieldOne);
                Assert.Equal(100500, first.IntField);

                Assert.Equal("Two", first.TextFieldTwo);
                Assert.Equal("Three", first.TextFieldThree);
                foreach (var second in first.Documents)
                {
                    Assert.True(second.Documents.Count == 3);
                    Assert.Equal("SecodDocTextField", second.TextField);

                    Assert.Equal(20, second.IntField);

                    foreach (var third in second.Documents)
                    {
                        Assert.Equal(3.0, third.DoubleField);
                        Assert.Equal("ThirdDocTextField", third.TextField);
                    }
                }
            }
            await collection.DropAsync();
        }
        public async Task UpdateOne_Max_Fail(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateMaxCollection" + DateTimeOffset.UtcNow);
            var update = Update.Max(new UpdateFirstIncOrMulOrMaxOrMinDoc(1));
            var (result, before, after) = await UpdateOneAsync(items, BsonDocument.Empty, update, collection);
            Assert.Equal(0, result.Modified);
            Assert.True(items.SequenceEqual(after));
            await collection.DropAsync();
        }
        public async Task UpdateMany_Max_Fail(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateMaxCollection" + DateTimeOffset.UtcNow);
            var update = Update.Max(new UpdateFirstIncOrMulOrMaxOrMinDoc(1));
            var (result, before, after) = await UpdateManyAsync(items, BsonDocument.Empty, update, collection);
            Assert.Equal(0, result.Modified);
            Assert.True(items.SequenceEqual(after));
            await collection.DropAsync();
        }
        public async Task UpdateOne_Min_Ok(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateMinCollection" + DateTimeOffset.UtcNow);
            var update = Update.Min(new UpdateFirstIncOrMulOrMaxOrMinDoc(1));
            var (result, before, after) = await UpdateOneAsync(items, BsonDocument.Empty, update, collection);
            Assert.True(items.Length == after.Count);

            var first = after[0];
            Assert.True(first.Documents.Count == 3);
            Assert.Equal("One", first.TextFieldOne);
            Assert.Equal(1, first.IntField);

            Assert.Equal("Two", first.TextFieldTwo);
            Assert.Equal("Three", first.TextFieldThree);
            foreach (var second in first.Documents)
            {
                Assert.True(second.Documents.Count == 3);
                Assert.Equal("SecodDocTextField", second.TextField);

                Assert.Equal(20, second.IntField);

                foreach (var third in second.Documents)
                {
                    Assert.Equal(3.0, third.DoubleField);
                    Assert.Equal("ThirdDocTextField", third.TextField);
                }
            }
            await collection.DropAsync();
        }
        public async Task UpdateMany_Min_Ok(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateMinCollection" + DateTimeOffset.UtcNow);
            var update = Update.Min(new UpdateFirstIncOrMulOrMaxOrMinDoc(1));
            var (result, before, after) = await UpdateManyAsync(items, BsonDocument.Empty, update, collection);
            Assert.True(items.Length == after.Count);
            foreach (var first in after)
            {
                Assert.True(first.Documents.Count == 3);
                Assert.Equal("One", first.TextFieldOne);
                Assert.Equal(1, first.IntField);

                Assert.Equal("Two", first.TextFieldTwo);
                Assert.Equal("Three", first.TextFieldThree);
                foreach (var second in first.Documents)
                {
                    Assert.True(second.Documents.Count == 3);
                    Assert.Equal("SecodDocTextField", second.TextField);

                    Assert.Equal(20, second.IntField);

                    foreach (var third in second.Documents)
                    {
                        Assert.Equal(3.0, third.DoubleField);
                        Assert.Equal("ThirdDocTextField", third.TextField);
                    }
                }
            }
            await collection.DropAsync();
        }
        public async Task UpdateOne_Min_Fail(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateMinCollection" + DateTimeOffset.UtcNow);
            var update = Update.Min(new UpdateFirstIncOrMulOrMaxOrMinDoc(100500));
            var (result, before, after) = await UpdateOneAsync(items, BsonDocument.Empty, update, collection);
            Assert.Equal(0, result.Modified);
            Assert.True(items.SequenceEqual(after));
            await collection.DropAsync();
        }
        public async Task UpdateMany_Min_Fail(MongoClient client)
        {
            var items = new[] { FirstDoc.Create(), FirstDoc.Create() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateMinCollection" + DateTimeOffset.UtcNow);
            var update = Update.Min(new UpdateFirstIncOrMulOrMaxOrMinDoc(100500));
            var (result, before, after) = await UpdateManyAsync(items, BsonDocument.Empty, update, collection);
            Assert.Equal(0, result.Modified);
            Assert.True(items.SequenceEqual(after));
            await collection.DropAsync();
        }
        public async Task UpdateOne_SetOnInsert(MongoClient client)
        {
            var items = new[] { MakeBsonDoc(), MakeBsonDoc() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<BsonDocument>("UpdateSetOnInsertCollection" + DateTimeOffset.UtcNow);
            var update = Update.SetOnInsert(new BsonDocument("NewField", 42));
            var (result, before, after) = await UpdateOneAsync(items, new BsonDocument("SomeInt", 12), update, collection, new UpdateOptions(true));
            Assert.True(result.Upserted.Count == 1);
            Assert.True(after.Count == before.Count + 1);
            Assert.Equal(12, (int)after[2]["SomeInt"].Value);
            Assert.Equal(42, (int)after[2]["NewField"].Value);
            Assert.Equal(3, after[2].Count);
            await collection.DropAsync();
            BsonDocument MakeBsonDoc()
            {
                var doc = new BsonDocument("SomeInt", 10);
                doc.Add("SomeString", "test");
                return doc;
            }
        }
        public async Task UpdateMany_SetOnInsert(MongoClient client)
        {
            var items = new[] { MakeBsonDoc(), MakeBsonDoc() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<BsonDocument>("UpdateSetOnInsertCollection" + DateTimeOffset.UtcNow);
            var update = Update.SetOnInsert(new BsonDocument("NewField", 42));
            var (result, before, after) = await UpdateManyAsync(items, new BsonDocument("SomeInt", 12), update, collection, new UpdateOptions(true));
            Assert.True(result.Upserted?.Count == 1);
            Assert.True(after.Count == before.Count + 1);
            Assert.Equal(12, (int)after[2]["SomeInt"].Value);
            Assert.Equal(42, (int)after[2]["NewField"].Value);
            Assert.Equal(3, after[2].Count);
            await collection.DropAsync();
            BsonDocument MakeBsonDoc()
            {
                var doc = new BsonDocument("SomeInt", 10);
                doc.Add("SomeString", "test");
                return doc;
            }
        }
        public async Task UpdateOne_Rename(MongoClient client)
        {
            var items = new[] { MakeBsonDoc(), MakeBsonDoc() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<BsonDocument>("UpdateRenameCollection" + DateTimeOffset.UtcNow);
            var update = Update.Rename(new BsonDocument("SomeInt", "RenamedField"));
            var (result, before, after) = await UpdateOneAsync(items, BsonDocument.Empty, update, collection);
            Assert.Equal(1, result.Modified);

            Assert.True(after[0][2].Name.Equals("RenamedField"));
            Assert.Equal(10, (int)after[0][2].Value);

            Assert.True(after[1][1].Name.Equals("SomeInt"));
            Assert.Equal(10, (int)after[1][1].Value);

            await collection.DropAsync();
            BsonDocument MakeBsonDoc()
            {
                var doc = new BsonDocument("SomeInt", 10);
                doc.Add("SomeString", "test");
                return doc;
            }
        }
        public async Task UpdateMany_Rename(MongoClient client)
        {
            var items = new[] { MakeBsonDoc(), MakeBsonDoc() };
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<BsonDocument>("UpdateRenameCollection" + DateTimeOffset.UtcNow);
            var update = Update.Rename(new BsonDocument("SomeInt", "RenamedField"));
            var (result, before, after) = await UpdateManyAsync(items, BsonDocument.Empty, update, collection);
            Assert.Equal(2, result.Modified);

            Assert.True(after[0][2].Name.Equals("RenamedField"));
            Assert.Equal(10, (int)after[0][2].Value);

            Assert.True(after[1][2].Name.Equals("RenamedField"));
            Assert.Equal(10, (int)after[1][2].Value);

            await collection.DropAsync();
            BsonDocument MakeBsonDoc()
            {
                var doc = new BsonDocument("SomeInt", 10);
                doc.Add("SomeString", "test");
                return doc;
            }
        }
    }
}
