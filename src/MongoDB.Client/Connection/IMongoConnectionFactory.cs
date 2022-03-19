using System.Threading.Channels;
﻿using MongoDB.Client.Authentication;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Connection
{
    internal interface IMongoConnectionFactory
    {
        ValueTask<MongoConnection> CreateAsync(MongoClientSettings settings, ScramAuthenticator authenticator, ChannelReader<MongoRequest> reader, MongoScheduler requestScheduler, CancellationToken token);
    }
}
