using System;
using System.Collections.Immutable;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MongoDB.Client
{
    internal class ChannelsPool2 : IChannelsPool
    {
        private static readonly int _maxChannels = Environment.ProcessorCount * 2;
        private const int Trashhold = 5; 
        private static readonly Random Random = new Random();
        
        private readonly EndPoint _endPoint;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ChannelsPool2> _logger;
        private ImmutableList<Channel> _channels = ImmutableList<Channel>.Empty;
        private readonly SemaphoreSlim _channelAllocateLock = new SemaphoreSlim(1);
        private int _channelNumber;
        private int _channelCounter;

        public ChannelsPool2(EndPoint endPoint, ILoggerFactory loggerFactory)
        {
            _endPoint = endPoint;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ChannelsPool2>();
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
                
                _logger.LogInformation("Allocating new channel");
                var channelNum = Interlocked.Increment(ref _channelNumber);
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