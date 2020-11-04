using System;
using System.Collections.Immutable;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MongoDB.Client
{
    internal class ChannelsPool
    {
        public int MaxChannels = Environment.ProcessorCount;
        private static readonly Random Random = new Random();
        
        private readonly EndPoint _endPoint;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ChannelsPool> _logger;
        private ImmutableList<Channel> _channels = ImmutableList<Channel>.Empty;
        private readonly SemaphoreSlim _channelAllocateLock = new SemaphoreSlim(1);
        private static int _channelCounter;

        public ChannelsPool(EndPoint endPoint, ILoggerFactory loggerFactory)
        {
            _endPoint = endPoint;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ChannelsPool>();
        }

        public ValueTask<Channel> GetChannelAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < _channels.Count; i++)
            {
                var channel = _channels[i];
                if (channel.IsBusy == false)
                {
                    return new ValueTask<Channel>(channel);
                }
            }

            if (_channels.Count == MaxChannels)
            {
                var idx = Random.Next(MaxChannels);
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
                    if (channel.IsBusy == false)
                    {
                        return channel;
                    }
                }
                
                if (_channels.Count == MaxChannels)
                {
                    var idx = Random.Next(MaxChannels);
                    return _channels[idx];
                }
                
                _logger.LogInformation("Allocating new channel");
                var channelNum = Interlocked.Increment(ref _channelCounter);
                channel = new Channel(_endPoint, _loggerFactory, channelNum);
                _ = await channel.InitConnectAsync(cancellationToken).ConfigureAwait(false);
                _channels = _channels.Add(channel);
                return channel;
            }
            finally
            {
                _channelAllocateLock.Release();
            }
        }
    }
}