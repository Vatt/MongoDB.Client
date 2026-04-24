using System.Net;
using System.Net.Sockets;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Settings;
using Xunit;

namespace MongoDB.Client.Tests.Client
{
    public class MongoClientDiagnosticsTests
    {
        [Fact]
        public async Task CreateClient_WhenEndpointIsUnavailable_PreservesSocketExceptionAsInnerCause()
        {
            var settings = new MongoClientSettings(new IPEndPoint(IPAddress.Loopback, 1));

            var exception = await Assert.ThrowsAsync<MongoException>(() => MongoClient.CreateClient(settings));

            Assert.StartsWith("Connection failed", exception.Message, StringComparison.OrdinalIgnoreCase);
            var innerException = Assert.IsType<SocketException>(exception.InnerException);
            Assert.Equal(SocketError.ConnectionRefused, innerException.SocketErrorCode);
        }
    }
}
