﻿using MongoDB.Bson;
using MongoDB.Client.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class MediumModel : IIdentified
    {
        [BsonId]
        [MongoDB.Bson.Serialization.Attributes.BsonIgnore]
        public Bson.Document.BsonObjectId Id { get; set; }

        [MongoDB.Bson.Serialization.Attributes.BsonId]
        [BsonIgnore]
        public ObjectId OldId { get; set; }

        public List<GeoIpForMedium> GeoIps { get; set; }
    }

    [BsonSerializable]
    public partial class GeoIpForMedium
    {
        public string status { get; set; }
        public string country { get; set; }
        public string countryCode { get; set; }
        public string region { get; set; }
        public string regionName { get; set; }
        public string city { get; set; }
        public int zip { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string timezone { get; set; }
        public string isp { get; set; }
        public string org { get; set; }
        public string query { get; set; }
    }
}
