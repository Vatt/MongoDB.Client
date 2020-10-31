using System.Net;
using System.Threading.Tasks;
using MongoDB.Client.Network;

namespace MongoDB.Client.Unused
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
