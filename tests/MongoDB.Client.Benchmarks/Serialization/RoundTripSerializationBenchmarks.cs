using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;

namespace MongoDB.Client.Benchmarks.Serialization
{
    [MemoryDiagnoser]
    public class RoundTripSerializationBenchmarks
    {
        private static readonly BsonDocumentSerializer _serializer = new BsonDocumentSerializer();
        
        [Benchmark]
        public async Task<BsonDocument> BsonSerialization()
        {
            var doc = new BsonDocument
            {
                { "int", 42},
                { "bool", true},
                { "string1", "string"},
                { "string2", ""},
                { "string3", default(string)},
                {"array", new  BsonArray { "item1", default(string), 42, true } },
                { "inner", new BsonDocument {
                    {"innerString", "inner string" }
                } }
            };

            return await SerializationHelper.RoundTripAsync(doc, _serializer);
        }
    }
}
