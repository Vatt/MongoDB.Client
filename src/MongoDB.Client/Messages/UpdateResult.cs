﻿using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class UpdateResult : IParserResult
    {
        [BsonElement("ok")]
        public double Ok { get; }
        
        [BsonElement("n")]
        public int N { get; }
        
        [BsonElement("nModified")]
        public int Modified { get; }
        
        [BsonElement("$clusterTime")]
        public MongoClusterTime ClusterTime { get; }
        
        [BsonElement("errmsg")]
        public string? ErrorMessage { get; }
        public UpdateResult( double ok, int n, int modified,  MongoClusterTime clusterTime, string errorMessage)
        {
            Modified = modified;
            Ok = ok;
            N = n;
            ClusterTime = clusterTime;
            ErrorMessage = errorMessage;
        }
    }
}