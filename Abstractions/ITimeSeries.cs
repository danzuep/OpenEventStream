namespace OpenEventStream.Abstractions;

public interface ITimeSeries<T> : IEnumerable<KeyValuePair<DateTimeOffset, T>>
{
    void Add(T value);
    IList<T> RemoveExpired();
    IList<T> RemoveExpired(TimeSpan threshold);
    IReadOnlyDictionary<DateTimeOffset, T> ToReadOnlyDictionary();
}