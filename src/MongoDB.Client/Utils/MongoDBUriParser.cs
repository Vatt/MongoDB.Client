using Sprache;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;

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
            string? optionsString)
        {
            Scheme = scheme;
            Login = login;
            Password = password;
            Hosts = hosts;
            AdminDb = adminDb;
            Options = new Dictionary<string, string>();

            if (optionsString != null)
            {
                var optStr = optionsString[optionsString.Length - 1] == '/' ? optionsString.Remove(optionsString.Length - 1) : optionsString;
                foreach (var opt in optStr.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (IfReadPreferenceTags(opt))
                    {
                        continue;
                    }
                    var splited = opt.Split('=', StringSplitOptions.RemoveEmptyEntries);
                    if (splited.Length == 1)
                    {
                        Options.Add(splited[0], string.Empty);
                    }
                    else
                    {
                        Options.Add(splited[0], splited[1]);
                    }

                }
            }
        }
        private bool IfReadPreferenceTags(string opt)
        {
            var splited = opt.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (splited[0].Equals("readPreferenceTags"))
            {
                if (splited.Length == 1 || splited[1] is null || splited[1].Equals(string.Empty))
                {
                    return true;
                }

                if (Options.TryGetValue("readPreferenceTags", out var tags))
                {
                    tags = tags + "&" + splited[1];
                    Options["readPreferenceTags"] = tags;
                    return true;
                }
                else
                {
                    Options["readPreferenceTags"] = splited[1];
                    return true;
                }
            }
            return false;
        }
    }

    internal static class MongoDBUriParser
    {
        static private Parser<MongoUriParseResult> _uriParser;
        static MongoDBUriParser()
        {
            Parser<(string, string)> userInfoParser =
                from user in Parse.Or(Parse.LetterOrDigit, Parse.Chars(/*':',*/ '%', '/', '?', '#', '[', ']'/*, '@'*/)).Many().Text()
                from separator in Parse.Char(':').Once()
                from password in Parse.Or(Parse.LetterOrDigit, Parse.Chars(/*':',*/ '%', '/', '?', '#', '[', ']'/*, '@'*/)).Many().Text()
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

            Parser<string> adminDbParser =
                from slash in Parse.Char('/')
                from test in Parse.Letter
                from tail in Parse.Letter.Many().Text()
                select test + tail;

            Parser<string> optionsParser =
                from slash in Parse.String("/?")
                from options in Parse.AnyChar.Many().Text()
                select options;

            _uriParser =
                from scheme in Parse.Or(Parse.String("mongodb://"), Parse.String("mongodb+srv://")).Text()
                from userInfo in userInfoParser.Optional()
                from hosts in hostsParser
                from adminDb in adminDbParser.Optional()
                from options in optionsParser.Optional()
                from closeSlash in Parse.Char('/').Optional()
                select new MongoUriParseResult(scheme,
                userInfo.IsEmpty ? null : userInfo.Get().Item1,
                userInfo.IsEmpty ? null : userInfo.Get().Item2,
                hosts.ToList(),
                adminDb.IsEmpty ? null : adminDb.Get(),
                options.IsEmpty ? null : options.Get());
        }
        internal static MongoUriParseResult ParseUri(string uri)
        {
            return _uriParser.Parse(uri);
        }

    }

}
