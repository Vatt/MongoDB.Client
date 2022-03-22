using MongoDB.Bson;
using BsonObjectId = MongoDB.Client.Bson.Document.BsonObjectId;

namespace MongoDB.Client.Tests.Models
{
    public class MediumModelSeeder
    {
        public IEnumerable<MediumModel> GenerateSeed(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var model = new MediumModel
                {
                    Id = BsonObjectId.NewObjectId(),
                    OldId = ObjectId.GenerateNewId(),
                    GeoIps = new List<GeoIpForMedium>(),
                    Update = "old"
                };
                for (int j = 0; j < 50; j++)
                {
                    var geoip = new GeoIpForMedium
                    {
                        city = "St Petersburg",
                        country = "Russia",
                        countryCode = "RU",
                        isp = "NevalinkRoute",
                        lat = 59.8944f,
                        lon = 30.2642f,
                        org = "Nevalink Ltd.",
                        query = "31.134.191.87",
                        region = "SPE",
                        regionName = "St.-Petersburg",
                        status = "success",
                        timezone = "Europe/Moscow",
                        zip = 190000
                    };
                    model.GeoIps.Add(geoip);
                }
                yield return model;
            }
        }
    }
}
