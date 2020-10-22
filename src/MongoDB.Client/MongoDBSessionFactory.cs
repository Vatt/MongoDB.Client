using MongoDB.Client.Network;
using System.Net;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    public class MongoDBSessionFactory
    {
        private readonly EndPoint? _endpoint;
        private readonly NetworkConnectionFactory _networkFactory;
        public MongoDBSessionFactory(EndPoint? endpoint)
        {
            _endpoint = endpoint;
            _networkFactory = new NetworkConnectionFactory();
        }
        public async ValueTask<MongoDBSession?> ConnectAsync()
        {
            var connection = await _networkFactory.ConnectAsync(_endpoint);
            return new MongoDBSession(connection);
        }
    }
}
