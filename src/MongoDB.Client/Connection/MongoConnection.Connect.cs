using MongoDB.Client.Bson.Document;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection
    {
        private ConnectionInfo? _connectionInfo;
        internal async ValueTask StartAsync(System.Net.Connections.Connection connection, CancellationToken cancellationToken = default)
        {
            _connection = connection;
            _protocolReader = _connection.CreateReader();
            _protocolWriter = _connection.CreateWriter();
            _listenerTask = StartProtocolListenerAsync();
            _connectionInfo = await DoConnectAsync(cancellationToken).ConfigureAwait(false);
            QueryMessage CreateConnectRequest(string database, BsonDocument document)
            {
                var num = GetNextRequestNumber();
                return new QueryMessage(num, database, document);
            }
            async Task<ConnectionInfo> DoConnectAsync(CancellationToken ct)
            {
                //var initialDoc = InitHelper.CreateInitialCommand();
                //QueryMessage? connectRequest = CreateConnectRequest("admin.$cmd", initialDoc);
                //var configMessage = await SendQueryAsync<BsonDocument>(connectRequest, ct).ConfigureAwait(false);
                //QueryMessage? buildInfoRequest = CreateQueryRequest(new BsonDocument("buildInfo", 1));
                //var hell = await SendQueryAsync<BsonDocument>(buildInfoRequest, ct).ConfigureAwait(false);
                //_connectionInfo = new ConnectionInfo(configMessage[0], hell[0]);
                //Init = true;
                //return _connectionInfo;
                return default;
            }
        }
    }
}