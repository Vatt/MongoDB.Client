using Xunit;

namespace MongoDB.Client.Tests.Infrastructure
{
    internal sealed class RequiresReplicaSetMongoFactAttribute : FactAttribute
    {
        public RequiresReplicaSetMongoFactAttribute()
        {
            Skip = IntegrationMongoTopologyRequirements.GetReplicaSetSkipReason();
        }
    }
}
