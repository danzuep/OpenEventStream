namespace OpenEventStream.Services;

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using OpenEventStream.Abstractions;

public sealed class TimeSeries<T> : ITimeSeries<T>
{
    private readonly ConcurrentQueue<KeyValuePair<long, T>> _records =
        new ConcurrentQueue<KeyValuePair<long, T>>();

    private readonly ITimedLock _timedLock;
    private readonly ITimestampProvider _timestampProvider;

    public TimeSeries(ITimedLock? timedLock = null, ITimestampProvider? timestampProvider = null)
    {
        _timedLock = timedLock ?? new TimedLock();
        _timestampProvider = timestampProvider ?? new SystemUtcTicks();
    }

    public bool TryAdd(T value, TimeSpan? lockTimeout = null)
    {
        var record = new KeyValuePair<long, T>(_timestampProvider.Ticks, value);
        return _timedLock.TryExecute(() => _records.Enqueue(record), lockTimeout);
    }

    public IList<T> RemoveExpired(TimeSpan? expiryThreshold = null, TimeSpan? lockTimeout = null)
    {
        var removedRecords = new List<T>();
        expiryThreshold ??= TimeSpan.Zero;
        var expirationTicks = _timestampProvider.Ticks - expiryThreshold.Value.Ticks;
        _timedLock.TryExecute(() =>
        {
            while (_records.TryPeek(out var oldestRecord) &&
                oldestRecord.Key < expirationTicks &&
                _records.TryDequeue(out var removedRecord))
            {
                removedRecords.Add(removedRecord.Value);
            }
        }, lockTimeout);

        return removedRecords;
    }

    public IReadOnlyDictionary<long, T> ToReadOnlyDictionary()
    {
        return _records.ToImmutableDictionary();
    }

    public IEnumerator<KeyValuePair<long, T>> GetEnumerator()
    {
        foreach (var record in _records)
        {
            yield return record;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}