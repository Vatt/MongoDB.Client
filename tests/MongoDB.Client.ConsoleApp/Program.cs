using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Readers;
using MongoDB.Client.Writers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Net;
﻿using System;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Test;

namespace MongoDB.Client.ConsoleApp
{
    [BsonSerializable]
    public class TestData : IEquatable<TestData>
    {
        [BsonSerializable]
        public class InnerTestData
        {
            public int a1;
            public int a2;
            public int a3;
        }
        [BsonElementField(ElementName = "_id")]
        public BsonObjectId Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public TestData InnerData { get; set; }
        public override bool Equals(object? obj)
        {
            return obj is Data data && Equals(data);
        }

        public bool Equals(TestData? other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (InnerData == null && other.InnerData == null)
            {
                return true;
            }
            if ((InnerData != null && other.InnerData == null) || (InnerData == null && other.InnerData != null))
            {
                return false;
            }
            return EqualityComparer<BsonObjectId>.Default.Equals(Id, other.Id) && Name == other.Name && Age == other.Age && InnerData.Equals(other.InnerData);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Age);
        }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new MongoClient(new DnsEndPoint("centos0.mshome.net", 27017));
            var connectionInfo = await client.ConnectAsync(default);
            var result1 = await client.GetCursorAsync<GeoIp>(EmptyCollection, default).ToListAsync();
            var result2 = await client.GetCursorAsync<GeoIp>(NotEmptyCollection, default).ToListAsync();
            var result3 = await client.GetCursorAsync<GeoIp>(EmptyCollection, default).ToListAsync();
            var result4 = await client.GetCursorAsync<GeoIp>(NotEmptyCollection, default).ToListAsync();
            var client = new MongoClient(/*new DnsEndPoint("centos0.mshome.net", 27017)*/);

            var db = client.GetDatabase("TestDb");
            var collection1 = db.GetCollection<GeoIp>("TestCollection2");
            var collection2 = db.GetCollection<GeoIp>("TestCollection3");

            var filter = new BsonDocument();
            
            var result0 = await collection1.GetCursorAsync<GeoIp>( filter, default);
            var result1 = await collection2.GetCursorAsync<GeoIp>( filter, default);



            var filter2 = new BsonDocument("_id", new BsonObjectId("5f987814bf344ec7cc57294b"));
            var result2 = await collection1.GetCursorAsync<GeoIp>(filter2, default);

            Console.WriteLine();
        }

        static void Test()
        {
            ReadOnlyMemory<byte> file = File.ReadAllBytes("../../../ReaderTestCollection.bson");
            //ReadOnlyMemory<byte> file = File.ReadAllBytes("../../../Meteoritelandings.bson");
            var reader = new BsonReader(file);
            //IBsonSerializable serializator = new MongoDB.Client.Test.Generated.NasaMeteoriteLandingGeneratedSerializator();
            //IBsonSerializable serializator = new MongoDB.Client.Bson.Serialization.Generated.DocumentObjectGeneratedSerializator();

            //serializator.TryParse(ref reader, out var doc);
            //reader.TryParseDocument(null, out var document);
            //reader.TryParseDocument(null, out var document1);
            //reader.TryParseDocument(null, out var document2);
            //reader.TryParseDocument(null, out var document3);
            //reader.TryParseDocument(null, out var document4);
            //reader.TryParseDocument(null, out var document5);
        }

    }
}
