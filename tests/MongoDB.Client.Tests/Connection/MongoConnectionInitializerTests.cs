using System.Security.Cryptography;
using System.Text;
using MongoDB.Client.Authentication;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Settings;
using Xunit;

namespace MongoDB.Client.Tests.Connection
{
    public class MongoConnectionInitializerTests
    {
        private static readonly UTF8Encoding Strict = new(false, true);

        [Fact]
        public async Task InitializeAsync_RunsPluginHandshakeFlowAndReturnsConnectionInfo()
        {
            var settings = new MongoClientSettings();
            var plugin = new RecordingInitializerPlugin();
            var initializer = MongoConnectionInitializerFactory.Create(settings, plugin);
            var helloResponse = new BsonDocument("isWritablePrimary", true);
            var buildInfoResponse = new BsonDocument("version", "8.0.0");
            var connection = new RecordingMongoConnection(helloResponse, buildInfoResponse);

            var info = await initializer.InitializeAsync(connection, CancellationToken.None);

            Assert.Equal(2, connection.Requests.Count);
            Assert.True(connection.Requests[0].TryGet("isMaster", out var helloCommand));
            Assert.Equal(1, helloCommand.AsInt);
            Assert.True(connection.Requests[0].TryGet("pluginMarker", out var marker));
            Assert.True((bool)marker!.Value!);
            Assert.True(connection.Requests[1].TryGet("buildInfo", out var buildInfoCommand));
            Assert.Equal(1, buildInfoCommand.AsInt);
            Assert.Same(helloResponse, plugin.HelloResult);
            Assert.Same(buildInfoResponse, plugin.BuildInfoResult);
            Assert.Same(helloResponse, info.IsMaster);
            Assert.Same(buildInfoResponse, info.BuildInfo);
        }

        [Fact]
        public async Task InitializeAsync_RunsAfterHelloStepsBeforeBuildInfo()
        {
            var settings = new MongoClientSettings();
            var plugin = new SequencingInitializerPlugin();
            var initializer = MongoConnectionInitializerFactory.Create(settings, plugin);
            var helloResponse = new BsonDocument("isWritablePrimary", true);
            var authResponse = new BsonDocument("ok", 1);
            var buildInfoResponse = new BsonDocument("version", "8.0.0");
            var connection = new RecordingMongoConnection(helloResponse, authResponse, buildInfoResponse);

            var info = await initializer.InitializeAsync(connection, CancellationToken.None);

            Assert.Equal(3, connection.Requests.Count);
            Assert.True(connection.Requests[0].TryGet("isMaster", out _));
            Assert.True(connection.Requests[1].TryGet("authenticate", out var authenticateCommand));
            Assert.Equal(1, authenticateCommand.AsInt);
            Assert.True(connection.Requests[2].TryGet("buildInfo", out _));
            Assert.Same(helloResponse, plugin.HelloSeenDuringAuth);
            Assert.Same(buildInfoResponse, plugin.BuildInfoSeenAfterBuildInfo);
            Assert.Same(buildInfoResponse, info.BuildInfo);
        }

        [Fact]
        public async Task InitializeAsync_WhenInvokedConcurrently_KeepsPluginStatePerCall()
        {
            var settings = new MongoClientSettings();
            var plugin = new PerCallStateInitializerPlugin();
            var initializer = MongoConnectionInitializerFactory.Create(settings, plugin);
            var firstConnection = new RecordingMongoConnection(
                new BsonDocument("connection", "first"),
                new BsonDocument("ok", 1),
                new BsonDocument("version", "8.0.0"));
            var secondConnection = new RecordingMongoConnection(
                new BsonDocument("connection", "second"),
                new BsonDocument("ok", 1),
                new BsonDocument("version", "8.0.0"));

            await Task.WhenAll(
                initializer.InitializeAsync(firstConnection, CancellationToken.None).AsTask(),
                initializer.InitializeAsync(secondConnection, CancellationToken.None).AsTask());

            Assert.Equal("first", firstConnection.Requests[1]["stateConnection"].AsString);
            Assert.Equal("second", secondConnection.Requests[1]["stateConnection"].AsString);
        }

        [Fact]
        public void FactoryCreate_WithoutPlugins_Throws()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://readonly-user@localhost");

            var exception = Assert.Throws<ArgumentException>(() => MongoConnectionInitializerFactory.Create(settings));

            Assert.Equal("plugins", exception.ParamName);
            Assert.Equal(
                "At least one connection initializer plugin is required. Use CreateRaw for an explicit no-auth initializer. (Parameter 'plugins')",
                exception.Message);
        }

        [Fact]
        public void Constructor_WithoutPlugins_Throws()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://readonly-user@localhost");

            var exception = Assert.Throws<ArgumentException>(() => new MongoConnectionInitializer(settings));

            Assert.Equal("plugins", exception.ParamName);
            Assert.Equal(
                "At least one connection initializer plugin is required. Use CreateRaw for an explicit no-auth initializer. (Parameter 'plugins')",
                exception.Message);
        }

        [Fact]
        public async Task FactoryCreateRaw_DoesNotInjectAuthenticationBehavior()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://readonly-user@localhost");
            var initializer = MongoConnectionInitializerFactory.CreateRaw(settings);
            var connection = new RecordingMongoConnection(
                new BsonDocument("isWritablePrimary", true),
                new BsonDocument("version", "8.0.0"));

            var info = await initializer.InitializeAsync(connection, CancellationToken.None);

            Assert.Equal(2, connection.Requests.Count);
            Assert.True(connection.Requests[0].TryGet("isMaster", out _));
            Assert.False(connection.Requests[0].TryGet("speculativeAuthenticate", out _));
            Assert.False(connection.Requests[0].TryGet("saslSupportedMechs", out _));
            Assert.Same(connection.Responses[0], info.IsMaster);
            Assert.Same(connection.Responses[1], info.BuildInfo);
        }

        [Fact]
        public async Task CreateConnectionInitializer_WithoutCredentials_DoesNotInjectAuthenticationBehavior()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://localhost");
            var initializer = MongoClient.CreateConnectionInitializer(settings);
            var connection = new RecordingMongoConnection(
                new BsonDocument("isWritablePrimary", true),
                new BsonDocument("version", "8.0.0"));

            var info = await initializer.InitializeAsync(connection, CancellationToken.None);

            Assert.Equal(2, connection.Requests.Count);
            Assert.True(connection.Requests[0].TryGet("isMaster", out _));
            Assert.False(connection.Requests[0].TryGet("speculativeAuthenticate", out _));
            Assert.False(connection.Requests[0].TryGet("saslSupportedMechs", out _));
            Assert.True(connection.Requests[1].TryGet("buildInfo", out _));
            Assert.Same(connection.Responses[0], info.IsMaster);
            Assert.Same(connection.Responses[1], info.BuildInfo);
        }

        [Fact]
        public async Task CreateConnectionInitializer_WithCredentialsWithoutPassword_UsesScramPlugin()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://readonly-user@localhost");
            var initializer = MongoClient.CreateConnectionInitializer(settings);
            var connection = new RecordingMongoConnection(
                new BsonDocument("isWritablePrimary", true),
                new BsonDocument("version", "8.0.0"));

            var exception = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => initializer.InitializeAsync(connection, CancellationToken.None).AsTask());

            Assert.Equal(
                "Authentication requires a password when a login is provided. The current SCRAM implementation only supports password-based authentication. code: 0",
                exception.Message);
            Assert.Empty(connection.Requests);
        }

        [Fact]
        public async Task CreateConnectionInitializer_WithCredentials_UsesProductionScramHappyPath()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost");
            var initializer = MongoClient.CreateConnectionInitializer(settings);
            var buildInfoResponse = new BsonDocument("version", "8.0.0");
            var connection = new SpeculativeScramMongoConnection(settings, buildInfoResponse);

            var info = await initializer.InitializeAsync(connection, CancellationToken.None);

            Assert.Equal(3, connection.Requests.Count);

            var hello = connection.Requests[0];
            Assert.True(hello.TryGet("isMaster", out _));
            Assert.Equal("admin.user", hello["saslSupportedMechs"].AsString);

            var speculativeAuthenticate = hello["speculativeAuthenticate"].AsBsonDocument!;
            Assert.Equal("SCRAM-SHA-256", speculativeAuthenticate["mechanism"].AsString);
            Assert.Equal("admin", speculativeAuthenticate["db"].AsString);
            Assert.True(speculativeAuthenticate.TryGet("saslStart", out var saslStart));
            Assert.Equal(1, saslStart.AsInt);

            var continueCommand = connection.Requests[1];
            Assert.True(continueCommand.TryGet("saslContinue", out var saslContinue));
            Assert.Equal(1, saslContinue.AsInt);

            Assert.True(connection.Requests[2].TryGet("buildInfo", out _));
            Assert.Same(connection.HelloResponse, info.IsMaster);
            Assert.Same(buildInfoResponse, info.BuildInfo);
        }

        private sealed class RecordingInitializerPlugin : IMongoConnectionInitializerPlugin
        {
            public BsonDocument? HelloResult { get; private set; }
            public BsonDocument? BuildInfoResult { get; private set; }

            public void Configure(MongoConnectionInitializerPipelineBuilder builder)
            {
                builder.Add(
                    MongoConnectionInitializerPhase.BeforeHello,
                    (_, context, _) =>
                    {
                        context.HandshakeCommand.Add("pluginMarker", true);
                        return ValueTask.CompletedTask;
                    });

                builder.Add(
                    MongoConnectionInitializerPhase.AfterBuildInfo,
                    (_, context, _) =>
                    {
                        HelloResult = context.HelloResult;
                        BuildInfoResult = context.BuildInfoResult;
                        return ValueTask.CompletedTask;
                    });
            }
        }

        private sealed class SequencingInitializerPlugin : IMongoConnectionInitializerPlugin
        {
            public BsonDocument? HelloSeenDuringAuth { get; private set; }
            public BsonDocument? BuildInfoSeenAfterBuildInfo { get; private set; }

            public void Configure(MongoConnectionInitializerPipelineBuilder builder)
            {
                builder.Add(
                    MongoConnectionInitializerPhase.AfterHello,
                    async (connection, context, cancellationToken) =>
                    {
                        HelloSeenDuringAuth = context.HelloResult;
                        await connection.SendQueryAsync<BsonDocument>(
                            "admin.$cmd",
                            new BsonDocument("authenticate", 1),
                            cancellationToken).ConfigureAwait(false);
                    });

                builder.Add(
                    MongoConnectionInitializerPhase.AfterBuildInfo,
                    (_, context, _) =>
                    {
                        BuildInfoSeenAfterBuildInfo = context.BuildInfoResult;
                        return ValueTask.CompletedTask;
                    });
            }
        }

        private sealed class RecordingMongoConnection : IMongoConnection
        {
            private readonly Queue<BsonDocument> _responses;

            public RecordingMongoConnection(params BsonDocument[] responses)
            {
                _responses = new Queue<BsonDocument>(responses);
                Responses = responses;
            }

            public List<BsonDocument> Requests { get; } = new();
            public IReadOnlyList<BsonDocument> Responses { get; }

            public ValueTask DisposeAsync()
            {
                return ValueTask.CompletedTask;
            }

            public ValueTask<QueryResult<TResp>> SendQueryAsync<TResp>(string database, BsonDocument document, CancellationToken cancellationToken)
                where TResp : IBsonSerializer<TResp>
            {
                Requests.Add(document);
                var response = _responses.Dequeue();
                var result = new QueryResult<BsonDocument>(1);
                result.Add(response);
                return ValueTask.FromResult((QueryResult<TResp>)(object)result);
            }
        }

        private sealed class SpeculativeScramMongoConnection : IMongoConnection
        {
            private readonly MongoClientSettings _settings;
            private readonly BsonDocument _buildInfoResponse;
            private ScramConversationHarness? _server;

            public SpeculativeScramMongoConnection(MongoClientSettings settings, BsonDocument buildInfoResponse)
            {
                _settings = settings;
                _buildInfoResponse = buildInfoResponse;
                HelloResponse = new BsonDocument("isWritablePrimary", true);
            }

            public List<BsonDocument> Requests { get; } = new();
            public BsonDocument HelloResponse { get; }

            public ValueTask DisposeAsync()
            {
                return ValueTask.CompletedTask;
            }

            public ValueTask<QueryResult<TResp>> SendQueryAsync<TResp>(string database, BsonDocument document, CancellationToken cancellationToken)
                where TResp : IBsonSerializer<TResp>
            {
                Requests.Add(document);

                BsonDocument response = document.TryGet("isMaster", out _)
                    ? CreateHelloResponse(document)
                    : document.TryGet("saslContinue", out _)
                        ? _server!.CreateProofResponse(document)
                        : document.TryGet("buildInfo", out _)
                            ? _buildInfoResponse
                            : throw new InvalidOperationException("Unexpected command sequence.");

                var result = new QueryResult<BsonDocument>(1);
                result.Add(response);
                return ValueTask.FromResult((QueryResult<TResp>)(object)result);
            }

            private BsonDocument CreateHelloResponse(BsonDocument document)
            {
                var speculativeAuthenticate = document["speculativeAuthenticate"].AsBsonDocument!;
                var saslStart = CreateSaslStart(speculativeAuthenticate["payload"].AsByteArray!);
                _server = new ScramConversationHarness(_settings.Login!, _settings.Password!, "SCRAM-SHA-256", saslStart);

                var saslSupportedMechs = new BsonArray();
                saslSupportedMechs.Add("SCRAM-SHA-256");
                HelloResponse.Add("saslSupportedMechs", saslSupportedMechs);
                HelloResponse.Add("speculativeAuthenticate", _server.CreateSaslStartResponse());
                return HelloResponse;
            }
        }

        private sealed class ScramConversationHarness
        {
            private readonly string _username;
            private readonly string _password;
            private readonly string _mechanism;
            private readonly SaslStart _saslStart;
            private readonly byte[] _saltBytes;
            private readonly string _serverFirstMessage;

            public ScramConversationHarness(string username, string password, string mechanism, SaslStart saslStart)
            {
                _username = username;
                _password = password;
                _mechanism = mechanism;
                _saslStart = saslStart;
                _saltBytes = Encoding.ASCII.GetBytes("testsalt");
                var clientNonce = ExtractClientNonce(saslStart.BaseMessage);
                _serverFirstMessage = $"r={clientNonce}server,s={Convert.ToBase64String(_saltBytes)},i=4096";
            }

            public BsonDocument CreateSaslStartResponse()
            {
                return new BsonDocument
                {
                    { "ok", 1 },
                    { "conversationId", 41 },
                    { "done", false },
                    { "payload", BsonBinaryData.Create(Strict.GetBytes(_serverFirstMessage)) }
                };
            }

            public BsonDocument CreateProofResponse(BsonDocument command)
            {
                return new BsonDocument
                {
                    { "ok", 1 },
                    { "conversationId", 41 },
                    { "done", true },
                    { "payload", BsonBinaryData.Create(Strict.GetBytes(CreateServerFinalMessage(command))) }
                };
            }

            private string CreateServerFinalMessage(BsonDocument command)
            {
                var clientFinalMessage = Strict.GetString(command["payload"].AsByteArray!);
                var proofSeparator = clientFinalMessage.LastIndexOf(",p=", StringComparison.Ordinal);
                var clientFinalMessageWithoutProof = proofSeparator >= 0
                    ? clientFinalMessage.Substring(0, proofSeparator)
                    : clientFinalMessage;
                var authMessage = _saslStart.BaseMessage + "," + _serverFirstMessage + "," + clientFinalMessageWithoutProof;

                var saltedPassword = _mechanism switch
                {
                    "SCRAM-SHA-256" => Rfc2898DeriveBytes.Pbkdf2(_password, _saltBytes, 4096, HashAlgorithmName.SHA256, 32),
                    "SCRAM-SHA-1" => Rfc2898DeriveBytes.Pbkdf2(ComputeMongoHashedPassword(_username, _password), _saltBytes, 4096, HashAlgorithmName.SHA1, 20),
                    _ => throw new InvalidOperationException($"Unsupported SCRAM mechanism '{_mechanism}'.")
                };

                using var serverKeyHmac = CreateServerKeyHmac(saltedPassword);
                var serverKey = serverKeyHmac.ComputeHash(Strict.GetBytes("Server Key"));
                using var signatureHmac = CreateServerKeyHmac(serverKey);
                var signature = signatureHmac.ComputeHash(Strict.GetBytes(authMessage));
                return "v=" + Convert.ToBase64String(signature);
            }

            private HMAC CreateServerKeyHmac(byte[] key)
            {
                return _mechanism switch
                {
                    "SCRAM-SHA-256" => new HMACSHA256(key),
                    "SCRAM-SHA-1" => new HMACSHA1(key),
                    _ => throw new InvalidOperationException($"Unsupported SCRAM mechanism '{_mechanism}'.")
                };
            }
        }

        private static SaslStart CreateSaslStart(byte[] payload)
        {
            var payloadText = Strict.GetString(payload);
            const string gs2Header = "n,,";
            if (!payloadText.StartsWith(gs2Header, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("SCRAM payload did not start with the GS2 header.");
            }

            var baseMessage = payloadText.Substring(gs2Header.Length);
            const string nonceMarker = ",r=";
            var markerIndex = baseMessage.IndexOf(nonceMarker, StringComparison.Ordinal);
            if (markerIndex < 0)
            {
                throw new InvalidOperationException("SCRAM base message did not contain a client nonce.");
            }

            var nonce = baseMessage.Substring(markerIndex + nonceMarker.Length);
            return new SaslStart(Strict.GetBytes(nonce), baseMessage, payload);
        }

        private static string ComputeMongoHashedPassword(string username, string password)
        {
            using var md5 = MD5.Create();
            var digest = md5.ComputeHash(Strict.GetBytes(username + ":mongo:" + password));
            return Convert.ToHexString(digest).ToLowerInvariant();
        }

        private static string ExtractClientNonce(string baseMessage)
        {
            const string marker = ",r=";
            var markerIndex = baseMessage.IndexOf(marker, StringComparison.Ordinal);
            return markerIndex >= 0
                ? baseMessage.Substring(markerIndex + marker.Length)
                : throw new InvalidOperationException("SCRAM base message did not contain a client nonce.");
        }

        private sealed class PerCallStateInitializerPlugin : IMongoConnectionInitializerPlugin
        {
            private static readonly object StateKey = new();
            private readonly TaskCompletionSource _bothCallsEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private int _beforeHelloCalls;

            public void Configure(MongoConnectionInitializerPipelineBuilder builder)
            {
                builder.Add(
                    MongoConnectionInitializerPhase.AfterHello,
                    async (connection, context, cancellationToken) =>
                    {
                        context.SetState(StateKey, context.HelloResult!["connection"].AsString);

                        if (Interlocked.Increment(ref _beforeHelloCalls) == 2)
                        {
                            _bothCallsEntered.SetResult();
                        }

                        await _bothCallsEntered.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                    });

                builder.Add(
                    MongoConnectionInitializerPhase.BeforeBuildInfo,
                    async (connection, context, cancellationToken) =>
                    {
                        Assert.True(context.TryGetState<string>(StateKey, out var stateConnection));
                        await connection.SendQueryAsync<BsonDocument>(
                            "admin.$cmd",
                            new BsonDocument("stateConnection", stateConnection!),
                            cancellationToken).ConfigureAwait(false);
                    });
            }
        }
    }
}
