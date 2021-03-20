using System;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Settings;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MongoDB.Client.Connection
{
    internal static class InitHelper
    {
        public static BsonDocument CreateInitialCommand(MongoClientSettings settings)
        {
            var command = CreateCommand();
            AddClientDocumentToCommand(command, settings.ApplicationName ?? string.Empty);
            AddCompressorsToCommand(command, Compressors);
            if (settings.Login is not null && settings.Password is not null)
            {
                AddLoginInfoToCommand(command, settings.Login, settings.Password, settings.AdminDB);
            }
            return command;
        }
        private static byte[] CreateScramLoginBytes(byte[] login)
        {
            var prepared = PrepareLogin(login);
            var randonBytes = GenerateRandonBytes(20);
            var len = 5 + 3 + prepared.Length + 20;
            var bytes = new byte[len];
            Span<byte> span = bytes;
            var loginSlice = span.Slice(5, prepared.Length);
            var randomSlice = span.Slice(5 + prepared.Length + 3, 20);
            bytes[0] = 110;
            bytes[1] = 44;
            bytes[2] = 44;
            bytes[3] = 110;
            bytes[4] = 61;
            prepared.CopyTo(loginSlice);
            bytes[5 + prepared.Length] = 44;
            bytes[5 + prepared.Length + 1] = 114;
            bytes[5 + prepared.Length + 2] = 61;
            randonBytes.CopyTo(randomSlice);
            return bytes;
        }

        private static byte[] GenerateRandonBytes(int len)
        {
            var rnd = new Random();
            var array = new byte[len];
            for (int i = 0; i < len; i++)
            {
                array[i] = (byte)rnd.Next(33, 126);
            }

            return array;
        }
        private static byte[] PrepareLogin(byte[] rawLogin)
        {
            var badBytesCount = 0;
            var span = rawLogin;
            for (int i = 0; i < rawLogin.Length - 1; i++)
            {
                if (span[i] == 61 || span[i] == 44)
                {
                    badBytesCount += 1;
                }
            }

            if (badBytesCount == 0)
            {
                return rawLogin;
            }

            var newLen = rawLogin.Length + (badBytesCount * 2);
            var newBytes = new byte[newLen];
            var newIdx = 0;
            for (int i = 0; i < rawLogin.Length - 1; i++)
            {
                var oldByte = span[i];
                switch (oldByte)
                {
                    case 61:
                        newBytes[++newIdx] = 61;
                        newBytes[++newIdx] = 51;
                        newBytes[++newIdx] = 68;
                        break;
                    case 44: 
                        newBytes[++newIdx] = 61;
                        newBytes[++newIdx] = 50;
                        newBytes[++newIdx] = 67;
                        break;
                    default: 
                        newBytes[++newIdx] = span[i];
                        break;
                }
            }

            return newBytes;
        }
        public static BsonDocument CreateSaslStart(MongoClientSettings settings)
        {
            var document = new BsonDocument();
            document.Add("saslContinue", 1);
            document.Add("conversationId", 1);
            document.Add("payload", BsonBinaryData.Create(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("n,,n=gamover,r=.`@cYWf%6FKEx?WCtDE9"))));
            //document.Add("payload", BsonBinaryData.Create(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(settings.Login))));
            //document.Add("payload", BsonBinaryData.Create(Encoding.UTF8.GetBytes("c=biws,r=.`@cYWf%6FKEx?WCtDE96MxRvkhr3OD7pNbXztr9wSPdUNY4J1sh,p=4Xgx4lMurUvq/iMPdX0/V41bdqzHwFSFUn59OwBdkFc=")));
            return document;
        }
        private static void AddLoginInfoToCommand(BsonDocument command, byte[] login, byte[] password, string db)
        {
            var loginStr = Encoding.UTF8.GetString(login);
            command.Add("saslSupportedMechs", $"{db}.{loginStr}");
            BsonDocument speculativeAuthenticate = new BsonDocument();
            speculativeAuthenticate.Add("saslStart", 1);
            speculativeAuthenticate.Add("mechanism", "SCRAM-SHA-256");
            speculativeAuthenticate.Add("payload", BsonBinaryData.Create(CreateScramLoginBytes(login)));
            //speculativeAuthenticate.Add("payload", BsonBinaryData.Create(Encoding.UTF8.GetBytes("n,,n=gamover,r=.`@cYWf%6FKEx?WCtDE9")));
            speculativeAuthenticate.Add("options", new BsonDocument("skipEmptyExchange", true));
            speculativeAuthenticate.Add("db", db);
            command.Add("speculativeAuthenticate", speculativeAuthenticate);
        }
        private static void AddClientDocumentToCommand(BsonDocument command, string appname)
        {
            var clientDocument = CreateClientDocument(appname);
            command.Add("client", clientDocument);
        }

        private static void AddCompressorsToCommand(BsonDocument command, BsonDocument compressors)
        {
            command.Add("compression", compressors);
        }

        private static BsonDocument CreateCommand()
        {
            BsonDocument? topologyVersion = null;
            //long? maxAwaitTimeMS = null;
            return new BsonDocument
            {
                { "isMaster", 1 },
                { "topologyVersion", topologyVersion, topologyVersion != null },
               // { "maxAwaitTimeMS", maxAwaitTimeMS, maxAwaitTimeMS.HasValue }
            };
        }

        private static readonly BsonArray Compressors = new BsonArray();

        private static BsonDocument CreateClientDocument(string applicationName)
        {
            var driverDocument = CreateDriverDocument();
            var osDocument = CreateOSDocument();
            var platformString = GetPlatformString();

            return CreateClientDocument(applicationName, driverDocument, osDocument, platformString);
        }



        private static BsonDocument CreateClientDocument(string applicationName, BsonDocument driverDocument, BsonDocument osDocument, string platformString)
        {
            var clientDocument = new BsonDocument
            {
                { "application", applicationName == null ? null : new BsonDocument("name", applicationName) },
                { "driver", driverDocument },
                { "os", osDocument },
                { "platform", platformString }
            };

            return clientDocument;
        }

        private static BsonDocument CreateDriverDocument()
        {
            return new BsonDocument
            {
                { "name", "dotnet-mongo-driver" },
                { "version", "0.1.0.0" }
            };
        }


        private static BsonDocument CreateOSDocument()
        {
            string osType;
            string osName;
            string? architecture;
            string? osVersion;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                osType = "Windows";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                osType = "Linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                osType = "macOS";
            }
            else
            {
                osType = "unknown";
            }

            osName = RuntimeInformation.OSDescription.Trim();

            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.Arm: architecture = "arm"; break;
                case Architecture.Arm64: architecture = "arm64"; break;
                case Architecture.X64: architecture = "x86_64"; break;
                case Architecture.X86: architecture = "x86_32"; break;
                default: architecture = null; break;
            }

            var match = Regex.Match(osName, @" (?<version>\d+\.\d[^ ]*)");
            if (match.Success)
            {
                osVersion = match.Groups["version"].Value;
            }
            else
            {
                osVersion = null;
            }

            return CreateOSDocument(osType, osName, architecture, osVersion);
        }

        private static BsonDocument CreateOSDocument(string osType, string osName, string? architecture, string? osVersion)
        {
            return new BsonDocument
            {
                { "type", osType },
                { "name", osName },
                { "architecture", architecture},
                { "version", osVersion }
            };
        }

        private static string GetPlatformString()
        {
            return RuntimeInformation.FrameworkDescription;
        }
    }
}
