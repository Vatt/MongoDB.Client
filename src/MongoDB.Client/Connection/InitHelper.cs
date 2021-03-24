using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Connection
{
    internal static class InitHelper
    {
        public static BsonDocument CreateInitialCommand(MongoClientSettings settings)
        {
            var command = CreateCommand();
            AddClientDocumentToCommand(command, settings.ApplicationName ?? string.Empty);
            AddCompressorsToCommand(command, Compressors);
            return command;
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