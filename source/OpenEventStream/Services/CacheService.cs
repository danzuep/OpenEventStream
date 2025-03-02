namespace OpenEventStream.Services;

using System.Collections;
using System.Collections.Concurrent;
using OpenEventStream.Abstractions;
using OpenEventStream.Models;

public sealed class CacheService<T> : ICacheService<T>
{
    private readonly CacheOptions _cacheOptions;
    private readonly ConcurrentDictionary<string, T> _cache =
        new ConcurrentDictionary<string, T>();
    private readonly ConcurrentQueue<KeyValuePair<long, string>> _expiry =
        new ConcurrentQueue<KeyValuePair<long, string>>();
    private readonly List<string> _expired = new List<string>();
    private KeyValuePair<long, string>? _recheck;
    private long _lastExpired;
    private object _lock = new object();

    private readonly ITimestampProvider _timestampProvider;
    private readonly ITimedLock _timedLock;

    public CacheService(CacheOptions? cacheOptions = null, ITimedLock? timedLock = null, ITimestampProvider? timestampProvider = null)
    {
        _timestampProvider = timestampProvider ?? new SystemUtcTicks();
        _timedLock = timedLock ?? new TimedLock();
        _cacheOptions = cacheOptions ?? new CacheOptions();
        _lastExpired = _timestampProvider.Ticks;
    }

    public T? GetOrAdd(string key, Func<string, T> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        var cacheKey = _cacheOptions.UseCompositeKey ?
            string.Join(_cacheOptions.Delimiter, typeof(T).Name, key) : key;
        var result = _cache.GetOrAdd(cacheKey, k => valueFactory(k));
        var lastUsed = new KeyValuePair<long, string>(_timestampProvider.Ticks, cacheKey);
        _expiry.Enqueue(lastUsed);
        CheckExpiryTimer();
        return result;
    }

    public T? GetOrAdd(string key, Func<T> valueFactory)
    {
        return GetOrAdd(key, _ => valueFactory());
    }

    public T? GetOrAdd(string key, T value)
    {
        return GetOrAdd(key, _ => value);
    }

    public bool TryGetValue(string key, out T? value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public T? Get(string key)
    {
        return _cache[key];
    }

    public void Set(string key, T value)
    {
        _cache[key] = value;
    }

    public void Remove(string key)
    {
        _cache.Remove(key, out _);
    }

    public int Count => _cache.Count;

    public IList<string> Flush(TimeSpan? expiryThreshold = null)
    {
        return RemoveExpired(expiryThreshold).ToList();
    }

    private void CheckExpiryTimer()
    {
        var expirationTicks = _lastExpired + _cacheOptions.Expiry.Ticks;
        if (_timestampProvider.Ticks > expirationTicks)
        {
            _ = Flush(TimeSpan.Zero);
        }
    }

    private IEnumerable<string> RemoveExpired(TimeSpan? expiryThreshold = null)
    {
        expiryThreshold ??= _cacheOptions.Expiry;
        foreach (var key in GetExpiredKeys(expiryThreshold.Value))
        {
            if (_cache.TryRemove(key, out _))
            {
                yield return key;
            }
            else
            {
                _expired.Add(key);
            }
        }
        foreach (var key in _expired)
        {
            if (_cache.TryRemove(key, out _))
            {
                yield return key;
            }
        }
    }

    private IEnumerable<string> GetExpiredKeys(TimeSpan expiryThreshold)
    {
        KeyValuePair<long, string> oldest;
        string? expiredValue = null;
        var expirationTicks = _timestampProvider.Ticks - expiryThreshold.Ticks;

        _timedLock.TryExecute(() =>
        {
            if (_recheck.HasValue && _recheck.Value.Key < expirationTicks)
            {
                expiredValue = _recheck.Value.Value;
                _recheck = null;
            }
        }, _cacheOptions.Timeout, _lock);

        if (expiredValue is not null)
        {
            yield return expiredValue;
        }

        while (_expiry.TryDequeue(out oldest) && oldest.Key < expirationTicks)
        {
            yield return oldest.Value;
        }

        _timedLock.TryExecute(() => _recheck = oldest, _cacheOptions.Timeout, _lock);
    }

    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
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
        _expiry.Clear();
        _expired.Clear();
        _recheck = null;
    }
}