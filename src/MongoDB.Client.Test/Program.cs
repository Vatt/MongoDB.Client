﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Intrinsics;
using System.Threading.Tasks;
using MongoDB.Client.Network;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Network;

namespace MongoDB.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {

   
            //var factory = new NetworkConnectionFactory();
            //var connection = await factory.ConnectAsync(new DnsEndPoint("centos0.mshome.net", 27017));
            //var seq = await connection.Pipe.Input.ReadAsync();
            var root = new BsonDocument();
            var driverDoc = new BsonDocument();
       
            driverDoc.Elements.AddRange(new List<BsonElement>{
                BsonElement.Create(driverDoc, "driver", "MongoDB.Client"),
                BsonElement.Create(driverDoc, "version", "0.0.0"),
            });
            root.Elements.AddRange(new List<BsonElement>
            {
                BsonElement.Create(root, "driver", driverDoc)
            }); ;
            Test();

        }
        static unsafe void Test()
        {
            ReadOnlyMemory<byte> file = File.ReadAllBytes("../../../ReaderTestCollection.bson");
            var reader = new MongoDBBsonReader(file);

            List<BsonDocument> docs = new List<BsonDocument>();
            for (int i = 0; i < 45716; i++)
            {
                reader.TryParseDocument(null, out var adddoc);
                docs.Add(adddoc);
            }

        }
    }
}
