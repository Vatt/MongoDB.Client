using System.Collections.Immutable;
using System.Net;
using MongoDB.Client.Utils;

namespace MongoDB.Client.Settings
{
    public record MongoClientSettings
    {
        public MongoClientSettings(IEnumerable<EndPoint> endpoints, string? login, string? password)
        {
            Login = login;
            Password = password;
            Endpoints = endpoints.ToImmutableArray();
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

        public string? ApplicationName { get; init; }
        public string? Login { get; init; }
        public string? Password { get; init; }
        public string? ReplicaSet { get; init; }
        public ReadPreference ReadPreference { get; init; }
        public ClientType ClientType { get; init; }
        public int ConnectionPoolMaxSize { get; init; } = 8;

        public static MongoClientSettings FromConnectionString(string uriString)
        {
            var result = MongoDBUriParser.ParseUri(uriString);

            result.Options.TryGetValue("replicaSet", out var replSet);


            int connectionPoolMaxSize = 16;
            if (result.Options.TryGetValue("maxPoolSize", out var maxPoolSize))
            {
                connectionPoolMaxSize = int.Parse(maxPoolSize);
            }
            var readPreference = ReadPreference.Primary;
            if (result.Options.TryGetValue("readPreference", out var readPreferenceStr))
            {
                readPreference = Enum.Parse<ReadPreference>(readPreferenceStr, true);
            }

            var clientType = ClientType.Default;
            if (result.Options.TryGetValue("clientType", out var clientTypeStr))
            {
                clientType = Enum.Parse<ClientType>(clientTypeStr, true);
            }

            string appName = "MongoDB.Client";
            if (result.Options.TryGetValue("appName", out var appNameVal))
            {
                appName = appNameVal;
            }

            var hosts = result.Hosts.ToArray();
            var settings = new MongoClientSettings
            {
                Endpoints = hosts.ToImmutableArray(),
                Login = result.Login,
                Password = result.Password,
                ReplicaSet = replSet,
                ConnectionPoolMaxSize = connectionPoolMaxSize,
                ReadPreference = readPreference,
                ClientType = clientType,
                ApplicationName = appName
            };

            return settings;
        }


        //public int? ConnectTimeoutMs { get; init; }
        //public int? SocketTimeoutMs { get; init; }
        //public bool TlsOrSslEnable { get; init; }
        //private static void UnusedForNow(MongoClientSettings settings, MongoUriParseResult result)
        //{
        //    bool tlsOrSslEnable = false;
        //    if (result.Options.TryGetValue("tls", out var tls))
        //    {
        //        if (tls.Equals("true", StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            tlsOrSslEnable = true;
        //        }
        //        else
        //        {
        //            tlsOrSslEnable = false;
        //        }
        //    }
        //    if (result.Options.TryGetValue("ssl", out var ssl))
        //    {
        //        if (ssl.Equals("true", StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            tlsOrSslEnable = true;
        //        }
        //        else
        //        {
        //            tlsOrSslEnable = false;
        //        }
        //    }
        //    int? connectTimeoutMs = null;
        //    if (result.Options.TryGetValue("connectTimeoutMS", out var connectionTimeout))
        //    {
        //        connectTimeoutMs = int.Parse(connectionTimeout);
        //    }
        //    int? socketTimeoutMs = null;
        //    if (result.Options.TryGetValue("socketTimeoutMS", out var socketTimeout))
        //    {
        //        socketTimeoutMs = int.Parse(socketTimeout);
        //    }
        //}

        //private static void ProcessTimeoutSettings(MongoClientSettings settings, MongoUriParseResult result)
        //{
        //    if (result.Options.TryGetValue("connectTimeoutMS", out var connectionTimeout))
        //    {
        //        settings.ConnectTimeoutMs = int.Parse(connectionTimeout);
        //    }
        //    if (result.Options.TryGetValue("socketTimeoutMS", out var socketTimeoutMs))
        //    {
        //        settings.SocketTimeoutMs = int.Parse(socketTimeoutMs);
        //    }
        //}
        //private static void ProcessConnectionPoolSettings(MongoClientSettings settings, MongoUriParseResult result)
        //{
        //    if (result.Options.TryGetValue("maxPoolSize", out var maxPoolSize))
        //    {
        //        settings.ConnectionPoolMaxSize = int.Parse(maxPoolSize);
        //    }
        //    if (result.Options.TryGetValue("minPoolSize", out var minPoolSize))
        //    {
        //        //settings.ConnectionPoolMinSize = int.Parse(minPoolSize);
        //        throw new NotSupportedException("minPoolSize");
        //    }
        //    if (result.Options.TryGetValue("maxIdleTimeMS", out var maxIdleTimeMs))
        //    {
        //        throw new NotSupportedException("maxIdleTimeMS");
        //    }
        //    if (result.Options.TryGetValue("waitQueueMultiple", out var waitQueueMultiple))
        //    {
        //        throw new NotSupportedException("waitQueueMultiple");
        //    }
        //    if (result.Options.TryGetValue("waitQueueTimeoutMS", out var waitQueueTimeoutMs))
        //    {
        //        throw new NotSupportedException("waitQueueTimeoutMS");
        //    }
        //}
    }
}
