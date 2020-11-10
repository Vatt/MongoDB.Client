using MongoDB.Client.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace MongoDB.Client.Connection
{
    public class ConnectionScheduler
    {
        private int MaxConnections => 64;
        private readonly List<MongoConnection> _connections;
        private readonly Channel<ManualResetValueTaskSource<IParserResult>> _channel;
        private readonly ChannelWriter<ManualResetValueTaskSource<IParserResult>> _writer;
        public ConnectionScheduler()
        {            
            var options = new UnboundedChannelOptions();
            options.SingleWriter = true;
            options.SingleReader = false;
            options.AllowSynchronousContinuations = true;
            _channel = System.Threading.Channels.Channel.CreateUnbounded<ManualResetValueTaskSource<IParserResult>>(options);
            _writer = _channel.Writer;
            _connections = new List<MongoConnection>();
        }
    }
}
