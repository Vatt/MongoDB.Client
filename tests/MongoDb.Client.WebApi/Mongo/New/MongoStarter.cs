namespace MongoDb.Client.WebApi
{
    public class MongoStarter : IHostedService
    {
        public readonly INewMongo _mongo;

        public MongoStarter(INewMongo mongo)
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
