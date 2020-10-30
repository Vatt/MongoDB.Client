using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Common;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    public class MongoClient
    {
        public EndPoint EndPoint { get; }

        public MongoClient(EndPoint? endPoint = null)
        {
            EndPoint = endPoint ?? new DnsEndPoint("localhost", 27017);
        }

        public MongoDatabase GetDatabase(string name)
        {
            return new MongoDatabase(this, name);
        }
    }
}
