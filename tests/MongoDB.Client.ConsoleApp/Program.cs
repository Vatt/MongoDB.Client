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
            await LoadTest<GeoIp>(1024*1024, new[] { 512 });

            Console.WriteLine("Done");
        }
        static async Task LoadTest<T>(int requestCount, IEnumerable<int> parallelism) where T : IIdentified
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
            });

            var client = new MongoClient(new DnsEndPoint(host, 27017), loggerFactory);
           // var client = MongoExperimental.CreateWithExperimentalConnection(new DnsEndPoint(host, 27017), loggerFactory);
            await client.InitAsync();
            var db = client.GetDatabase("TestDb");
            var stopwatch = new Stopwatch();
            Console.WriteLine(typeof(T).Name);
            foreach (var item in parallelism)
            {
                Console.WriteLine("Start: " + item);
                var bench = new ComplexBenchmarkBase<T>(db, item, requestCount);
                bench.Setup();

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