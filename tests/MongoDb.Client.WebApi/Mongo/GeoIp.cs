using MongoDB.Bson;
using MongoDB.Client.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace MongoDb.Client.WebApi
{
    [BsonSerializable]
    public partial class GeoIp
    {
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        [BsonIgnore]
        [JsonIgnore]
        public ObjectId OldId { get; set; }

        [BsonId]
        [MongoDB.Bson.Serialization.Attributes.BsonIgnore]
        [JsonIgnore]
        public MongoDB.Client.Bson.Document.BsonObjectId NewId { get; set; }

        [MongoDB.Bson.Serialization.Attributes.BsonIgnore]
        [BsonIgnore]
        public string id
        {
            get
            {
                if (NewId != default)
                {
                    return NewId.ToString();
                }
                if (OldId != default)
                {
                    return OldId.ToString();
                }
                return null;
            }
            set
            {
                OldId = ObjectId.Parse(value);
                NewId = new MongoDB.Client.Bson.Document.BsonObjectId(value);
            }
        }

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
