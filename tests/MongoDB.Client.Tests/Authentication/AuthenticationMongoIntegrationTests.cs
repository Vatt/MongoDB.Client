using MongoDB.Client.Tests.Infrastructure;

namespace MongoDB.Client.Tests.Authentication
{
    public class AuthenticationMongoIntegrationTests : AuthenticationIntegrationTestBase
    {
        [RequiresStandaloneMongoFact]
        public async Task StandaloneAuth_AllowsCrudRoundTrip()
        {
            await AssertRoundTripAsync(
                IntegrationMongoConnectionStringBuilder.BuildStandalone(maxPoolSize: 1),
                topologyName: "standalone");
        }

        [RequiresStandaloneMongoFact]
        public async Task StandaloneAuth_WithDeadSeedBeforeLiveSeed_AllowsCrudRoundTrip()
        {
            var configuration = IntegrationMongoConnectionStringBuilder.IntegrationMongoConnectionStringConfiguration.FromEnvironment() with
            {
                StandaloneHost = $"127.0.0.1:27099,{IntegrationMongoConnectionStringBuilder.StandaloneHost}"
            };

            await AssertRoundTripAsync(
                IntegrationMongoConnectionStringBuilder.BuildStandalone(maxPoolSize: 1, configuration),
                topologyName: "standalone_multiseed");
        }

        [RequiresReplicaSetMongoFact]
        public async Task ReplicaSetAuth_AllowsCrudRoundTrip()
        {
            await AssertRoundTripAsync(
                IntegrationMongoConnectionStringBuilder.BuildReplicaSet(maxPoolSize: 1),
                topologyName: "replicaset");
        }

        [RequiresReplicaSetMongoFact]
        public async Task ReplicaSetAuth_WithInvalidPassword_FailsExplicitly()
        {
            var configuration = IntegrationMongoConnectionStringBuilder.IntegrationMongoConnectionStringConfiguration.FromEnvironment() with
            {
                Password = "definitely-wrong-password"
            };

            await AssertExplicitAuthenticationFailureAsync(
                IntegrationMongoConnectionStringBuilder.BuildReplicaSet(maxPoolSize: 1, configuration));
        }

        [RequiresReplicaSetMongoFact]
        public async Task ReplicaSetAuth_WithMultiplePooledConnections_InitializesMultipleAuthenticatedPrimaryConnections()
        {
            await AssertRoundTripWithMultipleOperationsAsync(
                IntegrationMongoConnectionStringBuilder.BuildReplicaSet(maxPoolSize: 6),
                topologyName: "replicaset_pool");
        }

        [RequiresShardedMongoFact]
        public async Task ShardedAuth_AllowsCrudRoundTrip()
        {
            await AssertRoundTripAsync(
                IntegrationMongoConnectionStringBuilder.BuildSharded(maxPoolSize: 1),
                topologyName: "sharded");
        }

        [RequiresShardedMongoFact]
        public async Task ShardedAuth_WithDeadSeedBeforeLiveSeed_AllowsCrudRoundTrip()
        {
            var configuration = IntegrationMongoConnectionStringBuilder.IntegrationMongoConnectionStringConfiguration.FromEnvironment() with
            {
                ShardedHost = $"127.0.0.1:27099,{IntegrationMongoConnectionStringBuilder.ShardedHost}"
            };

            await AssertRoundTripAsync(
                IntegrationMongoConnectionStringBuilder.BuildSharded(maxPoolSize: 1, configuration),
                topologyName: "sharded_multiseed");
        }

        [RequiresShardedMongoFact]
        public async Task ShardedAuth_WithInvalidPassword_FailsExplicitly()
        {
            var configuration = IntegrationMongoConnectionStringBuilder.IntegrationMongoConnectionStringConfiguration.FromEnvironment() with
            {
                Password = "definitely-wrong-password"
            };

            await AssertExplicitAuthenticationFailureAsync(
                IntegrationMongoConnectionStringBuilder.BuildSharded(maxPoolSize: 1, configuration));
        }

        [RequiresStandaloneMongoFact]
        public async Task StandaloneAuth_WithInvalidPassword_FailsExplicitly()
        {
            var configuration = IntegrationMongoConnectionStringBuilder.IntegrationMongoConnectionStringConfiguration.FromEnvironment() with
            {
                Password = "definitely-wrong-password"
            };

            await AssertExplicitAuthenticationFailureAsync(
                IntegrationMongoConnectionStringBuilder.BuildStandalone(maxPoolSize: 1, configuration));
        }

        [RequiresStandaloneMongoFact]
        public async Task StandaloneAuth_WithInvalidAuthSource_FailsExplicitly()
        {
            var configuration = IntegrationMongoConnectionStringBuilder.IntegrationMongoConnectionStringConfiguration.FromEnvironment() with
            {
                AuthSource = "definitely_wrong_auth_source"
            };

            await AssertExplicitAuthenticationFailureAsync(
                IntegrationMongoConnectionStringBuilder.BuildStandalone(maxPoolSize: 1, configuration));
        }

        [RequiresStandaloneMongoFact]
        public async Task StandaloneAuth_WithInvalidPassword_AndMultiEndpointUri_FailsExplicitlyAfterEndpointIteration()
        {
            var configuration = IntegrationMongoConnectionStringBuilder.IntegrationMongoConnectionStringConfiguration.FromEnvironment() with
            {
                StandaloneHost = $"127.0.0.1:27099,{IntegrationMongoConnectionStringBuilder.StandaloneHost}",
                Password = "definitely-wrong-password"
            };

            await AssertExplicitAuthenticationFailureAsync(
                IntegrationMongoConnectionStringBuilder.BuildStandalone(maxPoolSize: 1, configuration));
        }

        [RequiresStandaloneMongoFact]
        public async Task StandaloneAuth_WithInvalidPassword_AndLiveEndpointBeforeDeadSeed_FailsExplicitlyAfterEndpointIteration()
        {
            var configuration = IntegrationMongoConnectionStringBuilder.IntegrationMongoConnectionStringConfiguration.FromEnvironment() with
            {
                StandaloneHost = $"{IntegrationMongoConnectionStringBuilder.StandaloneHost},127.0.0.1:27099",
                Password = "definitely-wrong-password"
            };

            await AssertExplicitAuthenticationFailureAsync(
                IntegrationMongoConnectionStringBuilder.BuildStandalone(maxPoolSize: 1, configuration));
        }
    }
}
