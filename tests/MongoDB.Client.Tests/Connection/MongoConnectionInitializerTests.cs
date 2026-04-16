using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using MongoDB.Client.Settings;
using Xunit;

namespace MongoDB.Client.Tests.Connection
{
    public class MongoConnectionInitializerTests
    {
        [Fact]
        public async Task InitializeAsync_RunsPluginHandshakeFlowAndReturnsConnectionInfo()
        {
            var settings = new MongoClientSettings();
            var plugin = new RecordingInitializerPlugin();
            var initializer = new MongoConnectionInitializer(settings, plugin);
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
            var initializer = new MongoConnectionInitializer(settings, plugin);
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
            var initializer = new MongoConnectionInitializer(settings, plugin);
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
            }

            public List<BsonDocument> Requests { get; } = new();

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
