using System.Net.Sockets;
using System.Reflection;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Tests.Infrastructure;
using MongoDB.Client.Tests.Models;
using Xunit;

namespace MongoDB.Client.Tests.Authentication
{
    public abstract class AuthenticationIntegrationTestBase
    {
        protected static async Task AssertRoundTripAsync(string connectionString, string topologyName)
        {
            var client = await MongoClient.CreateClient(connectionString);
            var databaseName = CreateDatabaseName(topologyName);
            var collectionName = CreateCollectionName(topologyName);
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<CommonModel>(collectionName);
            var model = CommonModel.Create();

            try
            {
                await collection.InsertAsync(model);

                var result = await collection.Find(BsonDocument.Empty).SingleOrDefaultAsync();

                Assert.NotNull(result);
                Assert.Equal(model, result);
            }
            finally
            {
                await database.DropCollectionAsync(collectionName);
            }
        }

        protected static async Task AssertRoundTripWithMultipleOperationsAsync(string connectionString, string topologyName)
        {
            var client = await MongoClient.CreateClient(connectionString);
            var primaryConnections = GetReplicaSetPrimaryConnections(client);

            Assert.True(
                primaryConnections.Count > 1,
                $"Expected replica set primary scheduler to initialize more than one pooled connection, but got {primaryConnections.Count}.");

            Assert.All(primaryConnections, connection =>
            {
                Assert.NotNull(connection.ConnectionInfo);
                Assert.NotNull(connection.ConnectionInfo!.IsMaster);
                Assert.NotNull(connection.ConnectionInfo.BuildInfo);
            });

            var databaseName = CreateDatabaseName(topologyName);
            var collectionName = CreateCollectionName(topologyName);
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<CommonModel>(collectionName);
            var models = Enumerable.Range(0, 3)
                .Select(index =>
                {
                    var model = CommonModel.Create();
                    model.IntProp = index;
                    model.StringField = $"{topologyName}_{index}";
                    return model;
                })
                .ToArray();

            try
            {
                await collection.InsertAsync(models);

                var firstRead = await collection.Find(BsonDocument.Empty).ToListAsync();
                Assert.Equal(models.Length, firstRead.Count);
                AssertAllModelsPresent(models, firstRead);

                var extraModel = CommonModel.Create();
                extraModel.IntProp = 99;
                extraModel.StringField = $"{topologyName}_extra";
                await collection.InsertAsync(extraModel);

                var secondRead = await collection.Find(BsonDocument.Empty).ToListAsync();
                Assert.Equal(models.Length + 1, secondRead.Count);
                AssertAllModelsPresent(models.Append(extraModel), secondRead);
            }
            finally
            {
                await database.DropCollectionAsync(collectionName);
            }
        }

        protected static async Task AssertExplicitAuthenticationFailureAsync(string connectionString)
        {
            var exception = await CaptureConnectionExceptionAsync(connectionString);
            var exceptionText = FlattenExceptionMessages(exception);
            var outerException = Assert.IsType<MongoException>(exception);

            Assert.StartsWith("Connection failed", outerException.Message, StringComparison.OrdinalIgnoreCase);
            Assert.NotNull(outerException.InnerException);
            Assert.False(
                ContainsConnectivityFailure(outerException.InnerException!),
                $"Expected preserved authentication failure, but got connectivity failure: {exceptionText}");

            var authCause = FindAuthenticationFailure(outerException.InnerException!);
            Assert.NotNull(authCause);
            Assert.Same(authCause, outerException.InnerException);
        }

        private static async Task<Exception> CaptureConnectionExceptionAsync(string connectionString)
        {
            var exception = await Record.ExceptionAsync(async () =>
            {
                await MongoClient.CreateClient(connectionString).ConfigureAwait(false);
            });

            return Assert.IsAssignableFrom<Exception>(exception);
        }

        private static string CreateDatabaseName(string topologyName)
        {
            return $"AuthLive_{topologyName}_{Guid.NewGuid():N}";
        }

        private static string CreateCollectionName(string topologyName)
        {
            return $"AuthCollection_{topologyName}_{Guid.NewGuid():N}";
        }

        private static void AssertAllModelsPresent(IEnumerable<CommonModel> expected, IReadOnlyCollection<CommonModel> actual)
        {
            foreach (var model in expected)
            {
                Assert.Contains(actual, candidate => candidate.Equals(model));
            }
        }

        private static Exception? FindAuthenticationFailure(Exception exception)
        {
            foreach (var current in EnumerateExceptions(exception))
            {
                if (current is MongoAuthentificationException)
                {
                    return current;
                }

                if (current is MongoCommandException commandException && commandException.Code == 18)
                {
                    return current;
                }

                if (ContainsAuthenticationMarker(current.Message))
                {
                    return current;
                }
            }

            return null;
        }

        private static bool ContainsConnectivityFailure(Exception exception)
        {
            foreach (var current in EnumerateExceptions(exception))
            {
                if (current is SocketException or TimeoutException or OperationCanceledException)
                {
                    return true;
                }

                if (current is MongoException mongoException &&
                    mongoException.Message.StartsWith("Connection failed", StringComparison.OrdinalIgnoreCase) &&
                    mongoException.InnerException is null)
                {
                    return true;
                }
            }

            return false;
        }

        private static string FlattenExceptionMessages(Exception exception)
        {
            return string.Join(" --> ", EnumerateExceptions(exception).Select(static ex => ex.GetType().Name + ": " + ex.Message));
        }

        private static IEnumerable<Exception> EnumerateExceptions(Exception exception)
        {
            for (var current = exception; current is not null; current = current.InnerException)
            {
                yield return current;
            }
        }

        private static bool ContainsAuthenticationMarker(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            return message.Contains("auth", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("authentication", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("sasl", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("SCRAM", StringComparison.OrdinalIgnoreCase);
        }

        private static IReadOnlyList<MongoConnection> GetReplicaSetPrimaryConnections(MongoClient client)
        {
            var scheduler = GetPrivateField<IMongoScheduler>(client, "_scheduler");
            var replicaSetScheduler = Assert.IsType<ReplicaSetScheduler>(scheduler);
            var primaryScheduler = GetPrivateField<MongoScheduler>(replicaSetScheduler, "_primary");
            var connections = GetPrivateField<List<MongoConnection>>(primaryScheduler, "_connections");

            Assert.NotEmpty(connections);
            return connections;
        }

        private static TField GetPrivateField<TField>(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);

            var value = field!.GetValue(instance);
            return Assert.IsAssignableFrom<TField>(value);
        }
    }
}
