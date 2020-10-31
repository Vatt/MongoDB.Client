using BenchmarkDotNet.Running;

namespace MongoDB.Client.Benchmarks
{
    class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new BenchmarkDotNet.Configs.DebugInProcessConfig());
#else
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
#endif
        }
    }
}