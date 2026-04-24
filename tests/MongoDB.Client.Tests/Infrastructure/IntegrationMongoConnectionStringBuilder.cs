using System.Text;

namespace MongoDB.Client.Tests.Infrastructure
{
    internal static class IntegrationMongoConnectionStringBuilder
    {
        private const string DefaultStandaloneHost = "localhost:27016";
        private const string DefaultReplicaSetHost = "localhost:27017";
        private const string DefaultShardedHost = "localhost:27029";
        private const string DefaultUsername = "root";
        private const string DefaultPassword = "password";
        private const string DefaultAuthSource = "admin";
        private const string DefaultAuthMechanism = "SCRAM-SHA-256";
        private const string DefaultReplicaSetName = "rs0";

        public static string StandaloneHost => IntegrationMongoConnectionStringConfiguration.FromEnvironment().StandaloneHost;
        public static string ReplicaSetHost => IntegrationMongoConnectionStringConfiguration.FromEnvironment().ReplicaSetHost;
        public static string ShardedHost => IntegrationMongoConnectionStringConfiguration.FromEnvironment().ShardedHost;
        public static string ReplicaSetName => IntegrationMongoConnectionStringConfiguration.FromEnvironment().ReplicaSetName;

        public static string BuildStandalone(int maxPoolSize)
        {
            return BuildStandalone(maxPoolSize, IntegrationMongoConnectionStringConfiguration.FromEnvironment());
        }

        public static string BuildStandalone(int maxPoolSize, IntegrationMongoConnectionStringConfiguration configuration)
        {
            return BuildConnectionString(configuration.StandaloneHost, maxPoolSize, configuration);
        }

        public static string BuildReplicaSet(int maxPoolSize, string? replicaSetName = null)
        {
            return BuildReplicaSet(maxPoolSize, IntegrationMongoConnectionStringConfiguration.FromEnvironment(), replicaSetName);
        }

        public static string BuildReplicaSet(int maxPoolSize, IntegrationMongoConnectionStringConfiguration configuration, string? replicaSetName = null)
        {
            var effectiveReplicaSetName = string.IsNullOrWhiteSpace(replicaSetName) ? configuration.ReplicaSetName : replicaSetName;
            return BuildConnectionString(configuration.ReplicaSetHost, maxPoolSize, configuration, ("replicaSet", effectiveReplicaSetName));
        }

        public static string BuildSharded(int maxPoolSize)
        {
            return BuildSharded(maxPoolSize, IntegrationMongoConnectionStringConfiguration.FromEnvironment());
        }

        public static string BuildSharded(int maxPoolSize, IntegrationMongoConnectionStringConfiguration configuration)
        {
            return BuildConnectionString(configuration.ShardedHost, maxPoolSize, configuration);
        }

        private static string BuildConnectionString(string host, int maxPoolSize, IntegrationMongoConnectionStringConfiguration configuration, params (string Key, string? Value)[] extraOptions)
        {
            var builder = new StringBuilder("mongodb://");
            var username = configuration.Username;
            var password = configuration.Password;

            if (!string.IsNullOrWhiteSpace(username))
            {
                builder.Append(Uri.EscapeDataString(username));

                if (!string.IsNullOrEmpty(password))
                {
                    builder.Append(':');
                    builder.Append(Uri.EscapeDataString(password));
                }

                builder.Append('@');
            }

            builder.Append(host);
            builder.Append("/?");

            var options = new List<(string Key, string Value)>
            {
                ("maxPoolSize", maxPoolSize.ToString())
            };

            if (!string.IsNullOrWhiteSpace(username))
            {
                options.Add(("authSource", configuration.AuthSource));
                options.Add(("authMechanism", configuration.AuthMechanism));
            }

            foreach (var (key, value) in extraOptions)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    options.Add((key, value));
                }
            }

            builder.Append(string.Join("&", options.Select(static x => $"{x.Key}={Uri.EscapeDataString(x.Value)}")));
            return builder.ToString();
        }

        private static string GetValue(string variableName, string defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(variableName);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private static string? GetOptionalValue(string variableName, string defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(variableName);
            return value is null ? defaultValue : value;
        }

        internal sealed record IntegrationMongoConnectionStringConfiguration(
            string StandaloneHost,
            string ReplicaSetHost,
            string ShardedHost,
            string ReplicaSetName,
            string? Username,
            string? Password,
            string AuthSource,
            string AuthMechanism)
        {
            public static IntegrationMongoConnectionStringConfiguration CreateDefault() =>
                new(
                    DefaultStandaloneHost,
                    DefaultReplicaSetHost,
                    DefaultShardedHost,
                    DefaultReplicaSetName,
                    DefaultUsername,
                    DefaultPassword,
                    DefaultAuthSource,
                    DefaultAuthMechanism);

            public static IntegrationMongoConnectionStringConfiguration FromEnvironment() =>
                new(
                    GetValue("MONGODB_HOST", DefaultStandaloneHost),
                    GetValue("MONGODB_RS_HOST", DefaultReplicaSetHost),
                    GetValue("MONGODB_SHARDED_HOST", DefaultShardedHost),
                    GetValue("MONGODB_RS_NAME", GetValue("MONGODB_REPLICA_SET", DefaultReplicaSetName)),
                    GetOptionalValue("MONGODB_USERNAME", DefaultUsername),
                    GetOptionalValue("MONGODB_PASSWORD", DefaultPassword),
                    GetValue("MONGODB_AUTH_SOURCE", DefaultAuthSource),
                    GetValue("MONGODB_AUTH_MECHANISM", DefaultAuthMechanism));
        }
    }
}
