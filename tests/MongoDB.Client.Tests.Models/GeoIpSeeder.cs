using MongoDB.Client.Bson.Document;
using System.Collections.Generic;

namespace MongoDB.Client.Tests.Models
{
    public class GeoIpSeeder
    {
        public IEnumerable<GeoIp> GenerateSeed(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return new GeoIp
                {
                    Id = BsonObjectId.NewObjectId(),
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
            }
        }
    }
}
