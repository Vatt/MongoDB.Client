using MongoDB.Client.Bson.Document;
using MongoDB.Client.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDb.Client.WebApi
{
    public interface IMongo
    {
        Task StartAsync(CancellationToken cancellationToken);
        ValueTask<DeleteResult> DeleteAsync(BsonObjectId id);
        IAsyncEnumerable<GeoIp> GetAllAsync();
        ValueTask<GeoIp> GetAsync(BsonObjectId id);
        ValueTask InsertAsync(GeoIp geoIp);
    }
}