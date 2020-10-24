using MongoDB.Client.Network;
using System.Net;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    public class MongoSessionFactory
    {
        private readonly EndPoint? _endpoint;
        private readonly NetworkConnectionFactory _networkFactory;
        public MongoSessionFactory(EndPoint? endpoint)
        {
            _endpoint = endpoint;
            _networkFactory = new NetworkConnectionFactory();
        }
        public async ValueTask<MongoSession?> ConnectAsync()
        {
            var connection = await _networkFactory.ConnectAsync(_endpoint);
            return new MongoSession(connection);
        }
    }
}
