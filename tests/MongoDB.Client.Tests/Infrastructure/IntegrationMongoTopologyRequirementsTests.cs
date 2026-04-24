using System.Net;
using System.Net.Sockets;
using MongoDB.Client.Messages;
using MongoDB.Client.Settings;
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
                static (_, _) => Task.FromException<MongoPingMessage>(new SocketException((int)SocketError.ConnectionRefused)),
                IntegrationMongoTopologyRequirements.IsStandaloneTopology);

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
                static (_, _) => Task.FromException<MongoPingMessage>(new TimeoutException("probe timed out")),
                IntegrationMongoTopologyRequirements.IsStandaloneTopology);

            Assert.NotNull(skipReason);
            Assert.Contains("probe timed out", skipReason);
        }

        [Fact]
        public async Task GetUnavailableReasonAsync_WhenProbeReturnsReplicaSetForStandalone_ReturnsSkipReason()
        {
            var endpoint = new DnsEndPoint("localhost", 27017);

            var skipReason = await IntegrationMongoTopologyRequirements.GetUnavailableReasonAsync(
                endpoint,
                "standalone MongoDB",
                static (_, _) => Task.FromResult(CreateReplicaSetPing()),
                IntegrationMongoTopologyRequirements.IsStandaloneTopology);

            Assert.NotNull(skipReason);
            Assert.Contains("standalone MongoDB", skipReason);
            Assert.Contains("localhost:27017", skipReason);
            Assert.Contains("replica set topology", skipReason);
        }

        [Fact]
        public async Task GetUnavailableReasonAsync_WhenStandaloneTopologyMatches_ReturnsNull()
        {
            var endpoint = new DnsEndPoint("localhost", 27016);

            var skipReason = await IntegrationMongoTopologyRequirements.GetUnavailableReasonAsync(
                endpoint,
                "standalone MongoDB",
                static (_, _) => Task.FromResult(CreateStandalonePing()),
                IntegrationMongoTopologyRequirements.IsStandaloneTopology);

            Assert.Null(skipReason);
        }

        [Fact]
        public async Task GetUnavailableReasonAsync_WhenProbeReturnsStandaloneForReplicaSet_ReturnsSkipReason()
        {
            var endpoint = new DnsEndPoint("localhost", 27016);

            var skipReason = await IntegrationMongoTopologyRequirements.GetUnavailableReasonAsync(
                endpoint,
                "replica set MongoDB",
                static (_, _) => Task.FromResult(CreateStandalonePing()),
                IntegrationMongoTopologyRequirements.IsReplicaSetTopology);

            Assert.NotNull(skipReason);
            Assert.Contains("replica set MongoDB", skipReason);
            Assert.Contains("standalone topology", skipReason);
        }

        [Fact]
        public async Task GetUnavailableReasonAsync_WhenReplicaSetTopologyMatches_ReturnsNull()
        {
            var endpoint = new DnsEndPoint("localhost", 27017);

            var skipReason = await IntegrationMongoTopologyRequirements.GetUnavailableReasonAsync(
                endpoint,
                "replica set MongoDB",
                static (_, _) => Task.FromResult(CreateReplicaSetPing()),
                IntegrationMongoTopologyRequirements.IsReplicaSetTopology);

            Assert.Null(skipReason);
        }

        [Fact]
        public async Task GetUnavailableReasonAsync_WhenShardedTopologyMatches_ReturnsNull()
        {
            var endpoint = new DnsEndPoint("localhost", 27029);

            var skipReason = await IntegrationMongoTopologyRequirements.GetUnavailableReasonAsync(
                endpoint,
                "sharded MongoDB",
                static (_, _) => Task.FromResult(CreateShardedPing()),
                IntegrationMongoTopologyRequirements.IsShardedTopology);

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
                    static (_, _) => Task.FromException<MongoPingMessage>(new InvalidOperationException("auth failure")),
                    IntegrationMongoTopologyRequirements.IsStandaloneTopology));

            Assert.Equal("auth failure", exception.Message);
        }

        [Fact]
        public async Task GetSkipReasonForTopologyAsync_WhenLaterEndpointMatches_ReturnsNull()
        {
            var endpoints = new EndPoint[]
            {
                new DnsEndPoint("localhost", 27018),
                new DnsEndPoint("localhost", 27017)
            };

            var skipReason = await IntegrationMongoTopologyRequirements.GetSkipReasonForTopologyAsync(
                endpoints,
                "replica set MongoDB",
                static (endpoint, _) =>
                {
                    if (((DnsEndPoint)endpoint).Port == 27018)
                    {
                        return Task.FromException<MongoPingMessage>(new SocketException((int)SocketError.ConnectionRefused));
                    }

                    return Task.FromResult(CreateReplicaSetPing());
                },
                IntegrationMongoTopologyRequirements.IsReplicaSetTopology);

            Assert.Null(skipReason);
        }

        [Fact]
        public async Task GetSkipReasonForTopologyAsync_WhenNoEndpointMatches_ReturnsLastSkipReason()
        {
            var endpoints = new EndPoint[]
            {
                new DnsEndPoint("localhost", 27018),
                new DnsEndPoint("localhost", 27016)
            };

            var skipReason = await IntegrationMongoTopologyRequirements.GetSkipReasonForTopologyAsync(
                endpoints,
                "replica set MongoDB",
                static (endpoint, _) =>
                {
                    if (((DnsEndPoint)endpoint).Port == 27018)
                    {
                        return Task.FromException<MongoPingMessage>(new SocketException((int)SocketError.ConnectionRefused));
                    }

                    return Task.FromResult(CreateStandalonePing());
                },
                IntegrationMongoTopologyRequirements.IsReplicaSetTopology);

            Assert.NotNull(skipReason);
            Assert.Contains("localhost:27016", skipReason);
            Assert.Contains("standalone topology", skipReason);
        }

        [Fact]
        public void ProbeMongoTopologyTimeouts_UseSeparateBudgets()
        {
            Assert.Equal(TimeSpan.FromSeconds(1), IntegrationMongoTopologyRequirements.DefaultSocketProbeTimeout);
            Assert.Equal(TimeSpan.FromSeconds(5), IntegrationMongoTopologyRequirements.DefaultAuthenticatedProbeTimeout);
            Assert.True(
                IntegrationMongoTopologyRequirements.DefaultAuthenticatedProbeTimeout > IntegrationMongoTopologyRequirements.DefaultSocketProbeTimeout);
        }

        [Fact]
        public async Task ProbeMongoTopologyAsync_RunsSocketProbeBeforeAuthenticatedProbe()
        {
            var settings = new MongoClientSettings(new DnsEndPoint("localhost", 27016), "admin", "root", "password");
            var endpoint = new DnsEndPoint("localhost", 27016);
            var calls = new List<string>();

            var ping = await IntegrationMongoTopologyRequirements.ProbeMongoTopologyAsync(
                settings,
                endpoint,
                async (probeEndpoint, token) =>
                {
                    calls.Add("socket");
                    await Task.Yield();
                    Assert.Equal("localhost", ((DnsEndPoint)probeEndpoint).Host);
                },
                async (probeSettings, probeEndpoint, token) =>
                {
                    calls.Add("auth");
                    await Task.Yield();
                    Assert.Equal("admin", probeSettings.AdminDB);
                    return CreateStandalonePing();
                },
                CancellationToken.None).ConfigureAwait(false);

            Assert.True(IntegrationMongoTopologyRequirements.IsStandaloneTopology(ping));
            Assert.Equal(new[] { "socket", "auth" }, calls);
        }

        [Fact]
        public async Task ProbeMongoTopologyAsync_WhenSocketProbeFails_DoesNotRunAuthenticatedProbe()
        {
            var settings = new MongoClientSettings(new DnsEndPoint("localhost", 27016), "admin", "root", "password");
            var endpoint = new DnsEndPoint("localhost", 27016);
            var authenticatedProbeCalled = false;

            await Assert.ThrowsAsync<SocketException>(() =>
                IntegrationMongoTopologyRequirements.ProbeMongoTopologyAsync(
                    settings,
                    endpoint,
                    static (_, _) => Task.FromException(new SocketException((int)SocketError.ConnectionRefused)),
                    (_, _, _) =>
                    {
                        authenticatedProbeCalled = true;
                        return Task.FromResult(CreateStandalonePing());
                    },
                    CancellationToken.None));

            Assert.False(authenticatedProbeCalled);
        }

        private static MongoPingMessage CreateStandalonePing()
        {
            return new MongoPingMessage(
                hosts: null!,
                setName: null!,
                message: null!,
                me: new DnsEndPoint("localhost", 27016),
                primary: null!,
                clusterTime: null!,
                isMaster: true,
                isSecondary: false);
        }

        private static MongoPingMessage CreateReplicaSetPing()
        {
            return new MongoPingMessage(
                hosts: new List<EndPoint>
                {
                    new DnsEndPoint("localhost", 27017)
                },
                setName: "rs0",
                message: null!,
                me: new DnsEndPoint("localhost", 27017),
                primary: new DnsEndPoint("localhost", 27017),
                clusterTime: null!,
                isMaster: true,
                isSecondary: false);
        }

        private static MongoPingMessage CreateShardedPing()
        {
            return new MongoPingMessage(
                hosts: null!,
                setName: null!,
                message: "isdbgrid",
                me: new DnsEndPoint("localhost", 27029),
                primary: null!,
                clusterTime: null!,
                isMaster: true,
                isSecondary: false);
        }
    }
}
