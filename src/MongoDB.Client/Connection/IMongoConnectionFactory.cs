﻿using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Connection
{
    internal interface IMongoConnectionFactory
    {
        ValueTask<MongoConnection> CreateAsync(MongoClientSettings settings, ChannelReader<MongoRequest> reader, ChannelReader<MongoRequest> findReader, MongoScheduler requestScheduler, CancellationToken token);
    }
}
