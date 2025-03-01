namespace OpenEventStream.Services;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenEventStream.Abstractions;
using OpenEventStream.Models;

public sealed class TimeSeriesBroker<T> : ITimeSeriesBroker<T>
{
    private readonly ConcurrentDictionary<string, ITimeSeries<T>> _timeSeriesCollection;
    private readonly TimeSeriesOptions _options;

    public TimeSeriesBroker(TimeSeriesOptions options)
    {
        _options = options;
        _timeSeriesCollection = new ConcurrentDictionary<string, ITimeSeries<T>>();
        _timeSeriesCollection.TryAdd(_options.DefaultTopic, new TimeSeries<T>());
    }

    public ITimeSeries<T> GetOrCreate(string? topic = null)
    {
        topic ??= _options.DefaultTopic;
        return _timeSeriesCollection.GetOrAdd(topic, new TimeSeries<T>());
    }

    public bool TryAdd(T value)
    {
        return TryAdd(null, value);
    }

    public bool TryAdd(string? topic, T value)
    {
        var timeSeries = GetOrCreate(topic);
        return timeSeries.TryAdd(value);
    }

    public IList<T> RemoveExpired(TimeSpan expiryThreshold)
    {
        return RemoveExpired(null, expiryThreshold, null);
    }

    public IList<T> RemoveExpired(string? topic = null, TimeSpan? expiryThreshold = null, TimeSpan? lockTimeout = null)
    {
        topic ??= _options.DefaultTopic;
        if (_timeSeriesCollection.TryGetValue(topic, out var timeSeries))
        {
            return timeSeries.RemoveExpired(expiryThreshold, lockTimeout);
        }
        return Array.Empty<T>();
    }

    public IReadOnlyList<T> GetAllRecords(string? topic = null)
    {
        if (TryGetTimeSeries(topic, out var partition))
        {
            return partition.Select(p => p.Value).ToArray();
        }
        return this.SelectMany(b => b.Select(p => p.Value)).ToArray();
    }

    public bool TryGetTimeSeries(string? topic, [MaybeNullWhen(false)] out ITimeSeries<T> timeSeries)
    {
        topic ??= _options.DefaultTopic;
        return _timeSeriesCollection.TryGetValue(topic, out timeSeries);
    }

    public IEnumerator<ITimeSeries<T>> GetEnumerator()
    {
        foreach (var keyValuePair in _timeSeriesCollection)
        {
            if (TryGetTimeSeries(keyValuePair.Key, out var timeSeries))
            {
                yield return timeSeries;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}