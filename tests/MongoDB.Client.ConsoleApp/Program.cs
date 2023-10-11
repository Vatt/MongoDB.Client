using System.Diagnostics;
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
    public class MappingTest : IBsonSerializer<MappingTest>
    {
        [BsonElement("strvalprop")]
        public string StringProperty { get; set; }

        [BsonElement("strvalfield")]
        public string StringField;

        public string Field;
        public string Property { get; set; }
        public static bool TryParseBson(ref BsonReader reader, out MappingTest message)
        {
            throw new NotImplementedException();
        }

        public static void WriteBson(ref BsonWriter writer, in MappingTest message)
        {
            throw new NotImplementedException();
        }
    }

    [BsonSerializable]
    public readonly partial struct TestModel
    {
        public BsonObjectId Id { get;}
        public int SomeId { get; }
        public string Name { get; }
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
            await FilterTest();
            //await LoadTest<GeoIp>(1024 * 1024, new[] { 512 });
            //await ReplicaSetConenctionTest<GeoIp>(1024*4, new[] { 4 }, false);
            //await TestShardedCluster();
            //await TestTransaction();
            //await TestStandalone();
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
            //await collection.InsertAsync(new TestModel(id1, "Test", 1));
            //await collection.InsertAsync(new TestModel(id2, "Test", 2));
            //await collection.InsertAsync(new TestModel(id3, "Test", 3));
            int[] arr = new int[] { 1, 2 ,3 };
            //var filter = ExpressionHelper.ParseExpression((TestModel x) => x.Id == id1 || x.Id == id2 || x.Id == id3);
            //var filter = ExpressionHelper.ParseExpression((TestModel x) => arr.Contains(x.SomeId) || x.Id == id1 && id2 == x.Id && 1 == x.SomeId && x.SomeId == 1);
            //var filter = ExpressionHelper.ParseExpression((TestModel x) => arr.Contains(x.SomeId));
            //var filter = FilterVisitor.BuildFilter((TestModel x) => arr.Contains(x.SomeId) || x.Id == id1 && x.Id == id2 && x.Id == id3);
            var mapping = MappingProvider<MappingTest>.Mapping;
            //var result1 = await collection.Find(x => x.Id == id1 && x.SomeId == 1 && x.SomeId == 1).ToListAsync();
            //var result2 = await collection.Find(x => arr.Contains(x.SomeId) ).ToListAsync();
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
            var items = new GeoIpSeeder().GenerateSeed(10000);
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
                var item = new GeoIpSeeder().GenerateSeed(1).First();
                var filter = new Bson.Document.BsonDocument("_id", item.Id);

                await collection.InsertAsync(item);
                var result = await collection.Find(filter).FirstOrDefaultAsync();
                await collection.DeleteOneAsync(filter);
            }

            async Task WithCommitTx(MongoCollection<GeoIp> collection)
            {
                var item = new GeoIpSeeder().GenerateSeed(1).First();
                var filter = new Bson.Document.BsonDocument("_id", item.Id);

                await using var transaction = client.StartTransaction();

                await collection.InsertAsync(transaction, item);
                var result = await collection.Find(transaction, filter).FirstOrDefaultAsync();
                await collection.DeleteOneAsync(transaction, filter);

                await transaction.CommitAsync();
            }


            async Task WithAbortTx(MongoCollection<GeoIp> collection)
            {
                var item = new GeoIpSeeder().GenerateSeed(1).First();
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

            var item = new GeoIpSeeder().GenerateSeed(1).First();
            var filter = new Bson.Document.BsonDocument("_id", item.Id);

            await collection.InsertAsync(item);
            var result = await collection.Find(filter).FirstOrDefaultAsync();
            await collection.DeleteOneAsync(filter);

            Console.WriteLine();
        }

        static async Task ReplicaSetConenctionTest<T>(int requestCount, IEnumerable<int> parallelism, bool useTransaction)
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
                var bench = new ComplexBenchmarkBase<T>(db, item, requestCount);
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
        static async Task LoadTest<T>(int requestCount, IEnumerable<int> parallelisms)
            where T : IIdentified, IBsonSerializer<T>
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            host = $"mongodb://{host}/?clientType=experimental&replicaSet=rs0&maxPoolSize=4";
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
                var bench = new ComplexBenchmarkBase<T>(db, parallelism, requestCount);
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
