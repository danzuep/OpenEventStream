namespace OpenEventStream;

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using OpenEventStream.Abstractions;

public sealed class TimeSeries<T> : ITimeSeries<T>
{
    private readonly ConcurrentQueue<KeyValuePair<DateTimeOffset, T>> _records =
        new ConcurrentQueue<KeyValuePair<DateTimeOffset, T>>();

    public void Add(T value)
    {
        _records.Enqueue(new KeyValuePair<DateTimeOffset, T>(DateTimeOffset.Now, value));
    }

    public IList<T> RemoveExpired()
    {
        return RemoveExpired(TimeSpan.Zero);
    }

    public IList<T> RemoveExpired(TimeSpan threshold)
    {
        var removedRecords = new List<T>();

        while (_records.TryPeek(out var oldestRecord) &&
            oldestRecord.Key < DateTimeOffset.Now - threshold &&
            _records.TryDequeue(out var removedRecord))
        {
            removedRecords.Add(removedRecord.Value);
        }

        return removedRecords;
    }

    public IReadOnlyDictionary<DateTimeOffset, T> ToReadOnlyDictionary()
    {
        return _records.ToImmutableDictionary();
    }

    public IEnumerator<KeyValuePair<DateTimeOffset, T>> GetEnumerator()
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