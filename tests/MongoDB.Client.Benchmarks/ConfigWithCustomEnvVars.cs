using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace MongoDB.Client.Benchmarks
{
    public class ConfigWithCustomEnvVars : ManualConfig
    {
        public ConfigWithCustomEnvVars()
        {
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core70).WithGcServer(true));
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core80).WithGcServer(true));
            //AddJob(Job.Default.WithRuntime(CoreRuntime.Core70)
            //    .WithEnvironmentVariables(new EnvironmentVariable("DOTNET_TieredPGO", "1"), new EnvironmentVariable("DOTNET_ReadyToRun", "0"))
            //    .WithId("PGO enabled"));

            //AddJob(Job.Default.WithNuGet("MongoDB.Driver", "2.13.1"));
            //AddJob(Job.Default.WithNuGet("MongoDB.Driver", "2.18.0"));
            //AddJob(Job.Default.WithNuGet("MongoDB.Driver", "2.21.0"));
        }
    }
}
