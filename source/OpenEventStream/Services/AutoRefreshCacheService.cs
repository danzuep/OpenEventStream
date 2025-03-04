namespace OpenEventStream.Services;

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenEventStream.Abstractions;
using OpenEventStream.Models;

public sealed class AutoRefreshCacheService<T> : IEnumerable<CacheEntry<T>>
{
    private long _lastExpired;
    private long _lastUpdated;
    private readonly Timer _updater;
    private readonly CacheOptions _cacheOptions;
    private readonly ConcurrentQueue<CacheEntry<T>> _cache = new();

    private readonly ITimestampProvider _timestampProvider;

    public AutoRefreshCacheService(CacheOptions? cacheOptions = null, ITimestampProvider? timestampProvider = null)
    {
        _timestampProvider = timestampProvider ?? new SystemUtcTicks();
        _cacheOptions = cacheOptions ?? new CacheOptions();
        _lastExpired = _timestampProvider.Ticks;
        _lastUpdated = _timestampProvider.Ticks;
        _updater = new Timer(Scan, null, _cacheOptions.Timeout, _cacheOptions.Timeout);
    }

    public int Count => _cache.Count;

    public T? Add(object key, Func<T> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        var cacheEntry = new CacheEntry<T>(key, valueFactory, _timestampProvider);
        _cache.Enqueue(cacheEntry);
        return cacheEntry.Value;
    }

    public void UpdateInterval(TimeSpan interval)
    {
        _updater.Change(interval, interval);
    }

    private void Scan(object? state)
    {
        var expired = RemoveExpired().ToList();
        var updated = RefreshCache().ToList();
    }

    private IEnumerable<CacheEntry<T>> RefreshCache()
    {
        var slidingLimit = _timestampProvider.Ticks - _cacheOptions.Timeout.Ticks;
        if (slidingLimit > _lastUpdated)
        {
            _lastUpdated = _timestampProvider.Ticks;
            while (_cache.TryPeek(out var oldest) &&
                oldest.TryUpdate(out var cacheEntry, _cacheOptions.Timeout))
            {
                _cache.TryDequeue(out _);
                Add(oldest.Key, () => cacheEntry);
                yield return oldest;
            }
        }
    }

    private IEnumerable<CacheEntry<T>> RemoveExpired()
    {
        var slidingLimit = _timestampProvider.Ticks - _cacheOptions.Expiry.Ticks;
        if (slidingLimit > _lastExpired)
        {
            _lastExpired = _timestampProvider.Ticks;
            while (_cache.TryPeek(out var oldest) &&
                oldest.TryDispose(_cacheOptions.Expiry))
            {
                _cache.TryDequeue(out _);
                yield return oldest;
            }
        }
    }

    public IEnumerator<CacheEntry<T>> GetEnumerator()
    {
        foreach (var record in _cache)
        {
            yield return record;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
        _cache.Clear();
    }
}