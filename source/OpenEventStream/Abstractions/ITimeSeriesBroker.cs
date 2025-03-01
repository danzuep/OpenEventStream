using System.Diagnostics.CodeAnalysis;

namespace OpenEventStream.Abstractions
{
    public interface ITimeSeriesBroker<T> : IEnumerable<ITimeSeries<T>>
    {
        ITimeSeries<T> GetOrCreate(string? topic = null);
        bool TryAdd(T value);
        bool TryAdd(string? topic, T value);
        IList<T> RemoveExpired(TimeSpan expiryThreshold);
        IList<T> RemoveExpired(string? topic = null, TimeSpan? expiryThreshold = null, TimeSpan? lockTimeout = null);
        bool TryGetTimeSeries(string topic, [MaybeNullWhen(false)] out ITimeSeries<T> timeSeries);
    }
}