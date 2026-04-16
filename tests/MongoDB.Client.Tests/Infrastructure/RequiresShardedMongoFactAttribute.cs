using Xunit;

namespace MongoDB.Client.Tests.Infrastructure
{
    internal sealed class RequiresShardedMongoFactAttribute : FactAttribute
    {
        public RequiresShardedMongoFactAttribute()
        {
            Skip = IntegrationMongoTopologyRequirements.GetShardedSkipReason();
        }
    }
}
