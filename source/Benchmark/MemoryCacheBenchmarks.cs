// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// <see href="https://github.com/dotnet/performance/blob/main/src/benchmarks/micro/libraries/Microsoft.Extensions.Caching.Memory/MemoryCacheTests.cs"/>

using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;

namespace Benchmark
{
    public class MemoryCacheBenchmarks
    {
        private readonly MemoryCache _memCache = new MemoryCache(new MemoryCacheOptions());

        [GlobalSetup(Targets = new[] { nameof(GetHit), nameof(TryGetValueHit), nameof(GetMiss), nameof(TryGetValueMiss), nameof(SetOverride), nameof(CreateEntry) })]
        public void SetupBasic()
        {
            for (var i = 0; i < 1024; i++)
            {
                _memCache.Set(i, i.ToString());
            }
        }

        [GlobalCleanup(Targets = new[] { nameof(GetHit), nameof(TryGetValueHit), nameof(GetMiss), nameof(TryGetValueMiss), nameof(SetOverride), nameof(CreateEntry) })]
        public void CleanupBasic() => _memCache.Dispose();

        [Benchmark]
        public object? GetHit() => _memCache.Get("256");

        [Benchmark]
        public bool TryGetValueHit() => _memCache.TryGetValue("256", out _);

        [Benchmark]
        public object? GetMiss() => _memCache.Get("-1");

        [Benchmark]
        public bool TryGetValueMiss() => _memCache.TryGetValue("-1", out _);

        [Benchmark]
        public object SetOverride() => _memCache.Set("512", "512");

        [Benchmark]
        public void CreateEntry() => _memCache.CreateEntry(this).Dispose();
    }
}