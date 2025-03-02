namespace OpenEventStream.Services;

using OpenEventStream.Abstractions;

public sealed class TimeSeriesFactory : ITimeSeriesFactory
{
    private readonly ITimestampProvider? _timestampProvider;
    private readonly ITimedLock? _timedLock;

    public TimeSeriesFactory(ITimedLock? timedLock = null, ITimestampProvider? timestampProvider = null)
    {
        _timedLock = timedLock ?? new TimedLock();
        _timestampProvider = timestampProvider ?? new SystemUtcTicks();
    }

    public ITimeSeries<T> Create<T>()
    {
        return new TimeSeries<T>(_timedLock, _timestampProvider);
    }
}
