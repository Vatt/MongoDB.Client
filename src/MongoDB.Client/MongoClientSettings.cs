using MongoDB.Client.Utils;
using System;
using System.Collections.Immutable;
using System.Net;

namespace MongoDB.Client
{
    public class MongoClientSettings
    {
        public MongoClientSettings(EndPoint[] endpoints, string? login, string? password)
        {
            // if (endpoints is {Length: 0})
            // {
            //     throw new ArgumentException("Endpoints must not be empty");
            // }
            Login = login;
            Password = password;
            Endpoints = ImmutableArray.Create(endpoints);
        }

        public MongoClientSettings(EndPoint[] endpoints)
        : this(endpoints, string.Empty, string.Empty)
        {
        }

        public MongoClientSettings(EndPoint endpoint)
            : this(new[] { endpoint }, string.Empty, string.Empty)
        {
        }

        public MongoClientSettings()
            : this(new[] { new IPEndPoint(IPAddress.Loopback, 27017) }, string.Empty, string.Empty)
        {
        }

        private readonly ImmutableArray<EndPoint> _endpoints;
        public ImmutableArray<EndPoint> Endpoints
        {
            get => _endpoints;
            init
            {
                if (value is { Length: 0 })
                {
                    throw new ArgumentException("Endpoints must not be empty");
                }

                _endpoints = value;
            }
        }
        public string? ApplicationName { get; private set; }
        public string? Login { get; private set; }
        public string? Password { get; private set; }
        public string? ReplicaSet { get; private set; }
        public int? ConnectTimeoutMs { get; private set; }
        public int? SocketTimeoutMs { get; private set; }
        public string? ReadPreference { get; private set; }
        public bool TlsOrSslEnable { get; private set; }
        public int ConnectionPoolMaxSize { get; private set; } = 16;
        //public int ConnectionPoolMinSize { get; private set; } = 0;
        //public int MultiplexingTreshold { get; init; } = 2;

        public static MongoClientSettings FromConnectionString(string uriString)
        {
            var result = MongoDBUriParser.Parse(uriString);
            var endpoints = new DnsEndPoint[result.Hosts.Count];
            for (int i = 0; i < result.Hosts.Count; i++)
            {
                var host = result.Hosts[i];
                endpoints[i] = new DnsEndPoint(host.Host, host.Port.HasValue ? host.Port.Value : 27017);
            }
            var settings = new MongoClientSettings(endpoints, result.Login, result.Password);
            if (result.Login != default)
            {
                settings.Login = result.Login;
            }

            if (result.Password != default)
            {
                settings.Password = result.Password;
            }
            if (result.Options.TryGetValue("replicaSet", out var replSet))
            {
                settings.ReplicaSet = replSet;
            }

            ProcessTimeoutSettings(settings, result);
            ProcessConnectionPoolSettings(settings, result);

            if (result.Options.TryGetValue("readPreference", out var readPreference))
            {
                settings.ReadPreference = readPreference;
            }
            if (result.Options.TryGetValue("tls", out var tls))
            {
                if (tls.Equals("true"))
                {
                    settings.TlsOrSslEnable = true;
                }
                else
                {
                    settings.TlsOrSslEnable = false;
                }
            }
            if (result.Options.TryGetValue("ssl", out var ssl))
            {
                if (ssl.Equals("true"))
                {
                    settings.TlsOrSslEnable = true;
                }
                else
                {
                    settings.TlsOrSslEnable = false;
                }
            }
            if (result.Options.TryGetValue("appName", out var appName))
            {
                settings.ApplicationName = appName;
            }
            return settings;
        }

        private static void ProcessTimeoutSettings(MongoClientSettings settings, MongoDBUriParser.MongoDbUriParseResult result)
        {
            if (result.Options.TryGetValue("connectTimeoutMS", out var connectionTimeout))
            {
                settings.ConnectTimeoutMs = int.Parse(connectionTimeout);
            }
            if (result.Options.TryGetValue("socketTimeoutMS", out var socketTimeoutMs))
            {
                settings.SocketTimeoutMs = int.Parse(socketTimeoutMs);
            }
        }
        private static void ProcessConnectionPoolSettings(MongoClientSettings settings, MongoDBUriParser.MongoDbUriParseResult result)
        {
            if (result.Options.TryGetValue("maxPoolSize", out var maxPoolSize))
            {
                settings.ConnectionPoolMaxSize = int.Parse(maxPoolSize);
            }
            if (result.Options.TryGetValue("minPoolSize", out var minPoolSize))
            {
                //settings.ConnectionPoolMinSize = int.Parse(minPoolSize);
                throw new NotSupportedException("minPoolSize");
            }
            if (result.Options.TryGetValue("maxIdleTimeMS", out var maxIdleTimeMs))
            {
                throw new NotSupportedException("maxIdleTimeMS");
            }
            if (result.Options.TryGetValue("waitQueueMultiple", out var waitQueueMultiple))
            {
                throw new NotSupportedException("waitQueueMultiple");
            }
            if (result.Options.TryGetValue("waitQueueTimeoutMS", out var waitQueueTimeoutMs))
            {
                throw new NotSupportedException("waitQueueTimeoutMS");
            }
        }
    }
}