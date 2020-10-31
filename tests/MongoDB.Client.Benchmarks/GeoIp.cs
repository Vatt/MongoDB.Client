using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Benchmarks
{
    [BsonSerializable]
    public class GeoIp
    {
        [BsonId]
        [Bson.Serialization.Attributes.BsonIgnore]
        public ObjectId OldId { get; set; }
        
        [BsonElementField(ElementName = "_id")]
        [MongoDB.Bson.Serialization.Attributes.BsonIgnore]
        public MongoDB.Client.Bson.Document.BsonObjectId Id { get; set; }
        
        
        public string status { get; set; }
        public string country { get; set; }
        public string countryCode { get; set; }
        public string region { get; set; }
        public string regionName { get; set; }
        public string city { get; set; }
        public long zip { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string timezone { get; set; }
        public string isp { get; set; }
        public string org { get; set; }
        public string query { get; set; }
    }
}
