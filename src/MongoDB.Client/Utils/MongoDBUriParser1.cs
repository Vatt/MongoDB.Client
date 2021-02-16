using Sprache;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;

namespace MongoDB.Client.Utils
{
    internal partial class MongoDBUriParser
    {
        internal class MongoUriParseResult
        {
            public string Scheme { get; }
            public string? Login { get; }
            public string? Password { get; }
            public IEnumerable<EndPoint> Hosts { get; }
            public Dictionary<string, string> Options { get; }

            internal MongoUriParseResult(
                string scheme,
                string login,
                string password,
                IEnumerable<EndPoint> hosts,
                Dictionary<string, string> options)
            {
                Scheme = scheme;
                Login = login;
                Password = password;
                Hosts = hosts;
                Options = options;
            }
        }
        public static void ParseUri(string uriStr)
        {
            Parser<(string, string)> userInfoParser =
                from user in Parse.LetterOrDigit.Many().Text()
                from separator in Parse.Char(':').Once()
                from password in Parse.LetterOrDigit.Many().Text()
                from end in Parse.Char('@').Once()
                select (user, password);
            
            Parser<int> portParser =
                from colon in Parse.Char(':').Once()
                from port in Parse.Number
                select int.Parse(port);
            Parser<EndPoint> hostParser =
                from host in Parse.Or(Parse.LetterOrDigit, Parse.Chars('-', '_', '.')).Many().Text()
                from port in portParser.Optional()
                select new DnsEndPoint(host, port.IsEmpty ? 27017 : port.Get());
            Parser<EndPoint> tailHostParser =
                from headspaces in Parse.WhiteSpace.Many().Optional()
                from colon in Parse.Char(',')
                from tailspaces in Parse.WhiteSpace.Many().Optional()
                from host in hostParser
                select host;
            Parser<IEnumerable<EndPoint>> hostsParser =
                from first in hostParser
                from tail in tailHostParser.Many().Optional()
                select ImmutableList<EndPoint>.Empty.Add(first).AddRange(tail.Get()); 

            Parser<KeyValuePair<string, string>> optionParser =
                from key in Parse.Letter.Many().Text()
                from eq in Parse.Char('=')
                from value in Parse.Or(Parse.LetterOrDigit, Parse.Chars('.', '-', '_', ':', ',')).Many().Text()
                select KeyValuePair.Create(key, value);


            Parser < MongoUriParseResult > uriParser =
                from scheme in Parse.String("mongodb://").Text()
                from userInfo in userInfoParser.Optional()
                from hosts in hostsParser
                from maybeSlash in Parse.Char('/').Optional()
                select new MongoUriParseResult(scheme,
                userInfo.IsEmpty ? null : userInfo.Get().Item1,
                userInfo.IsEmpty ? null : userInfo.Get().Item2,
                hosts.ToList(), null);

            var id = uriParser.Parse(uriStr);
        }
    }
}
