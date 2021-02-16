using MongoDB.Client.Exceptions;
using System;
using System.Collections.Generic;

namespace MongoDB.Client.Utils
{

    internal partial class MongoDBUriParser
    {
        internal class MongoDbUriParseResult
        {
            public string Scheme { get; }
            public string? Login { get; }
            public string? Password { get; }
            public List<HostInfo> Hosts { get; }
            public Dictionary<string, string> Options { get; }

            internal MongoDbUriParseResult(
                string scheme,
                string login,
                string password,
                List<HostInfo> hosts,
                Dictionary<string, string> options)
            {
                Scheme = scheme;
                Login = login;
                Password = password;
                Hosts = hosts;
                Options = options;
            }
        }
        internal class HostInfo
        {
            public string Host { get; }
            public int? Port { get; }

            public HostInfo(string host, int port)
            {
                Host = host;
                Port = port;
            }

            public HostInfo(string host)
            {
                Host = host;
                Port = null;
            }
        }
        private string _original;
        private string _unreaded;
        private readonly bool _haveUserInfo;
        private readonly bool _haveOptions;

        private string _scheme;
        private string _user;
        private string _password;
        private List<HostInfo> _hosts;
        private Dictionary<string, string> _options;
        private MongoDBUriParser(string uriString)
        {
            _hosts = new();
            _options = new();
            _original = uriString;
            _haveUserInfo = uriString.Contains('@');
            _haveOptions = uriString.Contains("/?");
            _unreaded = uriString;
        }
        private MongoDbUriParseResult ParsePrivate()
        {
            try
            {
                ParseScheme();
                if (_haveUserInfo)
                {
                    ParseUserInfo();
                }
                ParseHosts();
                if (_haveOptions)
                {
                    Skip(2);
                    ParseOptions();
                }
            }
            catch (Exception ex)
            {
                throw new MongoDBUriParserException($" {nameof(MongoDbUriParseResult)}: unhandled exception", ex);
            }

            if (_unreaded.Length != 0)
            {
                throw new MongoDBUriParserException($"Bad connection string {_original}");
            }

            return new MongoDbUriParseResult(_scheme, _user, _password, _hosts, _options);
        }
        private void ParseOptions()
        {
            while (TryReadTo('&', out var expr))
            {
                ParseOption(expr);
                Skip(1);
            }
            if (TryReadTo('/', out var last))
            {
                ParseOption(last);
            }
            else
            {
                ParseOption(Read(_unreaded.Length));
            }
            void ParseOption(string expr)
            {
                var splited = expr.Split('=', StringSplitOptions.TrimEntries);
                _options.Add(splited[0], splited[1]);
            }
        }
        private void ParseHosts()
        {
            if (_unreaded.Contains(','))
            {
                while (TryReadTo(',', out var host))
                {
                    _hosts.Add(ParseHost(host));
                    Skip(1);
                }
                if (TryReadTo('/', out var last))
                {
                    _hosts.Add(ParseHost(last));
                }
                else
                {
                    _hosts.Add(ParseHost(Read(_unreaded.Length)));
                }
            }
            else
            {
                if (TryReadTo('/', out var host))
                {
                    _hosts.Add(ParseHost(host));
                }
                else
                {
                    _hosts.Add(ParseHost(Read(_unreaded.Length)));
                }
            }

            HostInfo ParseHost(string hostStr)
            {
                if (hostStr.Length == 0)
                {
                    throw new MongoDBUriParserException($"Bad connection string {nameof(ParseHost)} : {nameof(hostStr)}.Lenght == 0");
                }
                if (hostStr.Contains(':'))
                {
                    var splited = hostStr.Split(':', StringSplitOptions.TrimEntries);
                    return new HostInfo(splited[0].Trim(), int.Parse(splited[1].Trim()));
                }

                return new HostInfo(hostStr.Trim());
            }
        }
        private void ParseUserInfo()
        {
            _user = ReadTo(':');
            Skip(1);
            _password = ReadTo('@');
            Skip(1);
        }
        private void ParseScheme()
        {
            _scheme = Read(7);
            var next = Read(3);
            if (!next.Equals("://"))
            {
                throw new MongoDBUriParserException($"Bad connection string ({nameof(ParseScheme)})");
            }

            if (!_scheme.Equals("mongodb"))
            {
                throw new MongoDBUriParserException($"Bad connection string ({nameof(ParseScheme)}), scheme error");
            }
        }
        private string Read(int count)
        {
            var result = _unreaded.Substring(0, count);
            _unreaded = _unreaded.Remove(0, count);
            return result;
        }
        private void Skip(int count)
        {
            if (_unreaded.Length < count)
            {
                throw new MongoDBUriParserException($"Bad connection string ({nameof(Skip)}) : {nameof(_unreaded)}.Lenght < {nameof(count)}");
            }
            _unreaded = _unreaded.Remove(0, count);
        }
        private string ReadTo(char ch)
        {
            var index = _unreaded.IndexOf(ch);
            if (index == -1)
            {
                throw new MongoDBUriParserException($"Bad connection string ({nameof(ReadTo)}) : char not found");
            }
            var result = _unreaded.Substring(0, index);
            _unreaded = _unreaded.Remove(0, index);
            return result;
        }
        private bool TryReadTo(char ch, out string result)
        {
            result = default;
            var index = _unreaded.IndexOf(ch);
            if (index == -1)
            {
                return false;
            }
            result = ReadTo(ch);
            return true;
        }
        public static MongoDbUriParseResult Parse1(string uriString)
        {
            var parser = new MongoDBUriParser(uriString);
            return parser.ParsePrivate();
        }
    }
}
