using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDb.Client.WebApi.Mongo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoDb.Client.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeoipController : ControllerBase
    {
        private readonly IMongoRepository<GeoIp> _mongo;

        public GeoipController(IMongoRepository<GeoIp> mongo)
        {
            _mongo = mongo;
        }

        [HttpGet]
        public IAsyncEnumerable<GeoIp> GetAllAsync()
        {
            return _mongo.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<GeoIp> GetAsync(string id)
        {
            return await _mongo.GetAsync(id);
        }

        [HttpPost()]
        public async Task InsertAsync([FromBody] GeoIp model)
        {
            await _mongo.InsertAsync(model);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(string id)
        {
            await _mongo.DeleteAsync(id);
        }
    }
}
