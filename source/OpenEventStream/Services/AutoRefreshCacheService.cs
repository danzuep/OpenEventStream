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
    private readonly ConcurrentQueue<KeyValuePair<long, CacheEntry<T>>> _cache = new();

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
        _cache.Enqueue(new(_timestampProvider.Ticks, cacheEntry));
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
        return ProcessCacheEntries(update: true, (entry, expiration) => entry.TryUpdate(expiration));
    }

    private IEnumerable<CacheEntry<T>> RemoveExpired()
    {
        return ProcessCacheEntries(update: false, (entry, expiration) => entry.TryDispose(expiration));
    }

    private IEnumerable<CacheEntry<T>> ProcessCacheEntries(bool update, Func<CacheEntry<T>, TimeSpan, bool> processEntry)
    {
        var expiration = update ? _cacheOptions.Timeout : _cacheOptions.Expiry;
        var slidingLimit = _timestampProvider.Ticks - expiration.Ticks;
        if (slidingLimit > (update ? _lastUpdated : _lastExpired))
        {
            if (update)
            {
                _lastUpdated = _timestampProvider.Ticks;
            }
            else
            {
                _lastExpired = _timestampProvider.Ticks;
            }
            KeyValuePair<long, CacheEntry<T>> oldest;
            while (_cache.TryDequeue(out oldest) && slidingLimit > oldest.Key && processEntry(oldest.Value, expiration))
            {
                if (update && !_cache.IsEmpty)
                {
                    _cache.Enqueue(new(_timestampProvider.Ticks, oldest.Value));
                }
                yield return oldest.Value;
            }
            if (!update && oldest.Value.TryUpdate(expiration)) { }
            _cache.Enqueue(new(_timestampProvider.Ticks, oldest.Value));
        }
    }

    public IEnumerator<CacheEntry<T>> GetEnumerator()
    {
        foreach (var record in _cache)
        {
            yield return record.Value;
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