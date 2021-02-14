using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;
namespace MongoDB.Client.Utils
{
    internal partial class MongoDBUriParser
    {
        public static void ParseUri(string uriStr)
        {
            var scheme = Terms.Text("mongodb");
            var colon = Terms.Char(':');
            var doubleSlash = Terms.Text("//");
            var slash = Terms.Text("/");
            var tt = scheme.And(colon).And(doubleSlash);
            var host = OneOrMany(Terms.NonWhiteSpace());
            //var ttt = scheme
            //    .And(colon)
            //    .And(Between(doubleSlash, host, slash))
            //    //.And(host)
            //    //.And(slash)
            //    .Parse(uriStr);
            var qwe = Between(doubleSlash, Terms.NonWhiteSpace(), slash).Parse(uriStr);
        }
    }
}
