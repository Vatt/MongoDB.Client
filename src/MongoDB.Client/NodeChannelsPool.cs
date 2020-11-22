using System;
using System.Collections.Immutable;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.MongoConnections;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client
{
    internal class NodeChannelsPool : IChannelsPool
    {
        private static readonly Random Random = new();
        
        private readonly MongoClientSettings _settings;
        private readonly EndPoint _endPoint;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<NodeChannelsPool> _logger;
        private ImmutableArray<Channel> _channels = ImmutableArray<Channel>.Empty;
        private readonly SemaphoreSlim _channelAllocateLock = new(1);
        private int _channelNumber;
        private int _channelCounter;

        public NodeChannelsPool(MongoClientSettings settings, EndPoint endPoint, ILoggerFactory loggerFactory)
        {
            _settings = settings;
            _endPoint = endPoint;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<NodeChannelsPool>();
            _initialDocument = InitHelper.CreateInitialCommand();
        }

        public ValueTask<Channel> GetChannelAsync(CancellationToken cancellationToken)
        {
            var idx = Interlocked.Increment(ref _channelCounter);
            var channels = _channels;
            
            for (int i = 0; i < channels.Length; i++)
            {
                var current = (idx + i) % channels.Length;
                var channel = channels[current];
                if (channel.RequestsInProgress < _settings.MultiplexingTreshold)
                {
                    return new ValueTask<Channel>(channel);
                }
            }

            if (channels.Length == _settings.ConnectionPoolMaxSize)
            {
                idx = Random.Next(_settings.ConnectionPoolMaxSize);
                return new ValueTask<Channel>(channels[idx]);
            }
            return AllocateNewChannel(cancellationToken);
        }

        private async ValueTask<Channel> AllocateNewChannel(CancellationToken cancellationToken)
        {
            await _channelAllocateLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                Channel channel;
                var channels = _channels;
                for (int i = 0; i < channels.Length; i++)
                {
                    channel = channels[i];
                    if (channel.RequestsInProgress < _settings.MultiplexingTreshold)
                    {
                        return channel;
                    }
                }
                
                if (channels.Length == _settings.ConnectionPoolMaxSize)
                {
                    var idx = Random.Next(_settings.ConnectionPoolMaxSize);
                    return channels[idx];
                }

                channel = await CreateChannelAsync(cancellationToken);
                
                
                _channels = channels.Add(channel);
                return channel;
            }
            finally
            {
                _channelAllocateLock.Release();
            }
        }

        private async Task<Channel> CreateChannelAsync(CancellationToken token)
        {
            _logger.LogInformation("Allocating new channel");
            var channelNum = Interlocked.Increment(ref _channelNumber);
            var channel = new Channel(_loggerFactory, channelNum);
            await channel.ConnectAsync(_endPoint, token).ConfigureAwait(false);
            var result = await OpenChannelAsync(channel,token);
            return channel;
        }
        
        
        private readonly BsonDocument _initialDocument;
        private async Task<ConnectionInfo> OpenChannelAsync(Channel channel, CancellationToken token)
        {
            var connectRequest = CreateQueryRequest(_initialDocument, channel.GetNextRequestNumber());
            var configMessage = await channel.SendQueryAsync<BsonDocument>(connectRequest, token).ConfigureAwait(false);
            var buildInfoRequest = CreateQueryRequest(new BsonDocument("buildInfo", 1), channel.GetNextRequestNumber());
            var hell = await channel.SendQueryAsync<BsonDocument>(buildInfoRequest, token).ConfigureAwait(false);
            return new ConnectionInfo(configMessage[0], hell[0]);
        }
        
        private QueryMessage CreateQueryRequest(BsonDocument document, int number)
        {
            var doc = CreateWrapperDocument(document);
            return CreateQueryRequest("admin.$cmd", doc, number);
        }

        private QueryMessage CreateQueryRequest(string database, BsonDocument document, int number)
        {
            return new QueryMessage(number, database, document);
        }

        private static BsonDocument CreateWrapperDocument(BsonDocument document)
        {
            BsonDocument? readPreferenceDocument = null;
            var doc = new BsonDocument
            {
                {"$query", document},
                {"$readPreference", readPreferenceDocument, readPreferenceDocument != null}
            };

            if (doc.Count == 1)
            {
                return doc["$query"].AsBsonDocument;
            }
            else
            {
                return doc;
            }
        }

        
    }
}