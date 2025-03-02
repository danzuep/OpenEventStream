using BenchmarkDotNet.Attributes;
using OpenEventStream.Models;
using OpenEventStream.Services;

namespace Benchmark
{
    public class CacheServiceBenchmarks
    {
        private readonly CacheService<string> _cache = new CacheService<string>(new CacheOptions());

        [GlobalSetup(Targets = new[] { nameof(GetHit), nameof(GetMiss) })]
        public void SetupBasic()
        {
            for (var i = 0; i < 1024; i++)
            {
                var kv = i.ToString();
                _cache.Set(kv, kv);
            }
        }

        [GlobalCleanup(Targets = new[] { nameof(GetHit), nameof(GetMiss) })]
        public void CleanupBasic() => _cache.Dispose();

        [Benchmark]
        public object? GetHit() => _cache.Get("256");

        [Benchmark]
        public bool TryGetValueHit() => _cache.TryGetValue("256", out _);

        [Benchmark]
        public object? GetMiss() => _cache.Get("-1");

        [Benchmark]
        public bool TryGetValueMiss() => _cache.TryGetValue("-1", out _);

        [Benchmark]
        public void SetOverride() => _cache.Set("512", "512");

        [Benchmark]
        public void CreateEntry() => _cache.GetOrAdd("256", "256");
    }
}
