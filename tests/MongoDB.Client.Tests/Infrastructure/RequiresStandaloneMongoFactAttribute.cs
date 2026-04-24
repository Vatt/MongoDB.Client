using Xunit;

namespace MongoDB.Client.Tests.Infrastructure
{
    internal sealed class RequiresStandaloneMongoFactAttribute : FactAttribute
    {
        public RequiresStandaloneMongoFactAttribute()
        {
            Skip = IntegrationMongoTopologyRequirements.GetStandaloneSkipReason();
        }
    }
}
