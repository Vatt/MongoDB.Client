using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using MongoDB.Client.Network;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Tests.Infrastructure
{
    internal static class IntegrationMongoTopologyRequirements
    {
        internal static readonly TimeSpan DefaultSocketProbeTimeout = TimeSpan.FromSeconds(1);
        internal static readonly TimeSpan DefaultAuthenticatedProbeTimeout = TimeSpan.FromSeconds(5);

        public static string? GetStandaloneSkipReason(CancellationToken cancellationToken = default)
        {
            return GetSkipReasonForTopology(
                static () => IntegrationMongoConnectionStringBuilder.BuildStandalone(maxPoolSize: 1),
                topologyName: "standalone MongoDB",
                IsStandaloneTopology,
                cancellationToken);
        }

        public static string? GetReplicaSetSkipReason(CancellationToken cancellationToken = default)
        {
            return GetSkipReasonForTopology(
                static () => IntegrationMongoConnectionStringBuilder.BuildReplicaSet(maxPoolSize: 1),
                topologyName: "replica set MongoDB",
                IsReplicaSetTopology,
                cancellationToken);
        }

        internal static async Task<string?> GetUnavailableReasonAsync(
            EndPoint endpoint,
            string topologyName,
            Func<EndPoint, CancellationToken, Task<MongoPingMessage>> probeAsync,
            Func<MongoPingMessage, bool> topologyRequirement,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var ping = await probeAsync(endpoint, cancellationToken).ConfigureAwait(false);
                if (topologyRequirement(ping))
                {
                    return null;
                }

                return $"Required {topologyName} topology is unavailable at '{endpoint}': server responded as {DescribeTopology(ping)}.";
            }
            catch (Exception ex) when (IsUnavailable(ex, cancellationToken))
            {
                return $"Required {topologyName} topology is unavailable at '{endpoint}': {ex.Message}";
            }
        }

        internal static async Task<MongoPingMessage> ProbeMongoTopologyAsync(MongoClientSettings settings, EndPoint endpoint, CancellationToken cancellationToken)
        {
            return await ProbeMongoTopologyAsync(
                settings,
                endpoint,
                PerformSocketProbeAsync,
                PerformAuthenticatedProbeAsync,
                cancellationToken).ConfigureAwait(false);
        }

        internal static async Task<MongoPingMessage> ProbeMongoTopologyAsync(
            MongoClientSettings settings,
            EndPoint endpoint,
            Func<EndPoint, CancellationToken, Task> socketProbeAsync,
            Func<MongoClientSettings, EndPoint, CancellationToken, Task<MongoPingMessage>> authenticatedProbeAsync,
            CancellationToken cancellationToken)
        {
            await ExecuteWithTimeoutAsync(
                endpoint,
                DefaultSocketProbeTimeout,
                "connecting to",
                async token =>
                {
                    await socketProbeAsync(endpoint, token).ConfigureAwait(false);
                    return true;
                },
                cancellationToken).ConfigureAwait(false);

            return await ExecuteWithTimeoutAsync(
                endpoint,
                DefaultAuthenticatedProbeTimeout,
                "running authenticated probe against",
                token => authenticatedProbeAsync(settings, endpoint, token),
                cancellationToken).ConfigureAwait(false);
        }

        internal static async Task PerformSocketProbeAsync(EndPoint endpoint, CancellationToken cancellationToken)
        {
            using var tcpClient = new TcpClient();

            switch (endpoint)
            {
                case DnsEndPoint dnsEndPoint:
                    await tcpClient.ConnectAsync(dnsEndPoint.Host, dnsEndPoint.Port, cancellationToken).ConfigureAwait(false);
                    break;
                case IPEndPoint ipEndPoint:
                    await tcpClient.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported endpoint type '{endpoint.GetType().Name}'.");
            }
        }

        private static async Task<MongoPingMessage> PerformAuthenticatedProbeAsync(
            MongoClientSettings settings,
            EndPoint endpoint,
            CancellationToken cancellationToken)
        {
            var connectionFactory = new NetworkConnectionFactory(NullLoggerFactory.Instance);
            var connectionInitializer = MongoConnectionInitializerFactory.Create(settings);
            var connectionContext = await connectionFactory.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);

            await using var serviceConnection = new MongoServiceConnection(connectionContext);

            await serviceConnection.Connect(connectionInitializer, cancellationToken).ConfigureAwait(false);
            return await serviceConnection.MongoPing(cancellationToken).ConfigureAwait(false);
        }

        internal static bool IsStandaloneTopology(MongoPingMessage ping)
        {
            return ping.Hosts is null &&
                   ping.SetName is null &&
                   ping.Message is null &&
                   ping.Primary is null &&
                   ping.IsMaster &&
                   !ping.IsSecondary;
        }

        internal static bool IsReplicaSetTopology(MongoPingMessage ping)
        {
            return ping.Hosts is not null &&
                   ping.SetName is not null &&
                   ping.Message is null &&
                   (ping.IsMaster || ping.IsSecondary || ping.Primary is not null);
        }

        private static bool IsShardedTopology(MongoPingMessage ping)
        {
            return ping.Hosts is null &&
                   ping.SetName is null &&
                   ping.Primary is null &&
                   string.Equals(ping.Message, "isdbgrid", StringComparison.Ordinal);
        }

        private static string DescribeTopology(MongoPingMessage ping)
        {
            if (IsReplicaSetTopology(ping))
            {
                return "replica set topology";
            }

            if (IsShardedTopology(ping))
            {
                return "sharded topology";
            }

            if (IsStandaloneTopology(ping))
            {
                return "standalone topology";
            }

            return "unknown topology shape";
        }

        private static bool IsUnavailable(Exception exception, CancellationToken cancellationToken)
        {
            if (exception is TimeoutException or SocketException)
            {
                return true;
            }

            return exception is OperationCanceledException && !cancellationToken.IsCancellationRequested;
        }

        internal static async Task<string?> GetSkipReasonForTopologyAsync(
            IReadOnlyList<EndPoint> endpoints,
            string topologyName,
            Func<EndPoint, CancellationToken, Task<MongoPingMessage>> probeAsync,
            Func<MongoPingMessage, bool> topologyRequirement,
            CancellationToken cancellationToken = default)
        {
            string? lastSkipReason = null;

            foreach (var endpoint in endpoints)
            {
                var skipReason = await GetUnavailableReasonAsync(
                    endpoint,
                    topologyName,
                    probeAsync,
                    topologyRequirement,
                    cancellationToken).ConfigureAwait(false);

                if (skipReason is null)
                {
                    return null;
                }

                lastSkipReason = skipReason;
            }

            return lastSkipReason ?? $"Required {topologyName} topology is unavailable: no endpoints configured.";
        }

        private static string? GetSkipReasonForTopology(
            Func<string> getConnectionString,
            string topologyName,
            Func<MongoPingMessage, bool> topologyRequirement,
            CancellationToken cancellationToken)
        {
            var settings = MongoClientSettings.FromConnectionString(getConnectionString());

            return GetSkipReasonForTopologyAsync(
                settings.Endpoints,
                topologyName,
                (probeEndpoint, token) => ProbeMongoTopologyAsync(settings, probeEndpoint, token),
                topologyRequirement,
                cancellationToken).GetAwaiter().GetResult();
        }

        private static async Task<T> ExecuteWithTimeoutAsync<T>(
            EndPoint endpoint,
            TimeSpan timeout,
            string operationDescription,
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            try
            {
                return await operation(timeoutCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException($"Timed out after {timeout.TotalMilliseconds:0} ms while {operationDescription} '{endpoint}'.");
            }
        }
    }
}
