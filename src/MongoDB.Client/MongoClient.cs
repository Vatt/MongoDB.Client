using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Experimental;
using MongoDB.Client.Messages;
using MongoDB.Client.Network;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;

namespace MongoDB.Client
{
    public class MongoClient
    {
        public MongoClientSettings Settings { get; }
        private readonly IMongoScheduler _scheduler;


        private MongoClient(MongoClientSettings settings, IMongoScheduler sceduler)
        {
            Settings = settings;
            _scheduler = sceduler;
        }
        public MongoDatabase GetDatabase(string name)
        {
            return new MongoDatabase(this, name, _scheduler);
        }


        private ValueTask InitAsync(CancellationToken token)
        {
            return _scheduler.StartAsync(token);
        }


        public TransactionHandler StartTransaction()
        {
            return TransactionHandler.Create(_scheduler);
        }

        public static Task<MongoClient> CreateClient(string connectionString, ILoggerFactory? loggerFactory = null, CancellationToken token = default)
        {
            return CreateClient(MongoClientSettings.FromConnectionString(connectionString), loggerFactory, token);
        }

        public static Task<MongoClient> CreateClient(EndPoint endPoint, ILoggerFactory? loggerFactory = null, CancellationToken token = default)
        {
            loggerFactory ??= new NullLoggerFactory();
            var settings = new MongoClientSettings(endPoint);
            return CreateClient(settings, loggerFactory, token);
        }
        public static async Task<MongoClient> CreateClient(MongoClientSettings settings, ILoggerFactory? loggerFactory = null, CancellationToken token = default)
        {
            loggerFactory ??= new NullLoggerFactory();
            var connectionFactory = new NetworkConnectionFactory(loggerFactory);
            IMongoScheduler? scheduler = null;
            MongoPingMessage? ping = null;
            foreach (var endpoint in settings.Endpoints)
            {

                try
                {
                    var ctx = await connectionFactory.ConnectAsync(endpoint, token).ConfigureAwait(false);
                    await using var serviceConnection = new MongoServiceConnection(ctx);
                    await serviceConnection.Connect(settings, token).ConfigureAwait(false);
                    ping = await serviceConnection.MongoPing(token).ConfigureAwait(false);
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (ping is null)
            {
                ThrowHelper.MongoInitExceptions<MongoClient>();
            }
            //Sharded cluster
            if (ping.Hosts is null && ping.SetName is null && ping.Primary is null && ping.Message is not null)
            {
                if (ping.Message.Equals("isdbgrid") == false)
                {
                    ThrowHelper.MongoInitExceptions<MongoClient>();
                }
                scheduler = new ShardedScheduler(settings, loggerFactory);
            }
            else if (ping.Hosts is not null && ping.SetName is not null && ping.Message is null)  //Replica set
            {
                scheduler = new ReplicaSetScheduler(settings, loggerFactory);
            }
            else //Standalone
            {
                IMongoConnectionFactory factory = settings.ClientType switch
                {
                    ClientType.Default => new MongoConnectionFactory(settings.Endpoints[0], loggerFactory),
                    ClientType.Experimental => new ExperimentalMongoConnectionFactory(settings.Endpoints[0], loggerFactory),
                    _ => throw new MongoBadClientTypeException()
                };
                scheduler = new StandaloneScheduler(settings, factory, loggerFactory);
            }

            if (scheduler is null)
            {
                ThrowHelper.MongoInitExceptions<MongoClient>();
            }
            var client = new MongoClient(settings, scheduler);
            await client.InitAsync(token).ConfigureAwait(false);
            return client;
        }
    }
}
