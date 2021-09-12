using MongoDB.Client.Utils;
using Xunit;

namespace MongoDB.Client.Tests.UriParser
{
    public class UriTests
    {
        public static string uri1 => @"mongodb://gamover:12345@centos1.mshome.net:3340 , centos2.mshome.net,centos3.mshome.net, 192.168.1.1:3341/TestAdminDB/?replicaSet=rs0&maxPoolSize=32&appName=MongoDB.Client.ConsoleApp/";
        public static string uri2 => "mongodb://myDBReader:D1fficultP%40ssw0rd@mongodb0.example.com:27017,mongodb1.example.com:27017,mongodb2.example.com:27017/?authSource=admin&replicaSet=myRepl";
        public static string uri3 => "mongodb://sysop:moon@localhost/records";
        public static string uri4 => "mongodb://mongos1.example.com,mongos2.example.com/?readPreference=secondary&readPreferenceTags=dc:ny,rack:r1&readPreferenceTags=dc:ny&readPreferenceTags=";
        public static string uri5 => "mongodb+srv://server.example.com/?connectTimeoutMS=300000&authSource=aDifferentAuthDB";

        [Fact]
        public void UriTest1()
        {
            MongoUriParseResult result = MongoDBUriParser.ParseUri(uri1);

            Assert.NotNull(result.Login);
            Assert.NotNull(result.Password);
            Assert.Equal("gamover", result.Login);
            Assert.Equal("12345", result.Password);
            Assert.Equal("Unspecified/centos1.mshome.net:3340", result.Hosts.ToArray()[0].ToString());
            Assert.Equal("Unspecified/centos2.mshome.net:27017", result.Hosts.ToArray()[1].ToString());
            Assert.Equal("Unspecified/centos3.mshome.net:27017", result.Hosts.ToArray()[2].ToString());
            Assert.Equal("Unspecified/192.168.1.1:3341", result.Hosts.ToArray()[3].ToString());
            Assert.Equal("TestAdminDB", result.AdminDb);
            Assert.True(result.Options.Count == 3);
            if (result.Options.TryGetValue("replicaSet", out var replicaSet))
            {
                Assert.Equal("rs0", replicaSet);
            }
            else
            {
                Assert.True(false);
            }
            if (result.Options.TryGetValue("maxPoolSize", out var maxPoolSize))
            {
                Assert.Equal("32", maxPoolSize);
            }
            else
            {
                Assert.True(false);
            }
            if (result.Options.TryGetValue("appName", out var appName))
            {
                Assert.Equal("MongoDB.Client.ConsoleApp", appName);
            }
            else
            {
                Assert.True(false);
            }
        }
        [Fact]
        public void UriTest2()
        {
            MongoUriParseResult result = MongoDBUriParser.ParseUri(uri2);

            Assert.NotNull(result.Login);
            Assert.NotNull(result.Password);
            Assert.Equal("myDBReader", result.Login);
            Assert.Equal("D1fficultP%40ssw0rd", result.Password);
            Assert.Equal("Unspecified/mongodb0.example.com:27017", result.Hosts.ToArray()[0].ToString());
            Assert.Equal("Unspecified/mongodb1.example.com:27017", result.Hosts.ToArray()[1].ToString());
            Assert.Equal("Unspecified/mongodb2.example.com:27017", result.Hosts.ToArray()[2].ToString());
            Assert.True(result.Options.Count == 2);
            if (result.Options.TryGetValue("replicaSet", out var replicaSet))
            {
                Assert.Equal("myRepl", replicaSet);
            }
            else
            {
                Assert.True(false);
            }
            if (result.Options.TryGetValue("authSource", out var authSource))
            {
                Assert.Equal("admin", authSource);
            }
            else
            {
                Assert.True(false);
            }
        }

        [Fact]
        public void UriTest3()
        {
            MongoUriParseResult result = MongoDBUriParser.ParseUri(uri3);

            Assert.NotNull(result.Login);
            Assert.NotNull(result.Password);
            Assert.Equal("sysop", result.Login);
            Assert.Equal("moon", result.Password);
            Assert.Equal("Unspecified/localhost:27017", result.Hosts.ToArray()[0].ToString());
            Assert.Equal("records", result.AdminDb);
            Assert.True(result.Options.Count == 0);
        }
        [Fact]
        public void UriTest4()
        {
            MongoUriParseResult result = MongoDBUriParser.ParseUri(uri4);

            Assert.Null(result.Login);
            Assert.Null(result.Password);
            Assert.Equal("Unspecified/mongos1.example.com:27017", result.Hosts.ToArray()[0].ToString());
            Assert.Equal("Unspecified/mongos2.example.com:27017", result.Hosts.ToArray()[1].ToString());
            Assert.Null(result.AdminDb);
            Assert.True(result.Options.Count == 2);
            if (result.Options.TryGetValue("readPreference", out var readPreference))
            {
                Assert.Equal("secondary", readPreference);
            }
            else
            {
                Assert.True(false);
            }
            if (result.Options.TryGetValue("readPreferenceTags", out var readPreferenceTags))
            {
                Assert.Equal("dc:ny,rack:r1&dc:ny", readPreferenceTags);
            }
            else
            {
                Assert.True(false);
            }
        }

        [Fact]
        public void UriTest5()
        {
            MongoUriParseResult result = MongoDBUriParser.ParseUri(uri5);
            Assert.Equal("mongodb+srv://", result.Scheme);
            Assert.Null(result.Login);
            Assert.Null(result.Password);
            Assert.Equal("Unspecified/server.example.com:27017", result.Hosts.ToArray()[0].ToString());
            Assert.Null(result.AdminDb);
            Assert.True(result.Options.Count == 2);
            if (result.Options.TryGetValue("connectTimeoutMS", out var connectTimeoutMS))
            {
                Assert.Equal("300000", connectTimeoutMS);
            }
            else
            {
                Assert.True(false);
            }
            if (result.Options.TryGetValue("authSource", out var authSource))
            {
                Assert.Equal("aDifferentAuthDB", authSource);
            }
            else
            {
                Assert.True(false);
            }
        }
    }
}
