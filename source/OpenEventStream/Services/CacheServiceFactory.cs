namespace OpenEventStream.Services;

using OpenEventStream.Abstractions;
using OpenEventStream.Models;

public sealed class CacheServiceFactory : ICacheServiceFactory
{
    private readonly ITimestampProvider? _timestampProvider;
    private readonly CacheOptions? _cacheOptions;
    private readonly ITimedLock? _timedLock;

    public CacheServiceFactory(CacheOptions? cacheOptions = null, ITimedLock ? timedLock = null, ITimestampProvider? timestampProvider = null)
    {
        _cacheOptions = cacheOptions ?? new CacheOptions();
        _timedLock = timedLock ?? new TimedLock();
        _timestampProvider = timestampProvider ?? new SystemUtcTicks();
    }

    public ICacheService<T> Create<T>(CacheOptions? cacheOptions = null)
    {
        cacheOptions ??= _cacheOptions;
        return new CacheService<T>(cacheOptions, _timedLock, _timestampProvider);
    }
}
