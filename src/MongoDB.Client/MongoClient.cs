using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Common;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    public class MongoClient : IAsyncDisposable
    {
        private readonly EndPoint _endPoint;

        private ConnectionInfo? _connectionInfo;
        private readonly Channel _channel;
        private readonly BsonDocument _initialDocument;

        //  private static readonly byte[] Config = new byte[] { 42, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 212, 7, 0, 0, 4, 0, 0, 0, 97, 100, 109, 105, 110, 46, 36, 99, 109, 100, 0, 0, 0, 0, 0, 255, 255, 255, 255, 3, 1, 0, 0, 16, 105, 115, 77, 97, 115, 116, 101, 114, 0, 1, 0, 0, 0, 3, 99, 108, 105, 101, 110, 116, 0, 214, 0, 0, 0, 3, 100, 114, 105, 118, 101, 114, 0, 56, 0, 0, 0, 2, 110, 97, 109, 101, 0, 20, 0, 0, 0, 109, 111, 110, 103, 111, 45, 99, 115, 104, 97, 114, 112, 45, 100, 114, 105, 118, 101, 114, 0, 2, 118, 101, 114, 115, 105, 111, 110, 0, 8, 0, 0, 0, 48, 46, 48, 46, 48, 46, 48, 0, 0, 3, 111, 115, 0, 111, 0, 0, 0, 2, 116, 121, 112, 101, 0, 8, 0, 0, 0, 87, 105, 110, 100, 111, 119, 115, 0, 2, 110, 97, 109, 101, 0, 29, 0, 0, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 87, 105, 110, 100, 111, 119, 115, 32, 49, 48, 46, 48, 46, 49, 57, 48, 52, 49, 0, 2, 97, 114, 99, 104, 105, 116, 101, 99, 116, 117, 114, 101, 0, 7, 0, 0, 0, 120, 56, 54, 95, 54, 52, 0, 2, 118, 101, 114, 115, 105, 111, 110, 0, 11, 0, 0, 0, 49, 48, 46, 48, 46, 49, 57, 48, 52, 49, 0, 0, 2, 112, 108, 97, 116, 102, 111, 114, 109, 0, 16, 0, 0, 0, 46, 78, 69, 84, 32, 67, 111, 114, 101, 32, 51, 46, 49, 46, 57, 0, 0, 4, 99, 111, 109, 112, 114, 101, 115, 115, 105, 111, 110, 0, 5, 0, 0, 0, 0, 0 };
        private static readonly byte[] Hell = new byte[] { 59, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 212, 7, 0, 0, 4, 0, 0, 0, 97, 100, 109, 105, 110, 46, 36, 99, 109, 100, 0, 0, 0, 0, 0, 255, 255, 255, 255, 20, 0, 0, 0, 16, 98, 117, 105, 108, 100, 73, 110, 102, 111, 0, 1, 0, 0, 0, 0 };

        public MongoClient(EndPoint? endPoint = null)
        {
            _endPoint = endPoint ?? new DnsEndPoint("localhost", 27017);
            _channel = new Channel(_endPoint);
            _initialDocument = CreateInitialCommand();
        }

        public ValueTask<ConnectionInfo> ConnectAsync(CancellationToken cancellationToken)
        {
            if (_connectionInfo is not null)
            {
                return new ValueTask<ConnectionInfo>(_connectionInfo);
            }

            return Slow(cancellationToken);

            async ValueTask<ConnectionInfo> Slow(CancellationToken cancellationToken)
            {
                await _channel.ConnectAsync(cancellationToken).ConfigureAwait(false);
                QueryMessage? request = CreateConnectRequest();
                var configMessage = await _channel.SendQueryAsync<MongoConnectionInfo>(request, cancellationToken);
                var hell = await _channel.SendAsync<BsonDocument>(Hell, cancellationToken);
                _connectionInfo = new ConnectionInfo(configMessage, hell);
                return _connectionInfo;
            }
        }

        public ValueTask<TResp> SendAsync<TResp>(ReadOnlyMemory<byte> message, CancellationToken cancellationToken)
        {
            return _channel.SendAsync<TResp>(message, cancellationToken);
        }

        public ValueTask<Cursor<TResp>> GetCursorAsync<TResp>(ReadOnlyMemory<byte> message, CancellationToken cancellationToken)
        {
            return _channel.GetCursorAsync<TResp>(message, cancellationToken);
        }

        public ValueTask<Cursor<TResp>> GetCursorAsync<TResp>(string database, CancellationToken cancellationToken)
        {
            var doc = new BsonDocument
            {
                {"find", database },
                {"filter", new BsonDocument() }
            };
            
            var request = CreateFindRequest(database, doc);
            return _channel.GetCursorAsync<TResp>(request, cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return _channel.DisposeAsync();
        }

        private int counter;

        private QueryMessage CreateConnectRequest()
        {
            var doc = CreateWrapperDocument();
            return CreateRequest("admin.$cmd", Opcode.Query, doc);
        }

        private QueryMessage CreateRequest(string database, Opcode opcode, BsonDocument document)
        {
            var requestNumber = Interlocked.Increment(ref counter);
            return new QueryMessage(requestNumber, database, opcode,document);
        }

        private MsgMessage CreateFindRequest(string database, BsonDocument document)
        {
            var requestNumber = Interlocked.Increment(ref counter);
            return new MsgMessage(requestNumber, database, document);
        }

        private BsonDocument CreateWrapperDocument()
        {
            BsonDocument? readPreferenceDocument = null;
            var doc = new BsonDocument
            {
                { "$query", _initialDocument },
                { "$readPreference", readPreferenceDocument, readPreferenceDocument != null}
            };

            if (doc.Count == 1)
            {
                return doc["$query"].AsBsonDocument;
            }
            else
            {
                return doc;
            }
        }


        private static BsonDocument CreateInitialCommand()
        {
            var command = CreateCommand();
            AddClientDocumentToCommand(command);
            AddCompressorsToCommand(command, _compressors);
            return command;
        }

        internal static void AddClientDocumentToCommand(BsonDocument command)
        {
            var clientDocument = CreateClientDocument("testapp");
            command.Add("client", clientDocument);
        }

        internal static void AddCompressorsToCommand(BsonDocument command, BsonDocument compressors)
        {
            command.Add("compression", compressors);
        }

        internal static BsonDocument CreateCommand()
        {
            BsonDocument? topologyVersion = null;
            long? maxAwaitTimeMS = null;
            return new BsonDocument
            {
                { "isMaster", 1 },
                { "topologyVersion", topologyVersion, topologyVersion != null },
                { "maxAwaitTimeMS", maxAwaitTimeMS, maxAwaitTimeMS.HasValue }
            };
        }

        private static readonly BsonArray _compressors = new BsonArray();

        internal static BsonDocument CreateClientDocument(string applicationName)
        {
            var driverDocument = CreateDriverDocument();
            var osDocument = CreateOSDocument();
            var platformString = GetPlatformString();

            return CreateClientDocument(applicationName, driverDocument, osDocument, platformString);
        }



        internal static BsonDocument CreateClientDocument(string applicationName, BsonDocument driverDocument, BsonDocument osDocument, string platformString)
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

        internal static BsonDocument CreateDriverDocument()
        {
            return new BsonDocument
            {
                { "name", "my-mongo-driver" },
                { "version", "0.1.0.0" }
            };
        }


        internal static BsonDocument CreateOSDocument()
        {
            string osType;
            string osName;
            string architecture;
            string osVersion;

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

        internal static BsonDocument CreateOSDocument(string osType, string osName, string architecture, string osVersion)
        {
            return new BsonDocument
            {
                { "type", osType },
                { "name", osName },
                { "architecture", architecture},
                { "version", osVersion }
            };
        }

        internal static string GetPlatformString()
        {
            return RuntimeInformation.FrameworkDescription;
        }

    }
}
