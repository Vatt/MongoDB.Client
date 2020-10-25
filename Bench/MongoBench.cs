using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Driver;

namespace Bench
{
    [MemoryDiagnoser]
    public class MongoBench
    {


        //public Task Init()
        //{
        //    var collection = new MongoClient().GetDatabase("TestDb").GetCollection<>
        //}

        //[Benchmark]
        //public Task<int> Old()
        //{

        //}
    }
}
