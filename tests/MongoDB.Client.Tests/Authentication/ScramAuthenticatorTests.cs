using System.Security.Cryptography;
using System.Text;
using MongoDB.Client.Authentication;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Settings;
using Xunit;

namespace MongoDB.Client.Tests.Authentication
{
    public class ScramAuthenticatorTests
    {
        private static readonly UTF8Encoding Strict = new(false, true);

        [Fact]
        public void AuthenticateIsMaster_UsesExplicitScramSha256MechanismInHandshake()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost/?authMechanism=SCRAM-SHA-256");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();

            var saslStart = authenticator.AuthenticateIsMaster(handshake);

            Assert.NotNull(saslStart);
            Assert.Equal("admin.user", handshake["saslSupportedMechs"].AsString);

            var speculativeAuthenticate = handshake["speculativeAuthenticate"].AsBsonDocument!;
            Assert.Equal("SCRAM-SHA-256", speculativeAuthenticate["mechanism"].AsString);
            Assert.Equal("admin", speculativeAuthenticate["db"].AsString);
        }

        [Fact]
        public void AuthenticateIsMaster_ThrowsForUnsupportedExplicitMechanism()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost/?authMechanism=SCRAM-SHA-1");
            var authenticator = new ScramAuthenticator(settings);

            var ex = Assert.Throws<MongoAuthentificationException>(() => authenticator.AuthenticateIsMaster(new BsonDocument()));

            Assert.Equal("Authentication mechanism 'SCRAM-SHA-1' is not supported by the current SCRAM implementation. code: 0", ex.Message);
        }

        [Fact]
        public void AuthenticateIsMaster_ThrowsWhenLoginProvidedWithoutPassword()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://readonly-user@localhost");
            var authenticator = new ScramAuthenticator(settings);

            var ex = Assert.Throws<MongoAuthentificationException>(() => authenticator.AuthenticateIsMaster(new BsonDocument()));

            Assert.Equal(
                "Authentication requires a password when a login is provided. The current SCRAM implementation only supports password-based authentication. code: 0",
                ex.Message);
        }

        [Fact]
        public void AuthenticateIsMaster_EscapesCommaAndEqualsInLogin()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user%2Cname%3Dqa:pass@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();

            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var payload = Strict.GetString(saslStart.Payload);

            Assert.StartsWith("n,,n=user=2Cname=3Dqa,r=", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("\0", payload, StringComparison.Ordinal);
        }

        [Fact]
        public async Task AuthenticateAsync_FallsBackToSaslStartWhenSpeculativeAuthenticateIsMissing()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost/?authSource=users");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var server = new ScramConversationHarness(settings.Password!, saslStart, longConversation: false);
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return server.CreateSaslStartResponse();
                    }

                    if (command.TryGet("saslContinue", out _) && command["payload"].AsByteArray!.Length != 0)
                    {
                        return server.CreateProofResponse(command);
                    }

                    throw new InvalidOperationException("Unexpected command sequence.");
                });

            await authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None);

            Assert.Equal(2, connection.Requests.Count);
            Assert.All(connection.Requests, request => Assert.Equal("users.$cmd", request.Database));
            Assert.True(connection.Requests[0].Command.TryGet("saslStart", out _));
            Assert.Equal("users", connection.Requests[0].Command["db"].AsString);
            Assert.True(connection.Requests[1].Command.TryGet("saslContinue", out _));
        }

        [Fact]
        public async Task AuthenticateAsync_CompletesShortConversationFromSpeculativeAuthenticate()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost/?authSource=users");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var server = new ScramConversationHarness(settings.Password!, saslStart, longConversation: false);
            var isMasterResult = new BsonDocument
            {
                { "speculativeAuthenticate", server.CreateSaslStartResponse() }
            };
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    Assert.True(command.TryGet("saslContinue", out _));
                    Assert.NotEmpty(command["payload"].AsByteArray!);
                    return server.CreateProofResponse(command);
                });

            await authenticator.AuthenticateAsync(connection, isMasterResult, saslStart, CancellationToken.None);

            Assert.Single(connection.Requests);
            Assert.Equal("users.$cmd", connection.Requests[0].Database);
        }

        [Fact]
        public async Task AuthenticateAsync_CompletesLongConversationWithEmptyFinalContinue()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost/?authSource=users");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var server = new ScramConversationHarness(settings.Password!, saslStart, longConversation: true);
            var isMasterResult = new BsonDocument
            {
                { "speculativeAuthenticate", server.CreateSaslStartResponse() }
            };
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command["payload"].AsByteArray!.Length == 0)
                    {
                        return server.CreateFinalResponse();
                    }

                    return server.CreateProofResponse(command);
                });

            await authenticator.AuthenticateAsync(connection, isMasterResult, saslStart, CancellationToken.None);

            Assert.Equal(2, connection.Requests.Count);
            Assert.NotEmpty(connection.Requests[0].Command["payload"].AsByteArray!);
            Assert.Empty(connection.Requests[1].Command["payload"].AsByteArray!);
        }

        [Fact]
        public async Task AuthenticateAsync_ThrowsWhenServerSignatureIsInvalid()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var server = new ScramConversationHarness(settings.Password!, saslStart, longConversation: false);
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return server.CreateSaslStartResponse();
                    }

                    return server.CreateProofResponse(command, "v=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");
                });

            var ex = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None));

            Assert.Equal("Server signature was invalid. code: 0", ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_ThrowsForUnexpectedServerResponse()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var server = new ScramConversationHarness(settings.Password!, saslStart, longConversation: false);
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return server.CreateSaslStartResponse();
                    }

                    return new BsonDocument
                    {
                        { "ok", 1 },
                        { "conversationId", server.ConversationId },
                        { "done", false },
                        { "payload", BsonBinaryData.Create(Strict.GetBytes("r=still-going")) }
                    };
                });

            var ex = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None));

            Assert.Equal("Unexpected SCRAM payload while authentication conversation is still in progress. code: 0", ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_RejectsServerNonceWithoutServerContribution()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var clientNonce = ExtractClientNonce(saslStart.BaseMessage);
            var serverFirstMessage = $"r={clientNonce},s={Convert.ToBase64String(Encoding.ASCII.GetBytes("testsalt"))},i=4096";
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return new BsonDocument
                        {
                            { "ok", 1 },
                            { "conversationId", 41 },
                            { "done", false },
                            { "payload", BsonBinaryData.Create(Strict.GetBytes(serverFirstMessage)) }
                        };
                    }

                    throw new InvalidOperationException("Unexpected command sequence.");
                });

            var ex = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None));

            Assert.Equal("Server sent an invalid nonce. code: 0", ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_RejectsReservedScramExtensionAttribute()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var server = new ScramConversationHarness(settings.Password!, saslStart, longConversation: false);
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return server.CreateSaslStartResponse();
                    }

                    return server.CreateProofResponse(command, "m=reserved,v=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");
                });

            var ex = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None));

            Assert.Equal("SCRAM server-final-message contained the reserved extension attribute 'm'. code: 0", ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_RejectsIterationCountBelowScramSha256Minimum()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var server = new ScramConversationHarness(settings.Password!, saslStart, longConversation: false, iterationCount: 4095);
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return server.CreateSaslStartResponse();
                    }

                    throw new InvalidOperationException("Unexpected command sequence.");
                });

            var ex = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None));

            Assert.Equal("SCRAM server-first-message iteration count must be at least 4096 for SCRAM-SHA-256. code: 0", ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_AppliesSaslPrepToPasswordBeforeKeyDerivation()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user:pass%C2%A0word@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var server = new ScramConversationHarness("pass word", saslStart, longConversation: false);
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return server.CreateSaslStartResponse();
                    }

                    return server.CreateProofResponse(command);
                });

            await authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None);

            Assert.Equal(2, connection.Requests.Count);
        }

        [Fact]
        public async Task AuthenticateAsync_RejectsPasswordContainingCodePointUnassignedInUnicode32BeforeNormalization()
        {
            var password = "pass\u1D2Cword";
            var settings = MongoClientSettings.FromConnectionString($"mongodb://user:{Uri.EscapeDataString(password)}@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return new ScramConversationHarness(password, saslStart, longConversation: false).CreateSaslStartResponse();
                    }

                    throw new InvalidOperationException("Unexpected command sequence.");
                });

            var ex = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None));

            Assert.Equal(
                "SCRAM-SHA-256 password contains a prohibited Unicode code point U+1D2C. code: 0",
                ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_UsesUnicode32NormalizationForCompatibilityMappings()
        {
            var password = "pass\U0002F868word";
            var settings = MongoClientSettings.FromConnectionString($"mongodb://user:{Uri.EscapeDataString(password)}@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var server = new ScramConversationHarness("pass\U0002136Aword", saslStart, longConversation: false);
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return server.CreateSaslStartResponse();
                    }

                    return server.CreateProofResponse(command);
                });

            await authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None);

            Assert.Equal(2, connection.Requests.Count);
        }

        [Fact]
        public async Task AuthenticateAsync_MapsZeroWidthJoinersToNothingBeforeKeyDerivation()
        {
            var password = "pass\u200Cword\u200D";
            var settings = MongoClientSettings.FromConnectionString($"mongodb://user:{Uri.EscapeDataString(password)}@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var server = new ScramConversationHarness("password", saslStart, longConversation: false);
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return server.CreateSaslStartResponse();
                    }

                    return server.CreateProofResponse(command);
                });

            await authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None);

            Assert.Equal(2, connection.Requests.Count);
        }

        [Fact]
        public async Task AuthenticateAsync_RejectsPasswordThatMixesRandALCatWithUnicode32LCatDigit()
        {
            var password = "א१א";
            var settings = MongoClientSettings.FromConnectionString($"mongodb://user:{Uri.EscapeDataString(password)}@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return new ScramConversationHarness(password, saslStart, longConversation: false).CreateSaslStartResponse();
                    }

                    throw new InvalidOperationException("Unexpected command sequence.");
                });

            var ex = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None));

            Assert.Equal(
                "SCRAM-SHA-256 password violates SASLprep bidirectional rules: RandALCat and LCat characters must not be mixed. code: 0",
                ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_RejectsPasswordThatViolatesSaslPrepBidirectionalRules()
        {
            var password = "abcא";
            var settings = MongoClientSettings.FromConnectionString($"mongodb://user:{Uri.EscapeDataString(password)}@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var server = new ScramConversationHarness(password, saslStart, longConversation: false);
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return server.CreateSaslStartResponse();
                    }

                    throw new InvalidOperationException("Unexpected command sequence.");
                });

            var ex = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None));

            Assert.Equal(
                "SCRAM-SHA-256 password violates SASLprep bidirectional rules: RandALCat and LCat characters must not be mixed. code: 0",
                ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_AcceptsPasswordThatContainsRandALCatAndNonLCategoryModifier()
        {
            var password = "אʹא";
            var settings = MongoClientSettings.FromConnectionString($"mongodb://user:{Uri.EscapeDataString(password)}@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var server = new ScramConversationHarness(password, saslStart, longConversation: false);
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return server.CreateSaslStartResponse();
                    }

                    return server.CreateProofResponse(command);
                });

            await authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None);

            Assert.Equal(2, connection.Requests.Count);
        }

        [Fact]
        public async Task AuthenticateAsync_RejectsPasswordThatMixesRandALCatWithHanCharacters()
        {
            var password = "א一א";
            var settings = MongoClientSettings.FromConnectionString($"mongodb://user:{Uri.EscapeDataString(password)}@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return new ScramConversationHarness(password, saslStart, longConversation: false).CreateSaslStartResponse();
                    }

                    throw new InvalidOperationException("Unexpected command sequence.");
                });

            var ex = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None));

            Assert.Equal(
                "SCRAM-SHA-256 password violates SASLprep bidirectional rules: RandALCat and LCat characters must not be mixed. code: 0",
                ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_RejectsPasswordContainingTaggingCharacters()
        {
            var password = "pass\U000E0001word";
            var settings = MongoClientSettings.FromConnectionString($"mongodb://user:{Uri.EscapeDataString(password)}@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return new ScramConversationHarness(password, saslStart, longConversation: false).CreateSaslStartResponse();
                    }

                    throw new InvalidOperationException("Unexpected command sequence.");
                });

            var ex = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None));

            Assert.Equal(
                "SCRAM-SHA-256 password contains a prohibited Unicode code point U+E0001. code: 0",
                ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_RejectsPasswordContainingNonAsciiControlFromC22()
        {
            var password = "pass\u06DDword";
            var settings = MongoClientSettings.FromConnectionString($"mongodb://user:{Uri.EscapeDataString(password)}@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return new ScramConversationHarness(password, saslStart, longConversation: false).CreateSaslStartResponse();
                    }

                    throw new InvalidOperationException("Unexpected command sequence.");
                });

            var ex = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None));

            Assert.Equal(
                "SCRAM-SHA-256 password contains a prohibited Unicode code point U+06DD. code: 0",
                ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_RejectsPasswordContainingMusicalControlFromC22()
        {
            var password = "pass\U0001D173word";
            var settings = MongoClientSettings.FromConnectionString($"mongodb://user:{Uri.EscapeDataString(password)}@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return new ScramConversationHarness(password, saslStart, longConversation: false).CreateSaslStartResponse();
                    }

                    throw new InvalidOperationException("Unexpected command sequence.");
                });

            var ex = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None));

            Assert.Equal(
                "SCRAM-SHA-256 password contains a prohibited Unicode code point U+1D173. code: 0",
                ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_RejectsPasswordContainingCodePointUnassignedInUnicode32()
        {
            var password = "pass😀word";
            var settings = MongoClientSettings.FromConnectionString($"mongodb://user:{Uri.EscapeDataString(password)}@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return new ScramConversationHarness(password, saslStart, longConversation: false).CreateSaslStartResponse();
                    }

                    throw new InvalidOperationException("Unexpected command sequence.");
                });

            var ex = await Assert.ThrowsAsync<MongoAuthentificationException>(
                () => authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None));

            Assert.Equal(
                "SCRAM-SHA-256 password contains a prohibited Unicode code point U+1F600. code: 0",
                ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_AcceptsPasswordThatSatisfiesSaslPrepBidirectionalRules()
        {
            var password = "אבא";
            var settings = MongoClientSettings.FromConnectionString($"mongodb://user:{Uri.EscapeDataString(password)}@localhost");
            var authenticator = new ScramAuthenticator(settings);
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            var server = new ScramConversationHarness(password, saslStart, longConversation: false);
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return server.CreateSaslStartResponse();
                    }

                    return server.CreateProofResponse(command);
                });

            await authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None);

            Assert.Equal(2, connection.Requests.Count);
        }

        [Fact]
        public void AuthenticateIsMaster_ThrowsWhenLoginContainsNul()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://user%00name:pass@localhost");
            var authenticator = new ScramAuthenticator(settings);

            var ex = Assert.Throws<MongoAuthentificationException>(() => authenticator.AuthenticateIsMaster(new BsonDocument()));

            Assert.Equal("SCRAM username must not contain NUL. code: 0", ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_ReusesAuthenticatorWithoutStaleKeyCacheAcrossDifferentServerFirstMessages()
        {
            var authenticator = new ScramAuthenticator(MongoClientSettings.FromConnectionString("mongodb://user:pass@localhost"));

            await AuthenticateOnceAsync(
                authenticator,
                new ScramConversationHarness(
                    "pass",
                    authenticator.AuthenticateIsMaster(new BsonDocument())!,
                    longConversation: false,
                    iterationCount: 4096,
                    saltBytes: Encoding.ASCII.GetBytes("testsalt1"),
                    conversationId: 41));

            await AuthenticateOnceAsync(
                authenticator,
                new ScramConversationHarness(
                    "pass",
                    authenticator.AuthenticateIsMaster(new BsonDocument())!,
                    longConversation: false,
                    iterationCount: 8192,
                    saltBytes: Encoding.ASCII.GetBytes("testsalt2"),
                    conversationId: 42));
        }

        private static async Task AuthenticateOnceAsync(ScramAuthenticator authenticator, ScramConversationHarness server)
        {
            var handshake = new BsonDocument();
            var saslStart = authenticator.AuthenticateIsMaster(handshake)!;
            server = server.WithSaslStart(saslStart);
            var connection = new FakeMongoConnection(
                (_, command, _) =>
                {
                    if (command.TryGet("saslStart", out _))
                    {
                        return server.CreateSaslStartResponse();
                    }

                    return server.CreateProofResponse(command);
                });

            await authenticator.AuthenticateAsync(connection, new BsonDocument(), saslStart, CancellationToken.None);

            Assert.Equal(2, connection.Requests.Count);
        }

        private static string ExtractClientNonce(string baseMessage)
        {
            const string marker = ",r=";
            var markerIndex = baseMessage.IndexOf(marker, StringComparison.Ordinal);
            return markerIndex >= 0
                ? baseMessage.Substring(markerIndex + marker.Length)
                : throw new InvalidOperationException("SCRAM base message did not contain a client nonce.");
        }

        private sealed class FakeMongoConnection : IMongoConnection
        {
            private readonly Func<string, BsonDocument, int, BsonDocument> _handler;

            public FakeMongoConnection(Func<string, BsonDocument, int, BsonDocument> handler)
            {
                _handler = handler;
            }

            public List<(string Database, BsonDocument Command)> Requests { get; } = new();

            public ValueTask DisposeAsync() => ValueTask.CompletedTask;

            public ValueTask<QueryResult<TResp>> SendQueryAsync<TResp>(string database, BsonDocument document, CancellationToken cancellationToken) where TResp : MongoDB.Client.Bson.Serialization.IBsonSerializer<TResp>
            {
                if (typeof(TResp) != typeof(BsonDocument))
                {
                    throw new NotSupportedException("FakeMongoConnection only supports BsonDocument responses.");
                }

                Requests.Add((database, document));
                var response = _handler(database, document, Requests.Count - 1);
                var queryResult = new QueryResult<BsonDocument>(0);
                queryResult.Add(response);
                return new ValueTask<QueryResult<TResp>>((QueryResult<TResp>)(object)queryResult);
            }
        }

        private sealed class ScramConversationHarness
        {
            private readonly string _password;
            private readonly SaslStart _saslStart;
            private readonly byte[] _saltBytes;
            private readonly string _saltBase64;
            private readonly string _serverFirstMessage;
            private readonly bool _longConversation;
            private readonly int _iterationCount;
            private readonly int _conversationId;

            public ScramConversationHarness(
                string password,
                SaslStart saslStart,
                bool longConversation,
                int iterationCount = 4096,
                byte[]? saltBytes = null,
                int conversationId = 41)
            {
                _password = password;
                _saslStart = saslStart;
                _longConversation = longConversation;
                _iterationCount = iterationCount;
                _conversationId = conversationId;
                _saltBytes = saltBytes ?? Encoding.ASCII.GetBytes("testsalt");
                _saltBase64 = Convert.ToBase64String(_saltBytes);
                var clientNonce = ExtractClientNonce(saslStart.BaseMessage);
                _serverFirstMessage = $"r={clientNonce}server,s={_saltBase64},i={iterationCount}";
            }

            public int ConversationId => _conversationId;

            public ScramConversationHarness WithSaslStart(SaslStart saslStart)
            {
                return new ScramConversationHarness(_password, saslStart, _longConversation, _iterationCount, _saltBytes, _conversationId);
            }

            public BsonDocument CreateSaslStartResponse()
            {
                return new BsonDocument
                {
                    { "ok", 1 },
                    { "conversationId", ConversationId },
                    { "done", false },
                    { "payload", BsonBinaryData.Create(Strict.GetBytes(_serverFirstMessage)) }
                };
            }

            public BsonDocument CreateProofResponse(BsonDocument command, string? finalPayloadOverride = null)
            {
                _finalPayload = finalPayloadOverride ?? CreateServerFinalMessage(command);

                if (_longConversation)
                {
                    return new BsonDocument
                    {
                        { "ok", 1 },
                        { "conversationId", ConversationId },
                        { "done", false },
                        { "payload", BsonBinaryData.Create(Array.Empty<byte>()) }
                    };
                }

                return new BsonDocument
                {
                    { "ok", 1 },
                    { "conversationId", ConversationId },
                    { "done", true },
                    { "payload", BsonBinaryData.Create(Strict.GetBytes(_finalPayload)) }
                };
            }

            public BsonDocument CreateFinalResponse()
            {
                return new BsonDocument
                {
                    { "ok", 1 },
                    { "conversationId", ConversationId },
                    { "done", true },
                    { "payload", BsonBinaryData.Create(Strict.GetBytes(_finalPayload ?? throw new InvalidOperationException("Proof response must be created first."))) }
                };
            }

            private string? _finalPayload;

            private string CreateServerFinalMessage(BsonDocument command)
            {
                var clientFinalMessage = Strict.GetString(command["payload"].AsByteArray!);
                var proofSeparator = clientFinalMessage.LastIndexOf(",p=", StringComparison.Ordinal);
                var clientFinalMessageWithoutProof = proofSeparator >= 0
                    ? clientFinalMessage.Substring(0, proofSeparator)
                    : clientFinalMessage;
                var authMessage = _saslStart.BaseMessage + "," + _serverFirstMessage + "," + clientFinalMessageWithoutProof;
                var saltedPassword = Rfc2898DeriveBytes.Pbkdf2(_password, _saltBytes, _iterationCount, HashAlgorithmName.SHA256, 32);
                using var serverKeyHmac = new HMACSHA256(saltedPassword);
                var serverKey = serverKeyHmac.ComputeHash(Strict.GetBytes("Server Key"));
                using var signatureHmac = new HMACSHA256(serverKey);
                var signature = signatureHmac.ComputeHash(Strict.GetBytes(authMessage));
                _finalPayload = "v=" + Convert.ToBase64String(signature);
                return _finalPayload;
            }

            private static string ExtractClientNonce(string baseMessage)
            {
                const string marker = ",r=";
                var markerIndex = baseMessage.IndexOf(marker, StringComparison.Ordinal);
                return markerIndex >= 0
                    ? baseMessage.Substring(markerIndex + marker.Length)
                    : throw new InvalidOperationException("SCRAM base message did not contain a client nonce.");
            }
        }
    }
}
