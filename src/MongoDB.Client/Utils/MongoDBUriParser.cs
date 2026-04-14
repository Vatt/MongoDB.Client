using System.Collections.Immutable;
using System.Net;
using MongoDB.Client.Exceptions;

namespace MongoDB.Client.Utils
{
    internal class MongoUriParseResult
    {
        public string Scheme { get; }
        public string? Login { get; }
        public string? Password { get; }
        public string? AdminDb { get; }
        public IEnumerable<EndPoint> Hosts { get; }
        public Dictionary<string, string> Options { get; }

        internal MongoUriParseResult(
            string scheme,
            string? login,
            string? password,
            IEnumerable<EndPoint> hosts,
            string? adminDb,
            Dictionary<string, string>? options = null)
        {
            Scheme = scheme;
            Login = login;
            Password = password;
            Hosts = hosts;
            AdminDb = adminDb;
            Options = options ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    internal static class MongoDBUriParser
    {
        private const string MongoDbScheme = "mongodb://";
        private const string MongoDbSrvScheme = "mongodb+srv://";

        internal static MongoUriParseResult ParseUri(string uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            var scheme = ParseScheme(uri, out var remainder);
            var slashIndex = remainder.IndexOf('/');
            var authority = slashIndex >= 0 ? remainder[..slashIndex] : remainder;
            var pathAndQuery = slashIndex >= 0 ? remainder[(slashIndex + 1)..] : null;

            var (login, password, hostsString) = ParseAuthority(authority);
            var hosts = ParseHosts(hostsString);
            var (adminDb, options) = ParsePathAndQuery(pathAndQuery);

            return new MongoUriParseResult(scheme, login, password, hosts, adminDb, options);
        }

        private static string ParseScheme(string uri, out string remainder)
        {
            if (uri.StartsWith(MongoDbScheme, StringComparison.OrdinalIgnoreCase))
            {
                remainder = uri[MongoDbScheme.Length..];
                return MongoDbScheme;
            }

            if (uri.StartsWith(MongoDbSrvScheme, StringComparison.OrdinalIgnoreCase))
            {
                remainder = uri[MongoDbSrvScheme.Length..];
                return MongoDbSrvScheme;
            }

            throw new MongoDBUriParserException("Unsupported MongoDB connection string scheme.");
        }

        private static (string? Login, string? Password, string Hosts) ParseAuthority(string authority)
        {
            var atIndex = authority.LastIndexOf('@');
            if (atIndex < 0)
            {
                return (null, null, authority);
            }

            var userInfo = authority[..atIndex];
            var hosts = authority[(atIndex + 1)..];
            var colonIndex = userInfo.IndexOf(':');

            if (colonIndex < 0)
            {
                return (Decode(userInfo), null, hosts);
            }

            var login = userInfo[..colonIndex];
            var password = userInfo[(colonIndex + 1)..];
            return (Decode(login), Decode(password), hosts);
        }

        private static IEnumerable<EndPoint> ParseHosts(string hostsString)
        {
            if (string.IsNullOrWhiteSpace(hostsString))
            {
                throw new MongoDBUriParserException("MongoDB connection string must include at least one host.");
            }

            var hosts = ImmutableList.CreateBuilder<EndPoint>();
            foreach (var hostEntry in hostsString.Split(',', StringSplitOptions.TrimEntries))
            {
                hosts.Add(ParseHost(hostEntry));
            }

            if (hosts.Count == 0)
            {
                throw new MongoDBUriParserException("MongoDB connection string must include at least one host.");
            }

            return hosts.ToImmutable();
        }

        private static EndPoint ParseHost(string hostEntry)
        {
            if (string.IsNullOrWhiteSpace(hostEntry))
            {
                throw new MongoDBUriParserException("MongoDB host must not be empty.");
            }

            if (hostEntry[0] == ':')
            {
                throw new MongoDBUriParserException("MongoDB host must not be empty.");
            }

            if (hostEntry[0] == '[')
            {
                return ParseBracketedHost(hostEntry);
            }

            if (hostEntry.Count(c => c == ':') > 1)
            {
                throw new MongoDBUriParserException($"MongoDB host entry '{hostEntry}' is invalid.");
            }

            var portSeparatorIndex = hostEntry.LastIndexOf(':');
            if (portSeparatorIndex > 0 && hostEntry.IndexOf(':') == portSeparatorIndex)
            {
                var host = hostEntry[..portSeparatorIndex];
                var portPart = hostEntry[(portSeparatorIndex + 1)..];

                if (string.IsNullOrWhiteSpace(host))
                {
                    throw new MongoDBUriParserException("MongoDB host must not be empty.");
                }

                if (string.IsNullOrWhiteSpace(portPart) || !int.TryParse(portPart, out var port) || port is < 1 or > 65535)
                {
                    throw new MongoDBUriParserException($"MongoDB port '{portPart}' is invalid for host '{host}'.");
                }

                return new DnsEndPoint(host, port);
            }

            return new DnsEndPoint(hostEntry, 27017);
        }

        private static EndPoint ParseBracketedHost(string hostEntry)
        {
            var closingBracketIndex = hostEntry.IndexOf(']');
            if (closingBracketIndex < 0)
            {
                throw new MongoDBUriParserException($"MongoDB host entry '{hostEntry}' is invalid.");
            }

            var host = hostEntry[1..closingBracketIndex];
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new MongoDBUriParserException("MongoDB host must not be empty.");
            }

            var remainder = hostEntry[(closingBracketIndex + 1)..];
            if (string.IsNullOrEmpty(remainder))
            {
                return new DnsEndPoint(host, 27017);
            }

            if (remainder[0] != ':')
            {
                throw new MongoDBUriParserException($"MongoDB host entry '{hostEntry}' is invalid.");
            }

            var portPart = remainder[1..];
            if (string.IsNullOrWhiteSpace(portPart) || !int.TryParse(portPart, out var port) || port is < 1 or > 65535)
            {
                throw new MongoDBUriParserException($"MongoDB port '{portPart}' is invalid for host '{host}'.");
            }

            return new DnsEndPoint(host, port);
        }

        private static (string? AdminDb, Dictionary<string, string> Options) ParsePathAndQuery(string? pathAndQuery)
        {
            var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (pathAndQuery is null)
            {
                return (null, options);
            }

            string? adminDb = null;
            var queryIndex = pathAndQuery.IndexOf('?');
            var databasePart = queryIndex >= 0 ? pathAndQuery[..queryIndex] : pathAndQuery;
            var optionsPart = queryIndex >= 0 ? pathAndQuery[(queryIndex + 1)..] : null;

            if (databasePart.EndsWith('/'))
            {
                databasePart = databasePart[..^1];
            }

            if (!string.IsNullOrEmpty(databasePart))
            {
                adminDb = Decode(databasePart);
            }

            if (!string.IsNullOrEmpty(optionsPart))
            {
                if (optionsPart.EndsWith('/'))
                {
                    optionsPart = optionsPart[..^1];
                }

                foreach (var opt in optionsPart.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    AddOption(options, opt);
                }
            }

            return (adminDb, options);
        }

        private static void AddOption(Dictionary<string, string> options, string option)
        {
            var separatorIndex = option.IndexOf('=');
            var key = separatorIndex >= 0 ? option[..separatorIndex] : option;
            var value = separatorIndex >= 0 ? option[(separatorIndex + 1)..] : string.Empty;

            key = Decode(key);
            value = Decode(value);

            if (key.Equals("authSource", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(value))
            {
                throw new MongoDBUriParserException("authSource must not be empty.");
            }

            if (key.Equals("readPreferenceTags", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                if (options.TryGetValue("readPreferenceTags", out var tags))
                {
                    options["readPreferenceTags"] = tags + "&" + value;
                }
                else
                {
                    options["readPreferenceTags"] = value;
                }

                return;
            }

            options.Add(key, value);
        }

        private static string Decode(string value)
        {
            return Uri.UnescapeDataString(value);
        }
    }

}
