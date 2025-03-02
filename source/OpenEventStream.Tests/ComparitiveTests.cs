using Microsoft.Extensions.Caching.Memory;
using ZiggyCreatures.Caching.Fusion;

namespace OpenEventStream.Tests;

public class ComparitiveTests
{
    private readonly int _iterations = 1024 * 1024;

    [Fact]
    public void CacheService()
    {
        var cache = new CacheService<string>();
        for (var i = 0; i < _iterations; i++)
        {
            var kv = i.ToString();
            cache.GetOrAdd(kv, _ => kv);
        }
        cache.Dispose();
    }

    [Fact]
    public void MemoryCache()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        for (var i = 0; i < _iterations; i++)
        {
            var kv = i.ToString();
            cache.Set(kv, kv);
        }
        cache.Dispose();
    }

    [Fact]
    public void FusionCache()
    {
        var cache = new FusionCache(new FusionCacheOptions());
        for (var i = 0; i < _iterations; i++)
        {
            var kv = i.ToString();
            cache.Set(kv, kv);
        }
        cache.Dispose();
    }
}
