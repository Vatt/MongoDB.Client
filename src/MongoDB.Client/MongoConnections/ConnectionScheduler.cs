using System.Collections.Generic;
using System.Threading.Channels;
using MongoDB.Client.Messages;

namespace MongoDB.Client.MongoConnections
{
    internal class MongoReuqest 
    {
        internal ManualResetValueTaskSource<IParserResult> Complection { get; }
        internal MongoReuqest(ManualResetValueTaskSource<IParserResult> complection)
        {
            Complection = complection;
        }
    }
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
