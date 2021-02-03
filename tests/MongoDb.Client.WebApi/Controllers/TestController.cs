using Microsoft.AspNetCore.Mvc;
using MongoDB.Client.Bson.Document;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoDb.Client.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IMongo _mongo;

        public TestController(IMongo mongo)
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
            return await _mongo.GetAsync(new BsonObjectId(id));
        }

        [HttpPost()]
        public async Task<IActionResult> InsertAsync([FromBody] GeoIp model)
        {
            await _mongo.InsertAsync(model);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            var result = await _mongo.DeleteAsync(new BsonObjectId(id));
            if (result.Ok == 1)
            {
                return Ok();
            }
            return NotFound();
        }
    }
}
