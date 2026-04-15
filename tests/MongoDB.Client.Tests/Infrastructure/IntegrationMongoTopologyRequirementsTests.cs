using System.Net;
using System.Net.Sockets;
using Xunit;

namespace MongoDB.Client.Tests.Infrastructure
{
    public class IntegrationMongoTopologyRequirementsTests
    {
        [Fact]
        public async Task GetUnavailableReasonAsync_WhenSocketProbeFails_ReturnsSkipReason()
        {
            var endpoint = new DnsEndPoint("localhost", 27016);

            var skipReason = await IntegrationMongoTopologyRequirements.GetUnavailableReasonAsync(
                endpoint,
                "standalone MongoDB",
                static (_, _) => Task.FromException(new SocketException((int)SocketError.ConnectionRefused)));

            Assert.NotNull(skipReason);
            Assert.Contains("standalone MongoDB", skipReason);
            Assert.Contains("localhost:27016", skipReason);
        }

        [Fact]
        public async Task GetUnavailableReasonAsync_WhenProbeTimesOut_ReturnsSkipReason()
        {
            var endpoint = new DnsEndPoint("localhost", 27016);

            var skipReason = await IntegrationMongoTopologyRequirements.GetUnavailableReasonAsync(
                endpoint,
                "standalone MongoDB",
                static (_, _) => Task.FromException(new TimeoutException("probe timed out")));

            Assert.NotNull(skipReason);
            Assert.Contains("probe timed out", skipReason);
        }

        [Fact]
        public async Task GetUnavailableReasonAsync_WhenProbeSucceeds_ReturnsNull()
        {
            var endpoint = new DnsEndPoint("localhost", 27016);

            var skipReason = await IntegrationMongoTopologyRequirements.GetUnavailableReasonAsync(
                endpoint,
                "standalone MongoDB",
                static (_, _) => Task.CompletedTask);

            Assert.Null(skipReason);
        }

        [Fact]
        public async Task GetUnavailableReasonAsync_WhenProbeFailsForUnexpectedReason_RethrowsOriginalException()
        {
            var endpoint = new DnsEndPoint("localhost", 27016);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                IntegrationMongoTopologyRequirements.GetUnavailableReasonAsync(
                    endpoint,
                    "standalone MongoDB",
                    static (_, _) => Task.FromException(new InvalidOperationException("auth failure"))));

            Assert.Equal("auth failure", exception.Message);
        }
    }
}
