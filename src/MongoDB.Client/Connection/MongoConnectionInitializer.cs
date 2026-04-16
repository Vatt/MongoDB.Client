using MongoDB.Client.Bson.Document;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Connection
{
    internal sealed class MongoConnectionInitializer : IMongoConnectionInitializer
    {
        private const string AdminDatabase = "admin.$cmd";
        private static readonly BsonDocument BuildInfoCommand = new("buildInfo", 1);

        private readonly MongoClientSettings _settings;
        private readonly IReadOnlyList<MongoConnectionInitializerPipelineBuilder.PipelineStep> _steps;

        public MongoConnectionInitializer(MongoClientSettings settings, params IMongoConnectionInitializerPlugin[] plugins)
        {
            _settings = settings;
            var builder = new MongoConnectionInitializerPipelineBuilder();
            for (var i = 0; i < plugins.Length; i++)
            {
                plugins[i].Configure(builder);
            }

            _steps = builder.Build();
        }

        public async ValueTask<ConnectionInfo> InitializeAsync(IMongoConnection connection, CancellationToken cancellationToken)
        {
            var context = new MongoConnectionInitializerContext(_settings, InitHelper.CreateInitialCommand(_settings));

            await ExecutePhaseAsync(MongoConnectionInitializerPhase.BeforeHello, connection, context, cancellationToken).ConfigureAwait(false);

            var handshakeResult = await connection.SendQueryAsync<BsonDocument>(AdminDatabase, context.HandshakeCommand, cancellationToken).ConfigureAwait(false);
            context.HelloResult = handshakeResult[0];

            await ExecutePhaseAsync(MongoConnectionInitializerPhase.AfterHello, connection, context, cancellationToken).ConfigureAwait(false);
            await ExecutePhaseAsync(MongoConnectionInitializerPhase.BeforeBuildInfo, connection, context, cancellationToken).ConfigureAwait(false);

            var buildInfoResult = await connection.SendQueryAsync<BsonDocument>(AdminDatabase, BuildInfoCommand, cancellationToken).ConfigureAwait(false);
            context.BuildInfoResult = buildInfoResult[0];

            await ExecutePhaseAsync(MongoConnectionInitializerPhase.AfterBuildInfo, connection, context, cancellationToken).ConfigureAwait(false);

            return new ConnectionInfo(context.HelloResult, context.BuildInfoResult);
        }

        private async ValueTask ExecutePhaseAsync(
            MongoConnectionInitializerPhase phase,
            IMongoConnection connection,
            MongoConnectionInitializerContext context,
            CancellationToken cancellationToken)
        {
            for (var i = 0; i < _steps.Count; i++)
            {
                var step = _steps[i];
                if (step.Phase != phase)
                {
                    continue;
                }

                await step.Step(connection, context, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
