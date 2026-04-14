using MongoDB.Client.Utils;
using MongoDB.Client.Settings;
using MongoDB.Client.Exceptions;
using System.Linq;
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
        public static string uri6 => "mongodb://user%40domain:p%40ss%2Fword@localhost/db%2D01?appName=My%20App&authMechanism=SCRAM-SHA-256";
        public static string uri7 => "mongodb://readonly-user@localhost";
        public static string uri8 => "mongodb://user:pass@localhost/db?AuthSource=users&AUTHMECHANISM=SCRAM-SHA-256&AppName=My%20App&READPREFERENCETAGS=dc:ny&readpreferencetags=rack:r1";

        [Fact]
        public void UriTest1()
        {
            MongoUriParseResult result = MongoDBUriParser.ParseUri(uri1);

            Assert.NotNull(result.Login);
            Assert.NotNull(result.Password);
            Assert.True(result.Login.SequenceEqual("gamover"));
            Assert.True(result.Password.SequenceEqual("12345"));
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
            Assert.True(result.Login.SequenceEqual("myDBReader"));
            Assert.True(result.Password.SequenceEqual("D1fficultP@ssw0rd"));
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
            Assert.True(result.Login.SequenceEqual("sysop"));
            Assert.True(result.Password.SequenceEqual("moon"));
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

        [Fact]
        public void UriTest6_PercentDecoding()
        {
            MongoUriParseResult result = MongoDBUriParser.ParseUri(uri6);

            Assert.Equal("user@domain", result.Login);
            Assert.Equal("p@ss/word", result.Password);
            Assert.Equal("db-01", result.AdminDb);
            Assert.Equal("My App", result.Options["appName"]);
            Assert.Equal("SCRAM-SHA-256", result.Options["authMechanism"]);
        }

        [Fact]
        public void UriTest7_UsernameWithoutPassword()
        {
            MongoUriParseResult result = MongoDBUriParser.ParseUri(uri7);

            Assert.Equal("readonly-user", result.Login);
            Assert.Null(result.Password);
            Assert.Equal("Unspecified/localhost:27017", result.Hosts.ToArray()[0].ToString());
        }

        [Fact]
        public void UriTest8_EmptyAuthSourceThrows()
        {
            var ex = Assert.Throws<MongoDBUriParserException>(() => MongoDBUriParser.ParseUri("mongodb://user:pass@localhost/?authSource="));
            Assert.Equal("authSource must not be empty.", ex.Message);
        }

        [Fact]
        public void UriTest9_DatabaseNameIsNotRestrictedToLetters()
        {
            MongoUriParseResult result = MongoDBUriParser.ParseUri("mongodb://user:pass@localhost/db-01_2026");

            Assert.Equal("db-01_2026", result.AdminDb);
        }

        [Fact]
        public void UriTest10_EmptyHostBetweenCommasThrows()
        {
            var ex = Assert.Throws<MongoDBUriParserException>(() => MongoDBUriParser.ParseUri("mongodb://host1,,host2"));

            Assert.Equal("MongoDB host must not be empty.", ex.Message);
        }

        [Fact]
        public void UriTest11_NonNumericPortThrows()
        {
            var ex = Assert.Throws<MongoDBUriParserException>(() => MongoDBUriParser.ParseUri("mongodb://host:badport"));

            Assert.Equal("MongoDB port 'badport' is invalid for host 'host'.", ex.Message);
        }

        [Fact]
        public void UriTest12_EmptyPortThrows()
        {
            var ex = Assert.Throws<MongoDBUriParserException>(() => MongoDBUriParser.ParseUri("mongodb://host:"));

            Assert.Equal("MongoDB port '' is invalid for host 'host'.", ex.Message);
        }

        [Theory]
        [InlineData("mongodb://host:70000", "MongoDB port '70000' is invalid for host 'host'.")]
        [InlineData("mongodb://host:-1", "MongoDB port '-1' is invalid for host 'host'.")]
        public void UriTest13_PortOutsideValidRangeThrows(string uri, string expectedMessage)
        {
            var ex = Assert.Throws<MongoDBUriParserException>(() => MongoDBUriParser.ParseUri(uri));

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData("mongodb://host::27017", "MongoDB host entry 'host::27017' is invalid.")]
        [InlineData("mongodb://host:bad:27017", "MongoDB host entry 'host:bad:27017' is invalid.")]
        public void UriTest14_HostEntriesWithMultipleColonsThrow(string uri, string expectedMessage)
        {
            var ex = Assert.Throws<MongoDBUriParserException>(() => MongoDBUriParser.ParseUri(uri));

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData("mongodb://:")]
        [InlineData("mongodb://:bad")]
        [InlineData("mongodb://:27017")]
        public void UriTest15_EmptyHostBeforeColonThrows(string uri)
        {
            var ex = Assert.Throws<MongoDBUriParserException>(() => MongoDBUriParser.ParseUri(uri));

            Assert.Equal("MongoDB host must not be empty.", ex.Message);
        }

        [Theory]
        [InlineData("mongodb://[::1]", "::1", 27017)]
        [InlineData("mongodb://[::1]:27017", "::1", 27017)]
        [InlineData("mongodb://[2001:db8::1]", "2001:db8::1", 27017)]
        public void UriTest16_BracketedIpv6HostParsing(string uri, string expectedHost, int expectedPort)
        {
            MongoUriParseResult result = MongoDBUriParser.ParseUri(uri);

            var endpoint = Assert.IsType<System.Net.DnsEndPoint>(result.Hosts.Single());
            Assert.Equal(expectedHost, endpoint.Host);
            Assert.Equal(expectedPort, endpoint.Port);
        }

        [Theory]
        [InlineData("mongodb://[]", "MongoDB host must not be empty.")]
        [InlineData("mongodb://[::1]:", "MongoDB port '' is invalid for host '::1'.")]
        [InlineData("mongodb://[::1]:bad", "MongoDB port 'bad' is invalid for host '::1'.")]
        [InlineData("mongodb://[::1]:70000", "MongoDB port '70000' is invalid for host '::1'.")]
        [InlineData("mongodb://[::1", "MongoDB host entry '[::1' is invalid.")]
        [InlineData("mongodb://[::1]extra", "MongoDB host entry '[::1]extra' is invalid.")]
        public void UriTest17_InvalidBracketedIpv6FormsThrow(string uri, string expectedMessage)
        {
            var ex = Assert.Throws<MongoDBUriParserException>(() => MongoDBUriParser.ParseUri(uri));

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void SettingsTest1_AuthSourcePrecedence_ExplicitWins()
        {
            MongoClientSettings settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost/records?authSource=users");

            Assert.Equal("users", settings.AdminDB);
        }

        [Fact]
        public void SettingsTest2_AuthSourcePrecedence_DatabaseWinsWhenAuthSourceMissing()
        {
            MongoClientSettings settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost/records");

            Assert.Equal("records", settings.AdminDB);
        }

        [Fact]
        public void SettingsTest3_AuthSourcePrecedence_AdminDefaultWhenCredentialsExist()
        {
            MongoClientSettings settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost");

            Assert.Equal("admin", settings.AdminDB);
        }

        [Fact]
        public void SettingsTest4_UsernameWithoutPasswordIsPreserved()
        {
            MongoClientSettings settings = MongoClientSettings.FromConnectionString(uri7);

            Assert.Equal("readonly-user", settings.Login);
            Assert.Null(settings.Password);
            Assert.Equal("admin", settings.AdminDB);
        }

        [Fact]
        public void SettingsTest5_ExplicitAuthMechanismIsPreserved()
        {
            MongoClientSettings settings = MongoClientSettings.FromConnectionString(uri6);

            Assert.Equal("SCRAM-SHA-256", settings.AuthMechanism);
        }

        [Fact]
        public void UriTest18_OptionsLookupIsCaseInsensitive()
        {
            MongoUriParseResult result = MongoDBUriParser.ParseUri(uri8);

            Assert.Equal("users", result.Options["authSource"]);
            Assert.Equal("SCRAM-SHA-256", result.Options["authMechanism"]);
            Assert.Equal("My App", result.Options["appName"]);
            Assert.Equal("dc:ny&rack:r1", result.Options["readPreferenceTags"]);
        }

        [Fact]
        public void SettingsTest6_AuthOptionsApplyCaseInsensitively()
        {
            MongoClientSettings settings = MongoClientSettings.FromConnectionString(uri8);

            Assert.Equal("users", settings.AdminDB);
            Assert.Equal("SCRAM-SHA-256", settings.AuthMechanism);
            Assert.Equal("My App", settings.ApplicationName);
        }
    }
}
