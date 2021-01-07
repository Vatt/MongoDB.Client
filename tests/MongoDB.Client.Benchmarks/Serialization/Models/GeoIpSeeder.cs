using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Benchmarks.Serialization.Models
{
    internal class GeoIpSeeder
    {
        public IEnumerable<GeoIp> GenerateSeed(int count = 500)
        {
            Console.WriteLine("Seeding a database for experiment....");
            var list = new List<GeoIp>();
            for (var i = 0; i < count; i++)
            {
                list.Add(new GeoIp
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
                });
            }
            return list;
        }
    }
}
