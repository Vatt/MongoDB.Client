using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDb.Client.WebApi
{
    public class MongoStarter : IHostedService
    {
        public readonly IMongo _mongo;

        public MongoStarter(IMongo mongo)
        {
            _mongo = mongo;
        }

        public MongoStarter()
        {
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _mongo.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }
}
