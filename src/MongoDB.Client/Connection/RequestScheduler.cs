using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Messages;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{

    internal class RequestScheduler
    {
        private int MaxConnections => 32;
        private readonly MongoConnectionFactory _connectionFactory;
        private readonly List<MongoConnection> _connections;
        private readonly Channel<MongoReuqestBase> _channel;
        private readonly ChannelWriter<MongoReuqestBase> _channelWriter;
        private static int _counter;
        private readonly ConcurrentQueue<ManualResetValueTaskSource<IParserResult>> _queue = new();
        public RequestScheduler(MongoConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            var options = new UnboundedChannelOptions();
            options.SingleWriter = true;
            options.SingleReader = false;
            options.AllowSynchronousContinuations = true;
            _channel = Channel.CreateUnbounded<MongoReuqestBase>(options);
            _channelWriter = _channel.Writer;
            _connections = new List<MongoConnection>();
            _counter = 0;
        }
        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _counter);
        }
        internal async ValueTask Init()
        {
            if (_connections.Count == 0)
            {
                for (int i = 0; i < MaxConnections; i++)
                {
                    _connections.Add(await CreateNewConnection());
                }
            }
        }
        private ValueTask<MongoConnection> CreateNewConnection()
        {
            return _connectionFactory.Create(_channel.Reader);
        }
        internal async ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token = default)
        {
            if (_connections.Count == 0)
            {
                await Init();
            }
            ManualResetValueTaskSource<IParserResult> taskSource;
            if (_queue.TryDequeue(out taskSource) == false)
            {
                taskSource = new ManualResetValueTaskSource<IParserResult>();
            }
            var request = new FindMongoRequest(message, taskSource);
            request.ParseAsync = CursorParserCallbackHolder<T>.CursorParseAsync;
            await _channelWriter.WriteAsync(request);
            var cursor = await taskSource.GetValueTask() as CursorResult<T>;
            taskSource.Reset();
            _queue.Enqueue(taskSource);
            return cursor;
        }

    }
}
