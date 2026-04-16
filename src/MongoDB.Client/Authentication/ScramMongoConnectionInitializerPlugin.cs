using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;

namespace MongoDB.Client.Authentication
{
    internal sealed class ScramMongoConnectionInitializerPlugin : IMongoConnectionInitializerPlugin
    {
        private static readonly object SaslStartStateKey = new();
        private readonly ScramAuthenticator _authenticator;

        public ScramMongoConnectionInitializerPlugin(ScramAuthenticator authenticator)
        {
            _authenticator = authenticator;
        }

        public void Configure(MongoConnectionInitializerPipelineBuilder builder)
        {
            builder.Add(
                MongoConnectionInitializerPhase.BeforeHello,
                (connection, context, cancellationToken) => OnBeforeHelloAsync(context));

            builder.Add(
                MongoConnectionInitializerPhase.AfterHello,
                OnAfterHelloAsync);
        }

        private ValueTask OnBeforeHelloAsync(MongoConnectionInitializerContext context)
        {
            context.SetState(SaslStartStateKey, _authenticator.AuthenticateIsMaster(context.HandshakeCommand));
            return ValueTask.CompletedTask;
        }

        private ValueTask OnAfterHelloAsync(IMongoConnection connection, MongoConnectionInitializerContext context, CancellationToken cancellationToken)
        {
            context.TryGetState<SaslStart>(SaslStartStateKey, out var saslStart);
            return new ValueTask(_authenticator.AuthenticateAsync(connection, context.HelloResult!, saslStart, cancellationToken));
        }
    }
}
