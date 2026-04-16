using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Connection
{
    internal enum MongoConnectionInitializerPhase
    {
        BeforeHello,
        AfterHello,
        BeforeBuildInfo,
        AfterBuildInfo
    }

    internal delegate ValueTask MongoConnectionInitializerStep(
        IMongoConnection connection,
        MongoConnectionInitializerContext context,
        CancellationToken cancellationToken);

    internal sealed class MongoConnectionInitializerContext
    {
        private Dictionary<object, object?>? _state;

        public MongoConnectionInitializerContext(MongoClientSettings settings, BsonDocument handshakeCommand)
        {
            Settings = settings;
            HandshakeCommand = handshakeCommand;
        }

        public MongoClientSettings Settings { get; }

        public BsonDocument HandshakeCommand { get; }

        public BsonDocument? HelloResult { get; set; }

        public BsonDocument? BuildInfoResult { get; set; }

        public void SetState(object key, object? value)
        {
            ArgumentNullException.ThrowIfNull(key);
            (_state ??= new Dictionary<object, object?>())[key] = value;
        }

        public bool TryGetState<T>(object key, out T? value)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (_state is not null
                && _state.TryGetValue(key, out var storedValue)
                && storedValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }
    }

    internal sealed class MongoConnectionInitializerPipelineBuilder
    {
        private readonly List<PipelineStep> _steps = new();

        public void Add(MongoConnectionInitializerPhase phase, MongoConnectionInitializerStep step)
        {
            ArgumentNullException.ThrowIfNull(step);
            _steps.Add(new PipelineStep(phase, step));
        }

        internal IReadOnlyList<PipelineStep> Build() => _steps;

        internal readonly record struct PipelineStep(
            MongoConnectionInitializerPhase Phase,
            MongoConnectionInitializerStep Step);
    }

    internal interface IMongoConnectionInitializerPlugin
    {
        void Configure(MongoConnectionInitializerPipelineBuilder builder);
    }
}
