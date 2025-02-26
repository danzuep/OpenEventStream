using System.Diagnostics.CodeAnalysis;

namespace OpenEventStream.Abstractions
{
    public interface ITimeSeriesBroker<T> : IEnumerable<ITimeSeries<T>>
    {
        ITimeSeries<T> GetOrCreate();
        ITimeSeries<T> GetOrCreate(string topic);
        void Add(T value);
        void Add(T value, string topic);
        IList<T> RemoveExpired();
        IList<T> RemoveExpired(TimeSpan threshold);
        IList<T> RemoveExpired(TimeSpan threshold, string topic);
        bool TryGetTimeSeries(string topic, [MaybeNullWhen(false)] out ITimeSeries<T> timeSeries);
    }
}