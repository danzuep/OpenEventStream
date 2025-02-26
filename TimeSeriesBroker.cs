namespace OpenEventStream;

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

    public ITimeSeries<T> GetOrCreate()
    {
        return GetOrCreate(_options.DefaultTopic);
    }

    public ITimeSeries<T> GetOrCreate(string topic)
    {
        return _timeSeriesCollection.GetOrAdd(topic, new TimeSeries<T>());
    }

    public void Add(T value)
    {
        Add(value, _options.DefaultTopic);
    }

    public void Add(T value, string topic)
    {
        var timeSeries = GetOrCreate(topic);
        timeSeries.Add(value);
    }

    public IList<T> RemoveExpired()
    {
        return RemoveExpired(TimeSpan.Zero, _options.DefaultTopic);
    }

    public IList<T> RemoveExpired(TimeSpan threshold)
    {
        return RemoveExpired(threshold, _options.DefaultTopic);
    }

    public IList<T> RemoveExpired(TimeSpan threshold, string topic)
    {
        if (_timeSeriesCollection.TryGetValue(topic, out var timeSeries))
        {
            return timeSeries.RemoveExpired(threshold);
        }
        return Array.Empty<T>();
    }

    public bool TryGetTimeSeries(string topic, [MaybeNullWhen(false)] out ITimeSeries<T> timeSeries)
    {
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