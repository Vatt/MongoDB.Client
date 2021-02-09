using Microsoft.Extensions.Options;

namespace MongoDb.Client.WebApi.Mongo
{
    public class OldGeoipRepository : BaseOldRepository<GeoIp>
    {
        public OldGeoipRepository(IOptions<MongoConfig> options) : base(options, "GeoipCollection")
        {
        }
    }
}
