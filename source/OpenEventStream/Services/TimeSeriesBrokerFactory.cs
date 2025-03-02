namespace OpenEventStream.Services;

using OpenEventStream.Abstractions;
using OpenEventStream.Models;

public sealed class TimeSeriesBrokerFactory : ITimeSeriesBrokerFactory
{
    private readonly TimeSeriesOptions _options;
    private readonly ITimeSeriesFactory _timeSeriesFactory;

    public TimeSeriesBrokerFactory(TimeSeriesOptions? options = null, ITimeSeriesFactory? timeSeriesFactory = null)
    {
        _options = options ?? new TimeSeriesOptions();
        _timeSeriesFactory = timeSeriesFactory ?? new TimeSeriesFactory();
    }

    public ITimeSeriesBroker<T> Create<T>()
    {
        return new TimeSeriesBroker<T>(_options, _timeSeriesFactory);
    }
}
