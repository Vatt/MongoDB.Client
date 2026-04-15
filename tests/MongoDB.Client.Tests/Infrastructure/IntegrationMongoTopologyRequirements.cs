using System.Net;
using System.Net.Sockets;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Tests.Infrastructure
{
    internal static class IntegrationMongoTopologyRequirements
    {
        internal static readonly TimeSpan DefaultProbeTimeout = TimeSpan.FromSeconds(1);

        public static string? GetStandaloneSkipReason(CancellationToken cancellationToken = default)
        {
            var connectionString = IntegrationMongoConnectionStringBuilder.BuildStandalone(maxPoolSize: 1);
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            var endpoint = settings.Endpoints.Single();

            return GetUnavailableReasonAsync(
                endpoint,
                topologyName: "standalone MongoDB",
                TcpConnectAsync,
                cancellationToken).GetAwaiter().GetResult();
        }

        internal static async Task<string?> GetUnavailableReasonAsync(
            EndPoint endpoint,
            string topologyName,
            Func<EndPoint, CancellationToken, Task> connectAsync,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await connectAsync(endpoint, cancellationToken).ConfigureAwait(false);
                return null;
            }
            catch (Exception ex) when (IsUnavailable(ex, cancellationToken))
            {
                return $"Required {topologyName} topology is unavailable at '{endpoint}': {ex.Message}";
            }
        }

        internal static async Task TcpConnectAsync(EndPoint endpoint, CancellationToken cancellationToken)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(DefaultProbeTimeout);

            try
            {
                using var tcpClient = new TcpClient();

                switch (endpoint)
                {
                    case DnsEndPoint dnsEndPoint:
                        await tcpClient.ConnectAsync(dnsEndPoint.Host, dnsEndPoint.Port, timeoutCts.Token).ConfigureAwait(false);
                        break;
                    case IPEndPoint ipEndPoint:
                        await tcpClient.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port, timeoutCts.Token).ConfigureAwait(false);
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported endpoint type '{endpoint.GetType().Name}'.");
                }
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException($"Timed out after {DefaultProbeTimeout.TotalMilliseconds:0} ms while connecting to '{endpoint}'.");
            }
        }

        private static bool IsUnavailable(Exception exception, CancellationToken cancellationToken)
        {
            if (exception is TimeoutException or SocketException)
            {
                return true;
            }

            return exception is OperationCanceledException && !cancellationToken.IsCancellationRequested;
        }
    }
}
