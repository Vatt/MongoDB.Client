using Microsoft.Extensions.Logging;
using MongoDB.Client.Experimental;
using MongoDB.Client.Tests.Models;
using MongoDB.Client.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace MongoDB.Client.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //await LoadTest<GeoIp>(1024 * 1024, new[] { 512 });

            //Console.WriteLine("Done");
            UriTest();
        }
        static void UriTest()
        {
            var uri = "mongodb://gamover:12345@centos1.mshome.net:3340 , centos2.mshome.net,centos3.mshome.net/?replicaSet=rs0&maxPoolSize=32&appName=MongoDB.Client.ConsoleApp";
            MongoDBUriParser.ParseUri(uri);
        }
        static async Task LoadTest<T>(int requestCount, IEnumerable<int> parallelism) where T : IIdentified
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Error)
                    .AddConsole();
            });

            //var client = new MongoClient(new DnsEndPoint(host, 27017), loggerFactory);
            var client = MongoExperimental.CreateWithExperimentalConnection(new DnsEndPoint(host, 27017), loggerFactory);
            await client.InitAsync();
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
                    await bench.Run();
                    stopwatch.Stop();
                }
                finally
                {
                    await bench.Clean();
                }

                Console.WriteLine($"End: {item}. Elapsed: {stopwatch.Elapsed}");
            }

            Console.WriteLine("Done");
        }
    }
}