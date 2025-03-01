namespace OpenEventStream.Abstractions
{
    public interface ITimeSeries<T> : IEnumerable<KeyValuePair<long, T>>
    {
        bool TryAdd(T value, TimeSpan? lockTimeout = null);
        IList<T> RemoveExpired(TimeSpan? expiryThreshold = null, TimeSpan? lockTimeout = null);
        IReadOnlyDictionary<long, T> ToReadOnlyDictionary();
    }
}