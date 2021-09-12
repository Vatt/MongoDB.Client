namespace MongoDb.Client.WebApi.Mongo
{
    public interface IMongoRepository<T>
    {
        ValueTask DeleteAsync(string id);
        IAsyncEnumerable<T> GetAllAsync();
        ValueTask<T> GetAsync(string id);
        ValueTask InsertAsync(T geoIp);
    }
}
