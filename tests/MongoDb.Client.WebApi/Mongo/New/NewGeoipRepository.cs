namespace MongoDb.Client.WebApi.Mongo
{
    public class NewGeoipRepository : BaseNewRepository<GeoIp>
    {
        public NewGeoipRepository(INewMongo mongo) : base(mongo, "GeoipCollection")
        {
        }
    }
}
