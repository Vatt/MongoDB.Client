using System.Net;
using MongoDB.Client.Settings;
using Xunit;

namespace MongoDB.Client.Tests.Infrastructure
{
    public class IntegrationMongoConnectionStringBuilderTests
    {
        [Fact]
        public void BuildStandalone_DefaultConfiguration_AddsExplicitAuthDefaults()
        {
            var configuration = IntegrationMongoConnectionStringBuilder.IntegrationMongoConnectionStringConfiguration.CreateDefault();

            var connectionString = IntegrationMongoConnectionStringBuilder.BuildStandalone(maxPoolSize: 42, configuration);
            var settings = MongoClientSettings.FromConnectionString(connectionString);

            Assert.Equal("root", settings.Login);
            Assert.Equal("password", settings.Password);
            Assert.Equal("admin", settings.AdminDB);
            Assert.Equal("SCRAM-SHA-256", settings.AuthMechanism);
            Assert.Equal(42, settings.ConnectionPoolMaxSize);

            var endpoint = Assert.IsType<DnsEndPoint>(Assert.Single(settings.Endpoints));
            Assert.Equal("localhost", endpoint.Host);
            Assert.Equal(27016, endpoint.Port);
        }

        [Fact]
        public void BuildReplicaSet_DefaultConfiguration_AddsExplicitAuthDefaultsAndReplicaSet()
        {
            var configuration = IntegrationMongoConnectionStringBuilder.IntegrationMongoConnectionStringConfiguration.CreateDefault();

            var connectionString = IntegrationMongoConnectionStringBuilder.BuildReplicaSet(maxPoolSize: 24, configuration);
            var settings = MongoClientSettings.FromConnectionString(connectionString);

            Assert.Equal("root", settings.Login);
            Assert.Equal("password", settings.Password);
            Assert.Equal("admin", settings.AdminDB);
            Assert.Equal("SCRAM-SHA-256", settings.AuthMechanism);
            Assert.Equal("rs0", settings.ReplicaSet);
            Assert.Equal(24, settings.ConnectionPoolMaxSize);

            var endpoint = Assert.IsType<DnsEndPoint>(Assert.Single(settings.Endpoints));
            Assert.Equal("localhost", endpoint.Host);
            Assert.Equal(27017, endpoint.Port);
        }

        [Fact]
        public void BuildReplicaSet_CustomConfiguration_UsesSuppliedValuesWithoutEnvironment()
        {
            var configuration = new IntegrationMongoConnectionStringBuilder.IntegrationMongoConnectionStringConfiguration(
                StandaloneHost: "mongo-standalone.internal:28016",
                ReplicaSetHost: "mongo-rs.internal:28017",
                ShardedHost: "mongo-sharded.internal:28029",
                ReplicaSetName: "rs-custom",
                Username: "user@domain",
                Password: "p@ss:word",
                AuthSource: "admin-db",
                AuthMechanism: "SCRAM-SHA-1");

            var connectionString = IntegrationMongoConnectionStringBuilder.BuildReplicaSet(maxPoolSize: 7, configuration);
            var settings = MongoClientSettings.FromConnectionString(connectionString);

            Assert.Equal("user@domain", settings.Login);
            Assert.Equal("p@ss:word", settings.Password);
            Assert.Equal("admin-db", settings.AdminDB);
            Assert.Equal("SCRAM-SHA-1", settings.AuthMechanism);
            Assert.Equal("rs-custom", settings.ReplicaSet);
            Assert.Equal(7, settings.ConnectionPoolMaxSize);

            var endpoint = Assert.IsType<DnsEndPoint>(Assert.Single(settings.Endpoints));
            Assert.Equal("mongo-rs.internal", endpoint.Host);
            Assert.Equal(28017, endpoint.Port);
        }

        [Fact]
        public void BuildSharded_DefaultConfiguration_AddsExplicitAuthDefaults()
        {
            var configuration = IntegrationMongoConnectionStringBuilder.IntegrationMongoConnectionStringConfiguration.CreateDefault();

            var connectionString = IntegrationMongoConnectionStringBuilder.BuildSharded(maxPoolSize: 11, configuration);
            var settings = MongoClientSettings.FromConnectionString(connectionString);

            Assert.Equal("root", settings.Login);
            Assert.Equal("password", settings.Password);
            Assert.Equal("admin", settings.AdminDB);
            Assert.Equal("SCRAM-SHA-256", settings.AuthMechanism);
            Assert.Equal(11, settings.ConnectionPoolMaxSize);

            var endpoint = Assert.IsType<DnsEndPoint>(Assert.Single(settings.Endpoints));
            Assert.Equal("localhost", endpoint.Host);
            Assert.Equal(27029, endpoint.Port);
        }

        [Fact]
        public void BuildStandalone_ExplicitConfiguration_PreservesCredentialWhitespaceAndEncoding()
        {
            const string username = " user name ";
            const string password = "  p@ss :word?  ";
            var configuration = new IntegrationMongoConnectionStringBuilder.IntegrationMongoConnectionStringConfiguration(
                StandaloneHost: "mongo-standalone.internal:28016",
                ReplicaSetHost: "mongo-rs.internal:28017",
                ShardedHost: "mongo-sharded.internal:28029",
                ReplicaSetName: "rs-custom",
                Username: username,
                Password: password,
                AuthSource: "admin",
                AuthMechanism: "SCRAM-SHA-256");

            var connectionString = IntegrationMongoConnectionStringBuilder.BuildStandalone(maxPoolSize: 5, configuration);
            var settings = MongoClientSettings.FromConnectionString(connectionString);

            Assert.Contains("mongodb://%20user%20name%20:%20%20p%40ss%20%3Aword%3F%20%20@", connectionString);
            Assert.Equal(username, settings.Login);
            Assert.Equal(password, settings.Password);
        }
    }
}
