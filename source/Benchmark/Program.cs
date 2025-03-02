using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // https://benchmarkdotnet.org/articles/configs/configs.html
            var config = DefaultConfig.Instance;
            var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAll();

            // Use this to select benchmarks from the console: .Run(args, config);
            //var summary = BenchmarkRunner.Run<CacheServiceBenchmarks>(config, args);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}