using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Filters;
using MongoDB.Client.Settings;
using MongoDB.Client.Tests.Models;

namespace MongoDB.Client.ConsoleApp
{
    [BsonSerializable]
    public readonly partial struct TestModel
    {
        [BsonElement("_ID_")] public BsonObjectId Id { get;}
        [BsonElement("_SomeId_")] public int SomeId { get; }
        [BsonElement("_Name_")] public string Name { get; }
        public TestModel(BsonObjectId id, string name, int someId)
        {
            Id = id;
            Name = name;
            SomeId = someId;
        }
    }
    [BsonSerializable]
    public readonly partial record struct UpdateDoc(int SomeId);

    [BsonSerializable]
    public readonly partial record struct SetOnInsertUpdateDoc(int SOMEID);

    [BsonSerializable]
    public readonly partial record struct RenameDoc(string SomeId);

    [BsonSerializable]
    public readonly partial record struct UnsetDoc(string SOMEID, string Name);
    class Program
    {
        static async Task Main(string[] args)
        {
            //var update = Update<TestModel>.Set(new {SomeId = 22});
            //await TestUpdate();
            //await LoadTest<GeoIp>(1024 * 1024, new[] { 8 });
            await FilterTest();
            //await LoadTest<GeoIp>(1024 * 1024, new[] { 512 });
            //await ReplicaSetConenctionTest<GeoIp>(1024*4, new[] { 4 }, false);
            //await TestShardedCluster();
            //await TestTransaction();
            //await TestStandalone();
        }
        class Wrapper
        {
            public WrappedArray WrappedArrayField = new();
            public WrappedArray WrappedArrayProperty { get; } = new();
            public WrappedInt32 WrappedInt32 { get; } = new();
        }
        class WrappedArray
        {
            public int[] FieldArray = new[] { 1, 2, 3, 4, 5 };
            public int[] PropertyArray { get; } = new[] { 5, 4, 3, 2, 1 };
        }
        class WrappedInt32
        {
            public int Value { get; } = 1;
        }
        private static int[] arr = new int[] { 1, 2, 3 };
        private static BsonObjectId id1 = BsonObjectId.NewObjectId();
        private static BsonObjectId id2 = BsonObjectId.NewObjectId();
        private static BsonObjectId id3 = BsonObjectId.NewObjectId();
        private static bool boolVar = false;
        private static IList<int> list = new List<int>() { 1, 2, 3 };
        private static IEnumerable<int> enumerable = new List<int>() { 1, 2, 3 };
        private static Wrapper wrapper { get; set; } = new();
        public static Filter Test()
        {
            var filter = Filter.FromExpression((TestModel x) => wrapper.WrappedArrayProperty.PropertyArray.Contains(x.SomeId) == boolVar ||
                                                            wrapper.WrappedArrayProperty.FieldArray.Contains(x.SomeId) != true ||
                                                            wrapper.WrappedArrayField.FieldArray.Contains(x.SomeId) != true ||
                                                            wrapper.WrappedArrayField.PropertyArray.Contains(x.SomeId) != boolVar &&
                                                            x.SomeId != wrapper.WrappedInt32.Value &&
                                                            wrapper.WrappedInt32.Value == x.SomeId &&
                                                            wrapper.WrappedInt32.Value != x.SomeId &&
                                                            wrapper.WrappedInt32.Value > x.SomeId &&
                                                            wrapper.WrappedInt32.Value < x.SomeId &&
                                                            wrapper.WrappedInt32.Value <= x.SomeId &&
                                                            wrapper.WrappedInt32.Value >= x.SomeId &&
                                                            arr.Contains(x.SomeId) &&
                                                            x.Id == id1 &&
                                                            id2 == x.Id &&
                                                            1 == x.SomeId ||
                                                            x.SomeId == 1);
            return filter;
        }
        static async Task FilterTest()
        {
            //var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            //var loggerFactory = LoggerFactory.Create(builder =>
            //{
            //    builder
            //        .SetMinimumLevel(LogLevel.Information)
            //        .AddConsole();
            //});
            //var settings = MongoClientSettings.FromConnectionString($"mongodb://{host}/?maxPoolSize=1");
            //var client = await MongoClient.CreateClient(settings, loggerFactory);
            //var db = client.GetDatabase("TestDb");
            //var collection = db.GetCollection<TestModel>("TestCollection");
            var id1 = BsonObjectId.NewObjectId();
            var id2 = BsonObjectId.NewObjectId();
            var id3 = BsonObjectId.NewObjectId();
            BsonObjectId? id4 = BsonObjectId.NewObjectId();
            BsonObjectId? id5 = null;
            var boolVar = false;
            //await collection.InsertAsync(new TestModel(id1, "Test", 1));
            //await collection.InsertAsync(new TestModel(id2, "Test", 2));
            //await collection.InsertAsync(new TestModel(id3, "Test", 3));
            //int[] arr = new int[] { 1, 2 ,3 };
            //var wrapper = new Wrapper();
            //var filter = Filter.FromExpression((TestModel x) => (x.SomeId == 1 && x.SomeId == 2) || (x.SomeId == 3 && x.SomeId == 4));
            //var filter = Test();
            //var filter = Filter.FromExpression((TestModel x) => arr.Contains(x.SomeId) || x.Id == id1 && id2 == x.Id && 1 == x.SomeId && x.SomeId == 1);
            //var filter = Filter.FromExpression((TestModel x) => list.Contains(x.SomeId));
            //var filter = Filter.FromExpression((TestModel x) => x.Name.Contains("asd"));
            var filter = Filter.FromExpression((TestModel x) => "asd".Contains(x.Name));
            //var filter = Test();
            //var filter = Filter.FromExpression((TestModel x) => x.SomeId == 1 && x.SomeId == 2 || x.SomeId == 3);
            //var filter = Filter.FromExpression((TestModel x) => arr.Contains(x.SomeId) || x.Id == id1 && x.Id == id2 && x.Id == id3);
            //var result1 = await collection.Find(x => x.Id == id1 && x.SomeId == 1 && x.SomeId == 1).ToListAsync();

            //var result2 = await collection.Find(x => wrapper.WrappedArray.FieldArray.Contains(x.SomeId) ).ToListAsync();
            //var result3 = await collection.Find(x => x.SomeId < 2).ToListAsync();
            //var result4 = await collection.Find(Filter.Empty).ToListAsync();
            //await collection.DropAsync();
        }
        static async Task TestUpdate()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
            });
            var settings = MongoClientSettings.FromConnectionString($"mongodb://{host}");
            var client = await MongoClient.CreateClient(settings, loggerFactory);
            var db = client.GetDatabase("TestDb");
            var collection = db.GetCollection<TestModel>("TestCollection");
            await collection.InsertAsync(new TestModel(BsonObjectId.NewObjectId(), "Test", 42));
            await collection.InsertAsync(new TestModel(BsonObjectId.NewObjectId(), "Test", 42));
            await collection.InsertAsync(new TestModel(BsonObjectId.NewObjectId(), "Test", 42));
            var result = await collection.UpdateManyAsync(new BsonDocument("Name", "Test"), Update.Set(new UpdateDoc(24)));
            result = await collection.UpdateManyAsync(new BsonDocument("Name", "Test"), Update.Inc(new UpdateDoc(24)));
            result = await collection.UpdateManyAsync(new BsonDocument("Name", "Test"), Update.Max(new UpdateDoc(49)));
            result = await collection.UpdateManyAsync(new BsonDocument("Name", "Test"), Update.Min(new UpdateDoc(21)));
            result = await collection.UpdateManyAsync(new BsonDocument("Name", "Test"), Update.Mul(new UpdateDoc(2)));
            result = await collection.UpdateManyAsync(new BsonDocument("Name", "Test1"), Update.SetOnInsert(new UpdateDoc(24)), new UpdateOptions(true));
            result = await collection.UpdateOneAsync(new BsonDocument("Name", "Test2"), Update.SetOnInsert(new SetOnInsertUpdateDoc(2)), new UpdateOptions(true));
            result = await collection.UpdateManyAsync(new BsonDocument("Name", "Test"), Update.Rename(new RenameDoc("SOMEID")));
            result = await collection.UpdateManyAsync(new BsonDocument("Name", "Test"), Update.Unset(new UnsetDoc("", "")));
            result = await collection.UpdateManyAsync(new BsonDocument("Name", "Test"), Update.Unset("Name"));
            await collection.DropAsync();

        }
        static async Task TestShardedCluster()
        {
            var items = new GeoIpSeeder().GenerateSeed(SeederOptions.Create(10000));
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
            });
            var client = await MongoClient.CreateClient("mongodb://centos2.mshome.net:27029, centos2.mshome.net:27030, centos2.mshome.net:27031/?maxPoolSize=9&appName=MongoDB.Client.ConsoleApp");
            //var client = await MongoClient.CreateClient(new DnsEndPoint("centos0.mshome.net", 27017), loggerFactory); ;
            var db = client.GetDatabase("TestDb");
            var collection = db.GetCollection<GeoIp>("TestCollection");
            // await collection.InsertAsync(items);
            var cursor = collection.Find(BsonDocument.Empty);
            var lst = await cursor.ToListAsync();
            //await collection.DeleteOneAsync(BsonDocument.Empty);
        }
        static async Task TestTransaction()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
            });
            var client = await MongoClient.CreateClient("mongodb://centos.mshome.net:27018,centos.mshome.net:27019,centos.mshome.net:27020/?replicaSet=rs0&maxPoolSize=9&appName=MongoDB.Client.ConsoleApp&readPreference=secondaryPreferred", loggerFactory);
            // var client = await MongoClient.CreateClient("mongodb://centos1.mshome.net,centos2.mshome.net,centos3.mshome.net/?replicaSet=rs0&maxPoolSize=9&appName=MongoDB.Client.ConsoleApp&readPreference=secondaryPreferred", loggerFactory);
            var db = client.GetDatabase("TestDb");

            try
            {
                await db.DropCollectionAsync("TransactionCollection");
            }
            catch (MongoCommandException e) when (e.Code == 26)
            {
                // skip
            }
            await db.CreateCollectionAsync("TransactionCollection");
            var collection = db.GetCollection<GeoIp>("TransactionCollection");

            await WithoutTx(collection);
            await WithCommitTx(collection);
            await WithAbortTx(collection);

            Console.WriteLine();

            async Task WithoutTx(MongoCollection<GeoIp> collection)
            {
                var item = new GeoIpSeeder().GenerateSeed(SeederOptions.Create(1)).First();
                var filter = new Bson.Document.BsonDocument("_id", item.Id);

                await collection.InsertAsync(item);
                var result = await collection.Find(filter).FirstOrDefaultAsync();
                await collection.DeleteOneAsync(filter);
            }

            async Task WithCommitTx(MongoCollection<GeoIp> collection)
            {
                var item = new GeoIpSeeder().GenerateSeed(SeederOptions.Create(1)).First();
                var filter = new Bson.Document.BsonDocument("_id", item.Id);

                await using var transaction = client.StartTransaction();

                await collection.InsertAsync(transaction, item);
                var result = await collection.Find(transaction, filter).FirstOrDefaultAsync();
                await collection.DeleteOneAsync(transaction, filter);

                await transaction.CommitAsync();
            }


            async Task WithAbortTx(MongoCollection<GeoIp> collection)
            {
                var item = new GeoIpSeeder().GenerateSeed(SeederOptions.Create(1)).First();
                var filter = new Bson.Document.BsonDocument("_id", item.Id);

                await using var transaction = client.StartTransaction();

                await collection.InsertAsync(transaction, item);
                var result = await collection.Find(transaction, filter).FirstOrDefaultAsync();
                await collection.DeleteOneAsync(transaction, filter);

                await transaction.AbortAsync();
            }
        }

        static async Task TestStandalone()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
            });

            var client = await MongoClient.CreateClient(new DnsEndPoint(host, 27017), loggerFactory);
            var db = client.GetDatabase("TestDb");
            try
            {
                await db.DropCollectionAsync("TransactionCollection");
            }
            catch (MongoCommandException e) when (e.Code == 26)
            {
                // skip
            }

            await db.CreateCollectionAsync("TransactionCollection");
            var collection = db.GetCollection<GeoIp>("TransactionCollection");

            var item = new GeoIpSeeder().GenerateSeed(SeederOptions.Create(1)).First();
            var filter = new Bson.Document.BsonDocument("_id", item.Id);

            await collection.InsertAsync(item);
            var result = await collection.Find(filter).FirstOrDefaultAsync();
            await collection.DeleteOneAsync(filter);

            Console.WriteLine();
        }

        static async Task ReplicaSetConenctionTest<T>(uint requestCount, IEnumerable<int> parallelism, bool useTransaction)
            where T : IIdentified, IBsonSerializer<T>
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
            });
            //var client = await MongoClient.CreateClient("mongodb://centos.mshome.net:27018,centos.mshome.net:27019,centos.mshome.net:27020/?replicaSet=rs0&maxPoolSize=9&appName=MongoDB.Client.ConsoleApp&readPreference=Primary", loggerFactory);
            var client = await MongoClient.CreateClient("mongodb://centos1.mshome.net:27020,centos1.mshome.net:27018,centos1.mshome.net:27017/?replicaSet=rs0&maxPoolSize=9&appName=MongoDB.Client.ConsoleApp&readPreference=Primary", loggerFactory);
            var db = client.GetDatabase("TestDb");
            var stopwatch = new Stopwatch();
            Console.WriteLine(typeof(T).Name);
            foreach (var item in parallelism)
            {
                Console.WriteLine("Start: " + item);
                var bench = new ComplexBenchmarkBase<T>(db, item, loggerFactory.CreateLogger<ComplexBenchmarkBase<T>>(), SeederOptions.Create(requestCount));
                await bench.Setup(); 

                stopwatch.Restart();
                try
                {
                    await bench.Run(useTransaction);
                    stopwatch.Stop();
                }
                finally
                {
                    await bench.Clean();
                }

                Console.WriteLine($"End: {item}. Elapsed: {stopwatch.Elapsed}");
            }
        }
        static async Task LoadTest<T>(uint requestCount, IEnumerable<int> parallelisms)
            where T : IIdentified, IBsonSerializer<T>
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            host = $"mongodb://{host}/?maxPoolSize=4";
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
            });
            var settings = MongoClientSettings.FromConnectionString(host);
            var client = await MongoClient.CreateClient(settings, loggerFactory);
            var db = client.GetDatabase("TestDb");
            var stopwatch = new Stopwatch();
            Console.WriteLine(typeof(T).Name);
            foreach (var parallelism in parallelisms)
            {
                Console.WriteLine("Start: " + parallelism);
                var bench = new ComplexBenchmarkBase<T>(db, parallelism, loggerFactory.CreateLogger<ComplexBenchmarkBase<T>>(), SeederOptions.CreateInfinite());
                await bench.Setup();

                stopwatch.Restart();
                try
                {
                    await bench.Run(false);
                    stopwatch.Stop();
                }
                finally
                {
                    await bench.Clean();
                }

                Console.WriteLine($"End: {parallelism}. Elapsed: {stopwatch.Elapsed}");
            }

            Console.WriteLine("Done");
        }
    }
}
