using MongoDB.Client.Authentication;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Settings;
using Xunit;

namespace MongoDB.Client.Tests.Authentication
{
    public class ScramAuthenticatorTests
    {
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
    }
}
