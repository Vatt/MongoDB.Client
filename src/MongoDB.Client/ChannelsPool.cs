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
    internal class ChannelsPool : IChannelsPool
    {
        private static readonly int _maxChannels = 64;//Environment.ProcessorCount * 6;
        private const int Trashhold = 2; 
        private static readonly Random Random = new();
        
        private readonly EndPoint _endPoint;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ChannelsPool> _logger;
        private ImmutableList<Channel> _channels = ImmutableList<Channel>.Empty;
        private readonly SemaphoreSlim _channelAllocateLock = new(1);
        private int _channelNumber;
        private int _channelCounter;

        public ChannelsPool(EndPoint endPoint, ILoggerFactory loggerFactory)
        {
            _endPoint = endPoint;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ChannelsPool>();
            _initialDocument = InitHelper.CreateInitialCommand();
        }

        public ValueTask<Channel> GetChannelAsync(CancellationToken cancellationToken)
        {
            var idx = Interlocked.Increment(ref _channelCounter);

            for (int i = 0; i < _channels.Count; i++)
            {
                var current = (idx + i) % _channels.Count;
                var channel = _channels[current];
                if (channel.RequestsInProgress < Trashhold)
                {
                    return new ValueTask<Channel>(channel);
                }
            }

            if (_channels.Count == _maxChannels)
            {
                idx = Random.Next(_maxChannels);
                return new ValueTask<Channel>(_channels[idx]);
            }
            return AllocateNewChannel(cancellationToken);
        }

        private async ValueTask<Channel> AllocateNewChannel(CancellationToken cancellationToken)
        {
            await _channelAllocateLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                Channel channel;
                for (int i = 0; i < _channels.Count; i++)
                {
                    channel = _channels[i];
                    if (channel.RequestsInProgress < Trashhold)
                    {
                        return channel;
                    }
                }
                
                if (_channels.Count == _maxChannels)
                {
                    var idx = Random.Next(_maxChannels);
                    return _channels[idx];
                }

                channel = await CreateChannelAsync(cancellationToken);
                
                
                _channels = _channels.Add(channel);
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
            var channel = new Channel(_endPoint, _loggerFactory, channelNum);
            await channel.ConnectAsync(token).ConfigureAwait(false);
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